using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements
{
    public abstract class StatementNode : ASTNode
    {
        public StatementNode(int line, int column) : base(line, column)
        {
        }
    }
}
