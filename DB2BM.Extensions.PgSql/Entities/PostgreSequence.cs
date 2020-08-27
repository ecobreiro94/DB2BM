using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.PgSql.Entities
{
    public class PostgreSequence
    {
        public string Name { get; set; }
        public string Increment { get; set; }

        public string Start { get; set; }

        public string MinValue { get; set; }

        public string MaxValue { get; set; }
    }
}
