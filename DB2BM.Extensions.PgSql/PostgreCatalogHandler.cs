using System.Linq;
using System.Collections.Generic;
using System;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions;
using DB2BM.Abstractions.Attrs;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Entities.UserDefined;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Newtonsoft.Json;
using DB2BM.Utils;

namespace DB2BM.Extensions.PgSql
{
    [DbmsAttr("postgre")]
    public class PostgreCatalogHandler : ICatalogHandler
    {
        PostgreDbContext internalDbContext;
        public PostgreDbContext InternalDbContext
        {
            get
            {
                if (internalDbContext == null)
                {
                    var connectionString = GetConnectionString(Options);
                    internalDbContext = new PostgreDbContext(PostgreDbContext.GetOptions(connectionString), connectionString);
                }
                return internalDbContext;
            }
        }

        public DbOption Options { get; set; }

        DatabaseCatalog Catalog;

        private string GetConnectionString(DbOption options)
        {
            return "Host=" + options.Host + ";Username=" + options.User + ";Password=" + options.Password + ";Database=" + options.DataBaseName;
        }
        
        private IEnumerable<StoreProcedure> GetFunctions()
        {
            var functs = InternalDbContext.Functions
                .FromSqlRaw(
                   @"
                      select 
                        specific_name,
                        routine_name,
                        routine_schema,
                        routine_type,
                        type_udt_name,
                        PG_GET_FUNCTION_RESULT(CAST(REPLACE(specific_name, CONCAT(routine_name, '_'), '') AS INT)) AS return_clause,
                        routine_definition,
                        external_language
                      from
                        information_schema.routines
                   ")
                .Include(x => x.Params)
                .Where(
                    f => f.SpecificSchema == "public" && f.FunctionType == "FUNCTION"  &&
                        (f.LanguageDefinition == "SQL" || f.LanguageDefinition == "PLPGSQL") && 
                        f.ReturnType != "trigger");

            foreach (var f in functs)
            {
                string definition = f.Definition;
                if (f.LanguageDefinition == "SQL")
                    definition = "BEGIN " + definition;
                
                var function = new StoreProcedure()
                {
                    Name = f.Name,
                    SpecificName = f.SpecificName,
                    LangageDefinition = f.LanguageDefinition,
                    PLDefinition = definition,
                    ReturnClause = f.ReturnClause,
                    ReturnType = f.ReturnType
                };
                    
                var parms = new List<Parameter>();
                var parameters = f.Params.ToList();
                
                foreach (var p in parameters)
                    parms.Add(new Parameter()
                    {
                        Name = p.Name,
                        OriginType = p.TypeName,
                        OrdinalPosition = p.OrdinalPosition,
                        IsResult = p.IsResult,
                        ParameterMode = (ParameterMode)Enum.Parse(typeof(ParameterMode), p.ParameterMode.Replace(" ", ""), true),
                    });

                function.Params = parms;
                yield return function;
            }
        }

        private IEnumerable<StoreProcedure> GetInternalFunctions()
        {
            var functs = InternalDbContext.Functions
               .FromSqlRaw(
                  @"
                      select 
                        specific_name,
                        routine_name,
                        routine_schema,
                        routine_type,
                        type_udt_name,
                        PG_GET_FUNCTION_RESULT(CAST(REPLACE(specific_name, CONCAT(routine_name, '_'), '') AS INT)) AS return_clause,
                        routine_definition,
                        external_language
                      from
                        information_schema.routines
                   ")
               .Include(x => x.Params)
               .Where(f => f.SpecificSchema == "pg_catalog");

            foreach (var f in functs)
            {
                var function = new StoreProcedure()
                {
                    Name = f.Name,
                    SpecificName = f.SpecificName,
                    ReturnType = f.ReturnType,
                    IsInternal = true
                };

                var parms = new List<Parameter>();
                var parameters = f.Params.ToList();

                foreach (var p in parameters)
                    parms.Add(new Parameter()
                    {
                        Name = p.Name,
                        OriginType = p.TypeName,
                        OrdinalPosition = p.OrdinalPosition,
                        IsResult = p.IsResult,
                        ParameterMode = (ParameterMode)Enum.Parse(typeof(ParameterMode), p.ParameterMode.Replace(" ", ""), true),
                    });

                function.Params = parms;
                yield return function;
            }
        }

        private bool ValueType(string type)
        {
            if (type == "int" ||
                type == "long" ||
                type == "uint" ||
                type == "ulong" ||
                type == "long" ||
                type == "short" ||
                type == "decimal" || 
                type == "float" || 
                type == "double" ||
                type == "DateTime"||
                type == "char" ||
                type == "bool")
                return true;
            return false;
        }

        private IEnumerable<Table> GetTables()
        {
            var tables = InternalDbContext.Tables.Include(x => x.Fields).Where(t => t.SchemaName == "public").ToList();
            foreach (var t in tables)
            {
                var fields = t.Fields.ToList();
                var tableFields = new List<TableField>();
                var table = new Table() { Name = t.Name };
                foreach (var f in fields)
                {
                    var name = f.Name.ToPascal();
                    tableFields.Add(new TableField() {
                        GenName = (table.Name == f.Name) ? "_" + name : name,
                        Name = f.Name,
                        IsNullable = (f.IsNullable == "SI")? true: false,
                        OrdinalPosition = f.OrdinalPosition,
                        OriginType = f.TypeName,
                        Default = f.Default,
                        CharacterMaximumLength = f.CharacterMaximumLength,
                    });
                    
                }
                tableFields.Sort((x, y) => (x.OrdinalPosition <= y.OrdinalPosition) ? -1 : 1);
                table.Fields = tableFields;
                yield return table;
            }
        }

        private IEnumerable<Relationship> GetRelations(IEnumerable<Table> tables)
        {
            var relations = InternalDbContext.Relations.Include(x => x.KeyColumn)
                .Include(x => x.RelationColumn).Where(r => r.SchemaName == "public" && r.ConstraintType != "CHECK").ToList();
            foreach (var r in relations)
            {
                var table = tables.First(f => f.Name == r.TableName);
                var rTable = tables.First(f => f.Name == r.RelationColumn.TableName);
                yield return new Relationship()
                {
                    Table = table,
                    ReferenceTable = rTable,
                    Column = table.Fields.First(c => c.Name == r.KeyColumn.ColumnName),
                    ReferenceColumn = rTable.Fields.First(c => c.Name == r.RelationColumn.ColumnName),
                    Type = (r.ConstraintType == "PRIMARY KEY")? RelationshipType.PrimaryKey : RelationshipType.ForeingKey
                };
            }
        }

        private IEnumerable<BaseUserDefinedType> GetUserDefineds()
        {                                               
            var udts = InternalDbContext.UDTs.Include(x => x.Fields)
                .Where(x => x.Schema == "public" && x.Category == "STRUCTURED").ToList();
            foreach (var udt in udts)
            {
                var fs = udt.Fields.ToList();
                var udtfs = new List<UdtField>();
                foreach (var f in fs)
                {
                    udtfs.Add(new UdtField() {
                        Name = f.Name,
                        OriginType = f.TypeName,
                        OrdinalPosition = f.OrdinalPosition,
                        Default = f.Default,
                        IsNullable = (f.IsNullable == "SI") ? true : false,
                        CharacterMaximumLength = f.CharacterMaximumLength
                    });
                }
                yield return new UserDefinedType() { TypeName = udt.Name, Fields = udtfs };
            }

            var query = $"SELECT pg_type.typname AS EnumName, pg_enum.enumlabel AS Option " +
                $"FROM pg_type JOIN pg_enum ON pg_enum.enumtypid = pg_type.oid";
            var enumsOptions = InternalDbContext.Set<Entities.PostgreUDEnumOption>().FromSqlRaw(query).ToList();

            var enumNames = new List<string>();
            foreach (var o in enumsOptions)
                if (!enumNames.Contains(o.EnumName))enumNames.Add(o.EnumName);

            foreach (var en in enumNames)
            {
                var opts = new List<string>();
                foreach (var o in enumsOptions)
                {
                    if (o.EnumName == en)opts.Add(o.Option);
                }
                yield return new UserDefinedEnumType()
                {
                    TypeName = en,
                    Options = opts
                }; 
            }
        }

        private IEnumerable<Sequence> GetSequences()
        {
            var pSequences = InternalDbContext.Sequences.ToList();
            foreach (var s in pSequences)
            {
                var mv = long.Parse(s.MaxValue);
                yield return new Sequence()
                {
                    Name = s.Name,
                    Increment = int.Parse(s.Increment),
                    Start = int.Parse(s.Start),
                    MinValue = int.Parse(s.MinValue),
                    MaxValue = mv,
                    IsInt = mv <= Int32.MaxValue
                }; 
            }
        }

        public DatabaseCatalog GetCatalog()
        {
            if (Catalog != null) return Catalog;


            var catalog = new DatabaseCatalog()
            {
                Name = Options.DataBaseName,
            };

            var tables = GetTables().ToList();
            var functions = GetFunctions().ToList();
            var relations = GetRelations(tables).ToList();
            var userDefineds = GetUserDefineds().ToList();
            var internalFunctions = GetInternalFunctions().ToList();

            var catalogTables = new Dictionary<string, Table>();
            var catalogFunctions = new Dictionary<string, StoreProcedure>();
            var catalogInternalFunctions = new Dictionary<string, StoreProcedure>();
            var catalogUserDefinedType = new Dictionary<string, BaseUserDefinedType>();
            var catalogSequences = new Dictionary<string, Sequence>();

            foreach (var t in tables)
                catalogTables.Add(t.Name, t);

            foreach (var f in functions)
                catalogFunctions.Add(f.SpecificName, f);

            foreach (var f in internalFunctions)
                catalogInternalFunctions.Add(f.SpecificName, f);


            foreach (var u in userDefineds)
                catalogUserDefinedType.Add(u.TypeName, u);

            foreach (var s in GetSequences())
                catalogSequences.Add(s.Name, s);

            catalog.StoreProcedures = catalogFunctions;
            catalog.InternalFunctions = catalogInternalFunctions;
            catalog.Relationships = relations;
            catalog.Sequences = catalogSequences;
            catalog.Tables = catalogTables;
            catalog.UserDefinedTypes = catalogUserDefinedType;

            Utils.ReviewEntities.PushRelations(catalog);
            Catalog = catalog;
            return Catalog;
        }
    }
}
