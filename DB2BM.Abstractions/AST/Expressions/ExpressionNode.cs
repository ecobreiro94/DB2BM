using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Expressions
{
    public abstract class ExpressionNode : ASTNode, IResult
    {
        public ExpressionNode(int line, int column) : base(line, column)
        {
        }

        public string TypeReturn { get; set; }
    }
}
