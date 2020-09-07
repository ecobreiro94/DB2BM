using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.AfterOps;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST
{
    public class WindowsDefinitionNode : ASTNode
    {
        public IdNode Identifier { get; set; }

        public List<ExpressionNode> PartitionByColumns { get; set; }

        public OrderByClauseNode OrderByClause { get; set; }

        public FrameClauseNode FrameClause { get; set; }

        public WindowsDefinitionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
