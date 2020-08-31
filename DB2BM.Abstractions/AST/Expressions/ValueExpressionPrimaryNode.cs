using DB2BM.Abstractions.AST.Statements;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Expressions
{
    public class ValueExpressionPrimaryNode : ExpressionNode
    {
        public SelectStmtNonParensNode SelectStmtNonParens { get; set; }
        public List<IndirectionNode> Indirections { get; set; }
        public ValueExpressionPrimaryNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
