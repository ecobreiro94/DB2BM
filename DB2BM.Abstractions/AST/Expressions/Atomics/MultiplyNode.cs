﻿using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions.Atomics
{
    public class MultiplyNode : ExpressionNode
    {
        public MultiplyNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
