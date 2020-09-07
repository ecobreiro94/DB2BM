using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions.Operators.Binarys
{
    public class OtherOpBinaryNode : BinaryExpressionNode
    {
        public OtherOpBinaryNode(int line, int column) : base(line, column)
        {
        }

        public OpNode Op { get; set; }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
