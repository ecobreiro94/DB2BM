using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.PgSql.Entities
{
    public class PostgreField : PostgreBaseField
    {
        public PostgreTable Table { get; set; }

        public string TableName { get; set; }
    }
}
