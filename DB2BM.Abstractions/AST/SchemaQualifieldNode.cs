using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Declarations;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST
{
    public class SchemaQualifieldNode : ASTNode, IResult
    {
        public SchemaQualifieldNode(int line, int column) : base(line, column)
        {
        }

        public List<IdNode> Identifiers { get; set; }
        public string TypeReturn { get; set; }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
