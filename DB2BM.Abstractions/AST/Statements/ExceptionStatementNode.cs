using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Statements
{
    public class ExceptionStatementNode : StatementNode
    {
        public List<ExpressionNode> Expressions { get; set; }

        public List<List<StatementNode>> Statements { get; set; }

        public ExceptionStatementNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
