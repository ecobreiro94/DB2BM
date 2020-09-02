using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DB2BM.Abstractions.Attrs;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Interfaces;

namespace DB2BM.Extensions.PgSql
{
    [Dbms("postgre")]
    public class PostgreSyntacticAnalizer : ISyntacticAnalyzer
    {
        #region ISyntacticAnalyzer implementation
        private void SetAst(StoreProcedure sp)
        {
            if (sp.Name == "testselect21")
            { }
            var parser = new Parser.FunctionDefinitionConstructor(sp.PLDefinition);
            sp.Definition = parser.GetAST();
        }

        public void Parse(DatabaseCatalog catalog, List<string> functionNames)
        {
            foreach (var function in catalog.StoreProcedures.Values.Where(f => functionNames.Contains(f.Name)))
                SetAst(function);
        }

        public void Parse(DatabaseCatalog catalog)
        {
            foreach (var function in catalog.StoreProcedures.Values)
                SetAst(function);
        } 
        #endregion
    }
}
