using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Extensions.AnsiCatalog.Entities;

namespace DB2BM.Extensions.PgSql.Entities
{
    public class PostgreUDTField : AnsiBaseField
    {
        public string UDTName { get; set; }

        public PostgreUserDefinedType UDT { get; set; }
    }
}
