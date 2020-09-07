using DB2BM.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Expressions.Operators.Binarys.Comparison
{
    public abstract class ComparisonBinaryExpressionNode : BinaryExpressionNode
    {
        public ComparisonBinaryExpressionNode(int line, int column) : base(line, column)
        {
        }
    }
}
