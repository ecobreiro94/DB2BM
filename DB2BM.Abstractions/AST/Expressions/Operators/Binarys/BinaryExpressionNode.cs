using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Expressions.Operators.Binarys
{
    public abstract class BinaryExpressionNode : ExpressionNode
    {
        public BinaryExpressionNode(int line, int column) : base(line, column)
        {
        }

        public ExpressionNode LeftOperand { get; set; }

        public ExpressionNode RightOperand { get; set; }
    }
}
