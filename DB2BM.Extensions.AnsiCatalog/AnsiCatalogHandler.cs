using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq.Expressions;
using DB2BM.Abstractions;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Entities.UserDefined;
using DB2BM.Extensions.AnsiCatalog.Entities;
using DB2BM.Utils;

namespace DB2BM.Extensions.AnsiCatalog
{
    public abstract class AnsiCatalogHandler<TDbContext> : ICatalogHandler
        where TDbContext : AnsiCatalogDbContext, new()
    {
        private DatabaseCatalog catalog;

        private TDbContext internalDbContext;

        public abstract string SchemaName { get; }

        public TDbContext InternalDbContext
        {
            get
            {
                if (internalDbContext == null)
                {
                    var connectionString = GetConnectionString(Options);
                    internalDbContext = new TDbContext()
                    {
                        ConnectionString = connectionString
                    };
                }
                return internalDbContext;
            }
        }

        public DbOption Options { get; set; }

        protected abstract string GetConnectionString(DbOption options);

        protected virtual IEnumerable<StoredProcedure> GetFunctions(bool internals)
        {
            if (internals)
                return Enumerable.Empty<StoredProcedure>();

            var functs = InternalDbContext.Routines
                .Include(x => x.Params)
                .Where(f => f.SpecificSchema == SchemaName)
                .AsEnumerable()
                .Select(f =>
                    new StoredProcedure()
                    {
                        Name = f.Name,
                        SpecificName = f.SpecificName,
                        LanguageDefinition = f.RoutineLanguage,
                        OriginalCode = f.Definition,
                        ReturnType = f.ReturnUdtType ?? f.ReturnDataType,
                        Params = f.Params.Select(p =>
                              new Parameter()
                              {
                                  Name = p.Name,
                                  OriginType = p.UdtName ?? p.DataTypeName,
                                  OrdinalPosition = p.OrdinalPosition,
                                  IsResult = p.IsResult,
                                  ParameterMode = (ParameterMode)Enum.Parse(typeof(ParameterMode), p.ParameterMode.Replace(" ", ""), true)
                              }).ToList()
                    });
            return functs;

        }

        protected virtual IEnumerable<Table> GetTables()
        {
            var tables =
                InternalDbContext.Tables
                    .Include(x => x.Fields)
                    .Where(t => t.SchemaName == SchemaName)
                    .AsEnumerable()
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
                                         OriginType = f.UdtName ?? f.DataTypeName,
                                         Default = f.Default,
                                         CharacterMaximumLength = f.CharacterMaximumLength,
                                     }).ToList()
                       });

            return tables;
        }

        protected virtual IEnumerable<Relationship> GetRelations(IDictionary<string, Table> tables)

        {
            var relations = InternalDbContext.Relationships
                                .Include(x => x.KeyColumn)
                                .Include(x => x.RelationColumn)
                                .Where(r => r.SchemaName == SchemaName && r.ConstraintType != "CHECK")
                                .AsEnumerable()
                                .Select(r =>
                                    new Relationship()
                                    {
                                        Table = tables[r.TableName],
                                        ReferencedTable = tables[r.RelationColumn.TableName],
                                        Column = tables[r.TableName].Fields.First(c => c.Name == r.KeyColumn.ColumnName),
                                        ReferencedColumn = tables[r.RelationColumn.TableName].Fields.First(c => c.Name == r.RelationColumn.ColumnName),
                                        Type = (r.ConstraintType == "PRIMARY KEY") ? RelationshipType.PrimaryKey : RelationshipType.ForeingKey
                                    });
            return relations;
        }

        protected virtual IEnumerable<Sequence> GetSequences()
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

        protected virtual IEnumerable<BaseUserDefinedType> GetUserDefinedTypes()
        {
            return Enumerable.Empty<BaseUserDefinedType>();
        }

        public DatabaseCatalog GetCatalog()
        {
            if (this.catalog != null)
                return this.catalog;

            catalog = new DatabaseCatalog()
            {
                Name = Options.DataBaseName,
            };

            var tables = GetTables();
            var catalogTables = new Dictionary<string, Table>();
            foreach (var t in tables)
                catalogTables.Add(t.Name, t);

            var relations = GetRelations(catalogTables).ToList();

            var functions = GetFunctions(false);
            var catalogFunctions = new Dictionary<string, StoredProcedure>();
            foreach (var f in functions)
                catalogFunctions.Add(f.SpecificName, f);

            var internalFunctions = GetFunctions(true);
            var catalogInternalFunctions = new Dictionary<string, StoredProcedure>();
            foreach (var f in internalFunctions)
                catalogInternalFunctions.Add(f.SpecificName, f);

            var userDefineds = GetUserDefinedTypes();
            var catalogUserDefinedType = new Dictionary<string, BaseUserDefinedType>();
            foreach (var u in userDefineds)
                catalogUserDefinedType.Add(u.TypeName, u);

            var catalogSequences = new Dictionary<string, Sequence>();
            foreach (var s in GetSequences())
                catalogSequences.Add(s.Name, s);

            catalog.StoredProcedures = catalogFunctions;
            catalog.InternalFunctions = catalogInternalFunctions;
            catalog.Relationships = relations;
            catalog.Sequences = catalogSequences;
            catalog.Tables = catalogTables;
            catalog.UserDefinedTypes = catalogUserDefinedType;

            return catalog;
        }
    }
}
