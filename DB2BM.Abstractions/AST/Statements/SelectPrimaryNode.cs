using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Statements
{
    public class SelectPrimaryNode : SelectBodyNode, IResult
    {
        public string SetQualifier { get; set; }
        public SchemaQualifieldNode SchemaQualifield { get; set; }
        public string TypeReturn { get; set; }

        public SelectPrimaryNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
