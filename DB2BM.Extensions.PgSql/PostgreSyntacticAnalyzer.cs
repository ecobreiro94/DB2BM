using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DB2BM.Abstractions.Attrs;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Extensions.PgSql.Parser;

namespace DB2BM.Extensions.PgSql
{
    [Dbms("postgre")]
    public class PostgreSyntacticAnalyzer : ISyntacticAnalyzer
    {        
        public void Parse(StoreProcedure sp)
        {
            var parser = new SPParser(sp.OriginalCode);
            sp.AST = parser.GetAST();
        }
    }
}
