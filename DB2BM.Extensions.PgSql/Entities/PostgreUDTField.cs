using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.PgSql.Entities
{
    public class PostgreUDTField : PostgreBaseField
    {
        public string UDTName { get; set; }

        public PostgreUserDefinedType UDT { get; set; }
    }
}
