using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.AfterOps
{
    public class AfterOpsLimitNode : AfterOpsNode
    {
        public bool All { get; set; }
        public ExpressionNode Expression { get; set; }
        public AfterOpsLimitNode(int line, int column) : base(line, column)
        {
        }
        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
