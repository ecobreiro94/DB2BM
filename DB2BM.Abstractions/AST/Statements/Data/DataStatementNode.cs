using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements.Data
{
    public abstract class DataStatementNode : StatementNode
    {
        public DataStatementNode(int line, int column) : base(line, column)
        {
        }
    }
}
