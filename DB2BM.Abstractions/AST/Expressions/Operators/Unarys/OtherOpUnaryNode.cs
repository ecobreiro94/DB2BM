using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions.Operators.Unarys
{
    public class OtherOpUnaryNode : UnaryExpressionNode
    {
        public OtherOpUnaryNode(int line, int column) : base(line, column)
        {
        }
        public bool FirstOp { get; set; }
        public OpNode Op { get; set; }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
