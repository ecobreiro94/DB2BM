using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DB2BM.Abstractions.Entities.UserDefined;

namespace DB2BM.Abstractions.Entities
{
    public class DatabaseCatalog
    {
        public string Name { get; set; }

        public Dictionary<string, Table> Tables { get; set; }

        public Dictionary<string, StoreProcedure> StoreProcedures { get; set; }

        public IEnumerable<Relationship> Relationships { get; set; }

        public Dictionary<string, BaseUserDefinedType> UserDefinedTypes { get; set; }

        public Dictionary<string, StoreProcedure> InternalFunctions { get; set; }

        public Dictionary<string, Sequence> Sequences { get; set; }

    }
}
