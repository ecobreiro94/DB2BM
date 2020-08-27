using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Statements;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions
{
    public class ComparisonModNode : ExpressionNode
    {
        public bool Any { get; set; }
        public bool All { get; set; }
        public bool Some { get; set; }
        public ExpressionNode Expression { get; set; }
        public SelectStmtNonParensNode SelectStmtNonParens { get; set; }

        public ComparisonModNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
