using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST
{
    public class AllOpRefNode : ASTNode
    {
        public AllSimpleOpNode AllSimpleOp { get; set; }
        public IdNode Identifier { get; set; }
        public AllOpRefNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
