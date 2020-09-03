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
    public class PostgreSyntacticAnalyzer : ISyntacticAnalyzer
    {

        public void Parse(StoreProcedure sp)
        {
            var parser = new Parser.FunctionDefinitionConstructor(sp.PLDefinition);
            sp.Definition = parser.GetAST();
        }
    }
}
