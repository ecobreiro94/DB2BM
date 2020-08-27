using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.PgSql.Entities
{
    public class PostgreFunctionResult
    {
        public string Clause { get; set; }
    }

    public class PostgreFunction
    {
        public string Name { get; set; }

        public virtual ICollection<PostgreParams> Params { get; set; }

        public string SpecificName { get; set; }

        public string FunctionType { get; set; }

        public string ReturnClause { get; set; }

        public string ReturnType { get; set; }

        public string Definition { get; set; }

        public string LanguageDefinition { get; set; }

        public string SpecificSchema { get; set; }
    }
}
