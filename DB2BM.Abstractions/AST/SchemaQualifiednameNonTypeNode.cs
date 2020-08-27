using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Declarations;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST
{
    public class SchemaQualifiednameNonTypeNode : ASTNode, IResult
    {
        public IdNode Schema { get; set; }
        public IdentifierNonTypeNode IdentifierNonType { get; set; }
        public string TypeReturn { get; set; }

        public SchemaQualifiednameNonTypeNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
