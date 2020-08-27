using DB2BM.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Expressions.Operators.Binarys.Logicals
{
    public abstract class LogicalBinaryExpressionNode : BinaryExpressionNode
    {
        public LogicalBinaryExpressionNode(int line, int column) : base(line, column)
        {
        }
    }
}
