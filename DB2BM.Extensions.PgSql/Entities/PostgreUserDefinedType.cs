using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.PgSql.Entities
{
    public class PostgreUserDefinedType
    {
        public string Name { get; set; }

        public string Schema { get; set; }

        public string Category { get; set; }

        public virtual ICollection<PostgreUDTField> Fields { get; set; }
    }
}
