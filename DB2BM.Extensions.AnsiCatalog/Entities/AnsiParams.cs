using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.AnsiCatalog.Entities
{
    public class AnsiParams
    {
        public string Name { get; set; }

        public string FunctionSpecificName { get; set; }

        public string DataTypeName { get; set; }

        public string UdtName { get; set; }

        public int OrdinalPosition { get; set; }

        public string IsResult { get; set; }

        public string ParameterMode { get; set; }

        public AnsiRoutine Function { get; set; }
    }
}
