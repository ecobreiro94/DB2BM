using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Statements.Control
{
    public class CaseElement
    {
        public List<ExpressionNode> Expressions { get; set; }
        public List<StatementNode> Stmts { get; set; }
    }
    public class CaseStmtNode : ControlStatementNode
    {
        public ExpressionNode HeaderExpression { get; set; }
        public List<CaseElement> Cases { get; set; }
        public List<StatementNode> ElseStmts { get; set; }

        public CaseStmtNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
