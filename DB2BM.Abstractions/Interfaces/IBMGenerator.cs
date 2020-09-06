using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Entities;

namespace DB2BM.Abstractions.Interfaces
{
    public interface IBMGenerator
    {
        DatabaseCatalog Catalog { get; set; }

        ISyntacticAnalyzer SyntacticAnalyzer { get; set; }

        ISemanticAnalyzer SemanticAnalyzer { get; set; }

        void SetOutputPath(string path, bool isProject);
        
        void GenerateEntities();

        void GenerateContext();

        void GenerateService(string className);

        void GenerateService(string className, List<string> functionNames);
    }
}
