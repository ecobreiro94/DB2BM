using DB2BM.Abstractions.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.Interfaces
{
    public interface ISemanticAnalyzer
    {
        DatabaseCatalog Catalog { get; set; }

        Dictionary<string, string> TypesMapper { get; set; }

        List<SemanticResult> Check(StoredProcedure sp);
    }
}
