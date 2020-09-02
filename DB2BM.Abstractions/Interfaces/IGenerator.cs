using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Entities;

namespace DB2BM.Abstractions.Interfaces
{
    public interface IGenerator
    {
        DatabaseCatalog Catalog { get; set; }

        ISemanticAnalyzer SemanticAnalizer { get; set; }

        string OutputPath { get; set; }
        
        void GenerateEntities();

        void GenerateSPs(string className);

        void GenerateSPs(string className, List<string> functionNames);

        void GenerateDbContext();
    }
}
