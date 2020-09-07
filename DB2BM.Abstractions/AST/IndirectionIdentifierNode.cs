using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST
{
    public class IndirectionIdentifierNode : ASTNode
    {
        public IdNode Identifier { get; set; }
        public List<IndirectionNode> Indirections { get; set; }
        public IndirectionIdentifierNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
