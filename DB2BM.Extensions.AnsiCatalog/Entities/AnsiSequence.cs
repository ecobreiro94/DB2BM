using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.AnsiCatalog.Entities
{
    public class AnsiSequence
    {
        public string Name { get; set; }

        public string Increment { get; set; }

        public string Start { get; set; }

        public string MinValue { get; set; }

        public string MaxValue { get; set; }
    }
}
