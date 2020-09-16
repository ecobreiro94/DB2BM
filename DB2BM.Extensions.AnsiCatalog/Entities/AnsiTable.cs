using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DB2BM.Extensions.AnsiCatalog.Entities
{
    public class AnsiTable
    {
        public string Name { get; set; }

        public string SchemaName { get; set; }

        public virtual ICollection<AnsiField> Fields { get; set; }
    }
}
