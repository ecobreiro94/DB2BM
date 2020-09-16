using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.AnsiCatalog.Entities
{
    public class AnsiRoutineResult
    {
        public string Clause { get; set; }
    }

    public class AnsiRoutine
    {
        public string Name { get; set; }

        public string SpecificSchema { get; set; }

        public string SpecificName { get; set; }

        public string RoutineType { get; set; }

        public string ReturnDataType { get; set; }

        public string ReturnUdtType { get; set; }

        public string ReturnClause { get; set; }

        public int MaxDynamicResultSets { get; set; }

        public string Definition { get; set; }

        public string RoutineLanguage { get; set; }

        public string ExternalLanguage { get; set; }

        public virtual ICollection<AnsiParams> Params { get; set; }
    }
}
