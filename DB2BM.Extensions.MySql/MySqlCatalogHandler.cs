using System.Linq;
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
using DB2BM.Extensions.AnsiCatalog;

namespace DB2BM.Extensions.MySql
{
    [Dbms("mysql")]
    public class MySqlCatalogHandler : AnsiCatalogHandler<MySqlDbContext>
    {
        public override string SchemaName => Options.DataBaseName;

        protected override string GetConnectionString(DbOption options)
        {
            return $"server={options.Host};user={options.User};password={options.Password};database=information_schema;port=3306;sslmode=preferred";            
        }

        protected override IEnumerable<Relationship> GetRelations(IDictionary<string, Table> tables)

        {
            var relations = InternalDbContext.Relationships
                                .Include(x => x.KeyColumn)
                                .Where(r => r.SchemaName == SchemaName && r.ConstraintType != "CHECK" && r.ConstraintType != "UNIQUE")
                                .AsEnumerable();
            return relations.Select(r =>
                                    new Relationship()
                                    {
                                        Table = tables[r.TableName],
                                        ReferencedTable = r.KeyColumn.ReferencedTableName != null ? tables[r.KeyColumn.ReferencedTableName] : null,
                                        Column = tables[r.TableName].Fields.First(c => c.Name == r.KeyColumn.ColumnName),
                                        ReferencedColumn = r.KeyColumn.ReferencedTableName != null ? tables[r.KeyColumn.ReferencedTableName].Fields.First(c => c.Name == r.KeyColumn.ReferencedColumnName) : null,
                                        Type = (r.ConstraintType == "PRIMARY KEY") ? RelationshipType.PrimaryKey : RelationshipType.ForeingKey
                                    });
            
        }

        protected override IEnumerable<Sequence> GetSequences()
        {
            return Enumerable.Empty<Sequence>();
        }

    }
}
