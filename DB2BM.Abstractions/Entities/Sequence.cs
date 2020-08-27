using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.Entities
{
    public class Sequence
    {
        public string Name { get; set; }
        public int Increment { get; set; }

        public int Start { get; set; }

        public int MinValue { get; set; }

        public long MaxValue { get; set; }

        public bool IsInt { get; set; }
    }
}
