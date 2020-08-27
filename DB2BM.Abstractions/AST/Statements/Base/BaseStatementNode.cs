using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements.Base
{
    public abstract class BaseStatementNode : StatementNode
    {
        public BaseStatementNode(int line, int column) : base(line, column)
        {
        }
    }
}
