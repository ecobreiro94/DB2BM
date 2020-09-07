using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions.Atomics
{
    public class IntNode : ExpressionNode
    {
        public int Value { get; set; }

        public IntNode(int line, int column, int value) : base(line, column)
        {
            Value = value;
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
