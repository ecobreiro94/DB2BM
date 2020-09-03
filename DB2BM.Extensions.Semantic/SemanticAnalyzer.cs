using DB2BM.Abstractions;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.Semantic
{
    public class SemanticAnalyzer : ISemanticAnalyzer
    {
        public DatabaseCatalog Catalog { get; set; }

        public Dictionary<string, string> TypesMapper { get => SemanticVisitor.TypesMapper; set { } }
        
        public List<SemanticResult> Check(StoreProcedure sp)
        {
            var semanticVisitor = new SemanticVisitor(Catalog, sp);
            return sp.Definition.Accept(semanticVisitor);
        }
    }
}
