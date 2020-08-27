using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.AfterOps
{
    public abstract class AfterOpsNode : ASTNode
    {
        public AfterOpsNode(int line, int column) : base(line, column)
        {
        }

    }
}
