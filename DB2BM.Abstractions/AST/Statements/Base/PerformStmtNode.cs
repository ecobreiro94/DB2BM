using DB2BM.Abstractions.AST.AfterOps;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements.Base
{
    public class PerformStmtNode : SelectBodyNode, IResult
    {
        public string SetQualifier { get; set; }

        public List<AfterOpsNode> AfterOps { get; set; }

        public bool Intersect { get; set; }

        public bool Union { get; set; }

        public bool Except { get; set; }

        public string SetQualifier1 { get; set; }

        public SelectOpsNode SelectOps { get; set; }

        public string TypeReturn { get; set; }

        public PerformStmtNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
