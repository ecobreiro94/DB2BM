using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST
{
    public class GroupByClauseNode : ASTNode
    {
        public List<ExpressionNode> Expressions { get; set; }

        public GroupByClauseNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
