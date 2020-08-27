using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.PgSql.Entities
{
    public abstract class PostgreBaseField
    {
        public string Name { get; set; }

        public string TypeName { get; set; }

        public int OrdinalPosition { get; set; }

        public string Default { get; set; }

        public string IsNullable { get; set; }

        public int? CharacterMaximumLength { get; set; }
    }
}
