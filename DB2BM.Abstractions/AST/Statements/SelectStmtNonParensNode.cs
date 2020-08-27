using DB2BM.Abstractions.AST.AfterOps;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements
{
    public class SelectStmtNonParensNode : StatementNode, IResult
    {
        public WithClauseNode WithClause { get; set; }

        public SelectOpsNoParensNode SelectOps { get; set; }

        public List<AfterOpsNode> AfterOps { get; set; }
        public string TypeReturn { get; set; }

        public SelectStmtNonParensNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class SelectOpsNoParensNode : StatementNode
    {
        public bool Intersect { get; set; }
        public bool Union { get; set; }
        public bool Except { get; set; }

        public string Qualifier { get; set; }

        public SelectOpsNode SelectOps { get; set; }

        public SelectPrimaryNode SelectPrimary { get; set; }

        public SelectStatementNode SelectStmt { get; set; }

        public SelectOpsNoParensNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
