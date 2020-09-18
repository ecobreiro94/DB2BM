using DB2BM.Extensions.AnsiCatalog.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.MySql.Entities
{
    public class MySqlRelationship : AnsiRelationship
    {
        public new virtual MySqlKeyColumnUsage KeyColumn { get; set; }
    }

    public class MySqlKeyColumnUsage : AnsiKeyColumnUsage
    {
        public string ReferencedTableName { get; set; }

        public string ReferencedColumnName { get; set; }

    }


}
