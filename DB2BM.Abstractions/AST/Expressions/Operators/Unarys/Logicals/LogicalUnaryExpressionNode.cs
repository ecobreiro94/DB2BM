using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Expressions.Operators.Unarys.Logicals
{
    public abstract class LogicalUnaryExpressionNode : UnaryExpressionNode
    {
        public LogicalUnaryExpressionNode(int line, int column) : base(line, column)
        {
        }
    }
}
