using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions.Operators
{
    public class InNode : ExpressionNode
    {
        public List<ExpressionNode> Expressions { get; set; }
        public bool Not { get; set; }
        public Statements.SelectStmtNonParensNode SelectStmtNonParens { get; set; }

        public InNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
