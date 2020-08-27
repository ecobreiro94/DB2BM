using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Statements;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions.Operators.Unarys.Logicals
{
    public class ExistsNode : ExpressionNode
    {
        public SelectStatementNode SelectStmt { get; set; }
        public ExistsNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
