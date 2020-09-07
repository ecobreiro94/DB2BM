using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.AST.Statements;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST
{
    public class UpdateSetNode : ASTNode
    {
        public List<IndirectionIdentifierNode> Columns { get; set; }

        public List<ExpressionNode> Values { get; set; }

        public SelectStatementNode TableSubquery { get; set; }

        public UpdateSetNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
