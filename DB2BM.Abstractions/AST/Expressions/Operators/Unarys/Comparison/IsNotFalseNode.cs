﻿using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions.Operators.Unarys.Comparison
{
    /// <summary>
    /// is true or unknown
    /// </summary>
    public class IsNotFalseNode : ComparisonUnaryExpressionNode
    {
        public IsNotFalseNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
