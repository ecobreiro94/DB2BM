using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Declarations
{
    public class DeclarationNode : ASTNode
    {
        public IdNode Identifier { get; set; }

        public TypeDeclarationNode TypeDeclaration { get; set; }

        public DeclarationNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
