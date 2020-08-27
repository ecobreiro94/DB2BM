using DB2BM.Abstractions.AST.Statements;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST
{
    public class SelectOpsNode : ASTNode
    {
        public SelectStatementNode SelectStmt { get; set; }

        public bool Intersect { get; set; }
        public bool Union { get; set; }
        public bool Except { get; set; }
        public List<SelectOpsNode> SelectOps { get; set; }
        public string SetQualifier { get; set; }

        public SelectPrimaryNode SelectPrimary { get; set; }

        public SelectOpsNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
