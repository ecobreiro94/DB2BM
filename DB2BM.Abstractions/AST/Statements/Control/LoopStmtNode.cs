using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements.Control
{
    public class LoopStmtNode : ControlStatementNode
    {
        public List<StatementNode> Statemets { get; set; }

        public LoopStartNode LoopStart { get; set; }

        public ExpressionNode Expression { get; set; }

        public bool Continue { get; set; }

        public bool Exit { get; set; }

        public LoopStmtNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
