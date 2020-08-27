using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Expressions;
namespace DB2BM.Abstractions.AST.Expressions.Operators.Ternarys
{
    public abstract class TernaryExpressionNode : ExpressionNode
    {
        public TernaryExpressionNode(int line, int column) : base(line, column)
        {
        }

        public ExpressionNode FirstOperand { get; set; }

        public ExpressionNode SecondOperand { get; set; }

        public ExpressionNode ThirdOperand { get; set; }
    }
}
