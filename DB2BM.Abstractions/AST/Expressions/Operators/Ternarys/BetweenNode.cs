using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions.Operators.Ternarys
{
    public class BetweenNode : TernaryExpressionNode
    {
        public bool Symmetric { get; set; }
        public bool Asymmetric { get; set; }
        public bool Not { get; set; }
        public BetweenNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
