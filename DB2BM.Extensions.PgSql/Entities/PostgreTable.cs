using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DB2BM.Extensions.PgSql.Entities
{
    public class PostgreTable
    {
        public string Name { get; set; }

        public string SchemaName { get; set; }

        public virtual ICollection<PostgreField> Fields { get; set; }

    }
}
