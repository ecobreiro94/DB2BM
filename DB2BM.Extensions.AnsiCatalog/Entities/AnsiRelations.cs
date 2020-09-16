using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.AnsiCatalog.Entities
{
    public class AnsiRelationColumnUsage
    {
        public string ConstraintName { get; set; }

        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public AnsiRelationship Relation { get; set; }
    }

    public class AnsiKeyColumnUsage
    {
        public string ConstraintName { get; set; }

        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public AnsiRelationship Relation { get; set; }
    }

    public class AnsiRelationship
    {
        public string ConstraintName { get; set; }

        public string SchemaName { get; set; }

        public string TableName { get; set; }

        public string ConstraintType { get; set; }

        public virtual AnsiRelationColumnUsage RelationColumn { get; set; }

        public virtual AnsiKeyColumnUsage KeyColumn { get; set; }
    }
}
