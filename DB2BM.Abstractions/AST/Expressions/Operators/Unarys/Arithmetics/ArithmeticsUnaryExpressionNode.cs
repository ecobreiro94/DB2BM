using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Expressions.Operators.Unarys.Arithmetics
{
    public abstract class ArithmeticsUnaryExpressionNode : UnaryExpressionNode
    {
        public ArithmeticsUnaryExpressionNode(int line, int column) : base(line, column)
        {
        }
    }
}
