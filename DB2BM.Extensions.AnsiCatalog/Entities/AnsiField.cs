using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.AnsiCatalog.Entities
{
    public class AnsiField : AnsiBaseField
    {
        public AnsiTable Table { get; set; }

        public string TableName { get; set; }
    }
}
