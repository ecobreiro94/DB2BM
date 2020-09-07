using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Expressions.Operators.Unarys.Comparison
{
    public abstract class ComparisonUnaryExpressionNode : UnaryExpressionNode
    {
        public ComparisonUnaryExpressionNode(int line, int column) : base(line, column)
        {
        }
    }
}
