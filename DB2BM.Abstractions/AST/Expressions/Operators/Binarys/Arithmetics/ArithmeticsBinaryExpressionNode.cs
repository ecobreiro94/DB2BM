using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Interfaces;

namespace DB2BM.Abstractions.AST.Expressions.Operators.Binarys.Arithmetics
{
    public abstract class ArithmeticsBinaryExpressionNode : BinaryExpressionNode
    {
        public ArithmeticsBinaryExpressionNode(int line, int column) : base(line, column)
        {
        }

       
    }
}
