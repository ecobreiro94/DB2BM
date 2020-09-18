using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DB2BM.Abstractions.AST;
using DB2BM.Abstractions.AST.Statements;
using DB2BM.Abstractions.Attrs;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Interfaces;

namespace DB2BM.Extensions.MySql
{
    [Dbms("mysql")]
    public class MySqlSyntacticAnalyzer : ISyntacticAnalyzer
    {
        public void Parse(StoredProcedure sp)
        {
            sp.AST = 
                new FunctionBlockNode(0, 0)            //AST de un SP que dispara una excepción
                {
                    Statements = new List<StatementNode>()
                    {
                        new RaiseMessageStatementNode(0, 0)
                        {
                            LogLevel = "EXCEPTION", Message = "Not implemented"
                        }
                    }
                };
        }
    }
}
