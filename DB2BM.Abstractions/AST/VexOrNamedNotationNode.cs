using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST
{
    public class VexOrNamedNotationNode : ASTNode, IResult
    {
        public IdNode ArgName { get; set; }
        public string Pointer { get; set; }

        public ExpressionNode Expression { get; set; }
        public string TypeReturn { get; set; }

        public VexOrNamedNotationNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
