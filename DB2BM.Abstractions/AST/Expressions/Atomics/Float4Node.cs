using System;
using System.Collections;
using System.Collections.Generic;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions.Atomics
{
    public class Float4Node : ExpressionNode
    {
        public float Value {get;}
        public Float4Node(int line, int column, float value) : base(line, column)
        {
            Value = value;
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}