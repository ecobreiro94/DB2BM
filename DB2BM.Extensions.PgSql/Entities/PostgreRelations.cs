using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.PgSql.Entities
{
    public class PostgreRelationColumnUsage
    {
        public string ConstraintName { get; set; }

        public string TableName { get; set; }

        public string ColumnName { get; set; }
        public PostgreRelation Relation { get; set; }
    }

    public class PostgreKeyColumnUsage
    {
        public string ConstraintName { get; set; }

        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public PostgreRelation Relation { get; set; }
    }

    public class PostgreRelation
    {
        public string ConstraintName { get; set; }

        public string SchemaName { get; set; }

        public string TableName { get; set; }

        public string ConstraintType { get; set; }

        public PostgreRelationColumnUsage RelationColumn { get; set; }

        public PostgreKeyColumnUsage KeyColumn { get; set; }
    }
}
