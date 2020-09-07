using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.PgSql.Entities
{
    public class PostgreParams
    {
        public string Name { get; set; }

        public string TypeName { get; set; }

        public string FunctionSpecificName { get; set; }

        public PostgreFunction Function { get; set; }

        public int OrdinalPosition { get; set; }

        public string IsResult { get; set; }

        public string ParameterMode { get; set; }
    }
}
