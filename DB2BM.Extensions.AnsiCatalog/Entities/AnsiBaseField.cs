using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.AnsiCatalog.Entities
{
    public abstract class AnsiBaseField
    {
        public string Name { get; set; }

        public string DataTypeName { get; set; }

        public string UdtName { get; set; }

        public int OrdinalPosition { get; set; }

        public string Default { get; set; }

        public string IsNullable { get; set; }

        public uint? CharacterMaximumLength { get; set; }
    }
}
