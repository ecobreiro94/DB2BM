using System;
using System.Collections;
using System.Collections.Generic;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions.Atomics
{
    public class Int8Node : ExpressionNode
    {
        public long Value { get; set; }

        public Int8Node(int line, int column, long value) : base(line, column)
        {
            Value = value;
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}