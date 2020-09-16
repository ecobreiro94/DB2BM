﻿using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Newtonsoft.Json;
using System.Linq.Expressions;
using DB2BM.Utils;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions;
using DB2BM.Abstractions.Attrs;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Entities.UserDefined;
using DB2BM.Extensions.AnsiCatalog.Entities;
using DB2BM.Extensions.PgSql.Entities;
using DB2BM.Extensions.AnsiCatalog;

namespace DB2BM.Extensions.PgSql
{
    [Dbms("postgre")]
    public class PostgreCatalogHandler : AnsiCatalogHandler<PostgreDbContext>
    {
        protected override string GetConnectionString(DbOption options)
        {
            return $"Host={options.Host};Username={options.User};Password={options.Password};Database={options.DataBaseName};SSL Mode=Prefer";
        }

        protected override IEnumerable<StoredProcedure> GetFunctions(bool internals)
        {
            var filter = internals ?
                (Expression<Func<AnsiRoutine, bool>>)(f => f.SpecificSchema == "pg_catalog") :
                (Expression<Func<AnsiRoutine, bool>>)
                    (f => f.SpecificSchema == "public" && (f.RoutineType == "FUNCTION" || f.RoutineType == "PROCEDURE") &&
                          (f.RoutineLanguage == "SQL" || f.ExternalLanguage == "PLPGSQL") &&
                          f.ReturnClause != "trigger");

            var functs = InternalDbContext.Routines
                .FromSqlRaw(
                   @"
                      select 
                        specific_name,
                        routine_name,
                        routine_schema,
                        routine_type,
                        data_type,
                        type_udt_name,
                        PG_GET_FUNCTION_RESULT(CAST(REPLACE(specific_name, CONCAT(routine_name, '_'), '') AS INT)) AS return_clause,
                        routine_definition,
                        routine_body,
                        external_language
                      from
                        information_schema.routines
                   ")
                .Include(x => x.Params)
                .Where(filter)
                .AsEnumerable()
                .Select(f =>
                    new StoredProcedure()
                    {
                        Name = f.Name,
                        SpecificName = f.SpecificName,
                        LanguageDefinition = f.RoutineLanguage == "EXTERNAL" ? f.ExternalLanguage : f.RoutineLanguage,
                        OriginalCode = f.RoutineLanguage == "SQL" ? 
                            $"BEGIN {f.Definition} END" : 
                            f.Definition,
                        ReturnIsSet = f.ReturnClause == "setof",
                        ReturnType = f.ReturnUdtType,
                        Params = f.Params.Select(p =>
                              new Parameter()
                              {
                                  Name = p.Name,
                                  OriginType = p.UdtName,
                                  OrdinalPosition = p.OrdinalPosition,
                                  IsResult = p.IsResult,
                                  ParameterMode = (ParameterMode)Enum.Parse(typeof(ParameterMode), p.ParameterMode.Replace(" ", ""), true)
                              }).ToList()
                    });
            return functs;
        }

        protected override IEnumerable<Table> GetTables()
        {
            var tables =
                InternalDbContext.Tables
                    .Include(x => x.Fields)
                    .Where(t => t.SchemaName == "public")
                    .Select(t =>
                       new Table()
                       {
                           Name = t.Name,
                           Fields = t.Fields
                               .OrderBy(f => f.OrdinalPosition)
                               .Select(f =>
                                     new TableField()
                                     {
                                         GenName = (t.Name == f.Name) ? "_" + f.Name.ToPascal() : f.Name.ToPascal(),
                                         Name = f.Name,
                                         IsNullable = (f.IsNullable == "YES") ? true : false,
                                         OrdinalPosition = f.OrdinalPosition,
                                         OriginType = f.UdtName,
                                         Default = f.Default,
                                         CharacterMaximumLength = f.CharacterMaximumLength,
                                     }).ToList()
                       });

            return tables;
        }

        protected override IEnumerable<Relationship> GetRelations(IDictionary<string, Table> tables)
        {
            var relations = InternalDbContext.Relationships
                                .Include(x => x.KeyColumn)
                                .Include(x => x.RelationColumn)
                                .Where(r => r.SchemaName == "public" && r.ConstraintType != "CHECK")
                                .AsEnumerable()
                                .Select(r =>
                                    new Relationship()
                                    {
                                        Table = tables[r.TableName],
                                        ReferenceTable = tables[r.RelationColumn.TableName],
                                        Column = tables[r.TableName].Fields.First(c => c.Name == r.KeyColumn.ColumnName),
                                        ReferenceColumn = tables[r.RelationColumn.TableName].Fields.First(c => c.Name == r.RelationColumn.ColumnName),
                                        Type = (r.ConstraintType == "PRIMARY KEY") ? RelationshipType.PrimaryKey : RelationshipType.ForeingKey
                                    });
            return relations;
        }

        protected override IEnumerable<BaseUserDefinedType> GetUserDefinedTypes()
        {
            IEnumerable<BaseUserDefinedType> udts = InternalDbContext.UDTs
                .Include(x => x.Fields)
                .Where(x => x.Schema == "public" && x.Category == "STRUCTURED")
                .AsEnumerable()
                .Select(x =>
                    new UserDefinedType()
                    {
                        TypeName = x.Name,
                        Fields = x.Fields.Select(f =>
                                   new UdtField()
                                   {
                                       Name = f.Name,
                                       OriginType = f.UdtName,
                                       OrdinalPosition = f.OrdinalPosition,
                                       Default = f.Default,
                                       IsNullable = (f.IsNullable == "YES") ? true : false,
                                       CharacterMaximumLength = f.CharacterMaximumLength
                                   })
                    });

            IEnumerable<BaseUserDefinedType> enumsOptions =
                InternalDbContext.Set<PostgreUDEnumOption>()
                   .FromSqlRaw(@"
                                SELECT 
                                    pg_type.typname AS EnumName, 
                                    pg_enum.enumlabel AS Option
                                FROM 
                                    pg_type 
                                    JOIN pg_enum ON pg_enum.enumtypid = pg_type.oid
                            ")
                   .AsEnumerable()
                   .GroupBy(x => x.EnumName)
                   .Select(x =>
                       new UserDefinedEnumType()
                       {
                           TypeName = x.Key,
                           Options = x.Select(o => o.Option)
                       });
            return udts.Union(enumsOptions);
        }

        protected override IEnumerable<Sequence> GetSequences()
        {
            return InternalDbContext.Sequences.Select(x =>
                   new Sequence()
                   {
                       Name = x.Name,
                       Increment = int.Parse(x.Increment),
                       Start = int.Parse(x.Start),
                       MinValue = int.Parse(x.MinValue),
                       MaxValue = long.Parse(x.MaxValue)
                   });
        }

    }
}
