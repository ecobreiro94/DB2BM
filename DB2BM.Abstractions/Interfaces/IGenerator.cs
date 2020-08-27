using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Entities;

namespace DB2BM.Abstractions.Interfaces
{
    public interface IGenerator
    {
        DatabaseCatalog Catalog { get; set; }

        string OutputPath { get; set; }

        //Dictionary<string, string> TypesMapper { get; set; }
        
        void GenerateEntities();

        void GenerateAllFunctions(string className);

        void GenerateFunctions(string className, List<string> functionNames);

        void GenerateDbContext();
    }
}
