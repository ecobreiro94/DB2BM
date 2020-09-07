using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST
{
    public class IdentifierNonTypeNode : ASTNode
    {
        public string Text { get; set; }

        public IdentifierNonTypeNode(int line, int column, string id) : base(line, column)
        {
            Text = id;
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
