using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions
{
    public class ExpressionListNode : ExpressionNode
    {
        public List<ExpressionNode> Expressions { get; set; }
        public ExpressionListNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
