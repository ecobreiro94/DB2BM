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
        where TDbContext: AnsiCatalogDbContext, new()
    {
        TDbContext internalDbContext;
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

        private DatabaseCatalog Catalog;

        protected abstract string GetConnectionString(DbOption options);

        protected abstract IEnumerable<StoredProcedure> GetFunctions(bool internals);

        protected abstract IEnumerable<Table> GetTables();

        protected abstract IEnumerable<Relationship> GetRelations(IDictionary<string, Table> tables);

        protected abstract IEnumerable<Sequence> GetSequences();

        protected abstract IEnumerable<BaseUserDefinedType> GetUserDefinedTypes();

        public DatabaseCatalog GetCatalog()
        {
            if (Catalog != null) return Catalog;


            var catalog = new DatabaseCatalog()
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

            Catalog = catalog;
            return Catalog;
        }
    }
}
