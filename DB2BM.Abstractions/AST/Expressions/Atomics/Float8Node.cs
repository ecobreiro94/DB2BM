using System;
using System.Collections;
using System.Collections.Generic;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions.Atomics
{
    public class Float8Node : ExpressionNode
    {
        public double Value {get;}

        public Float8Node(int line, int column, double value) : base(line, column)
        {
            Value = value;
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}