using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.AfterOps
{
    public class AfterOpsFetchNode : AfterOpsNode
    {
        public ExpressionNode Expression { get; set; }
        public bool First { get; set; }
        public bool Row { get; set; }
        public AfterOpsFetchNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
