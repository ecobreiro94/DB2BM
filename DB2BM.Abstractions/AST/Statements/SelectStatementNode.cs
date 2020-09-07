using DB2BM.Abstractions.AST.AfterOps;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements
{
    public class SelectStatementNode : StatementNode, IResult
    {
        public WithClauseNode WithClause { get; set; }

        public SelectOpsNode SelectOps { get; set; }

        public List<AfterOpsNode> AfterOps { get; set; }
        public string TypeReturn { get; set; }

        public SelectStatementNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
    
}
