using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions.Operators
{
    public class OfNode : ExpressionNode
    {
        public ExpressionNode Expression { get; set; }
        public List<DataTypeNode> DataTypes { get; set; }
        public bool Not { get; set; }
        public OfNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
