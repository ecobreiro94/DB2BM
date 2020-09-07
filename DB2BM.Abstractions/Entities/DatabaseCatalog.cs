using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DB2BM.Abstractions.Entities.UserDefined;
using DB2BM.Utils;

namespace DB2BM.Abstractions.Entities
{
    public class DatabaseCatalog
    {
        public string Name { get; set; }

        public Dictionary<string, Table> Tables { get; set; }

        public Dictionary<string, StoredProcedure> StoredProcedures { get; set; }

        public IEnumerable<Relationship> Relationships { get; set; }

        public Dictionary<string, BaseUserDefinedType> UserDefinedTypes { get; set; }

        public Dictionary<string, StoredProcedure> InternalFunctions { get; set; }

        public Dictionary<string, Sequence> Sequences { get; set; }

    }
}
