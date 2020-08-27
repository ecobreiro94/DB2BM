using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST
{
    public class OrderSpecificationNode : ASTNode
    {
        public bool Asc { get; set; }
        public AllOpRefNode Using { get; set; }

        public OrderSpecificationNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
