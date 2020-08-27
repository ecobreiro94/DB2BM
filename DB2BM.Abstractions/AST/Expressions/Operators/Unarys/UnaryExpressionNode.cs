using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Expressions.Operators.Unarys
{
    public abstract class UnaryExpressionNode : ExpressionNode
    {
        public UnaryExpressionNode(int line, int column) : base(line, column)
        {
        }

        public ExpressionNode Operand { get; set; }

    }
}
