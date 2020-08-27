using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions
{
    public class CaseExpressionNode : ExpressionNode
    {
        public bool Else { get; set; }
        public List<ExpressionNode> Expressions { get; set; }

        public bool ContainsRootExpression => (Else) ? Expressions.Count % 2 == 0 : Expressions.Count % 2 == 1;
        public CaseExpressionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
