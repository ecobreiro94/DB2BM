using System;
using System.Collections;
using System.Collections.Generic;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions.Atomics
{
    public class BoolNode :ExpressionNode
    {
        public bool Value { get; }

        public BoolNode(int line, int column, bool value) : base(line, column)
        {
            Value = value;
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}