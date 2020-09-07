using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Statements.Base
{
    public class VarNode : ASTNode, IResult
    {
        public SchemaQualifieldNode SchemaQualifield { get; set; }
        public DollarNumberNode Id { get; set; }
        public List<ExpressionNode> Expressions { get; set; }
        public string TypeReturn { get; set; }

        public VarNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
