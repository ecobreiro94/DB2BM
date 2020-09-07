using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements.Control
{
    public abstract class ControlStatementNode : StatementNode
    {
        public ControlStatementNode(int line, int column) : base(line, column)
        {
        }
    }
}
