using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements.Control
{
    public abstract class LoopStartNode : ControlStatementNode
    {

        public LoopStartNode(int line, int column) : base(line, column)
        {
        }
    }
}
