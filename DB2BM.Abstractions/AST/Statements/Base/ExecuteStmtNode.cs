using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Statements.Base
{
    public class ExecuteStmtNode : BaseStatementNode, IResult
    {
        public ExpressionNode Expression { get; set; }

        public List<ExpressionNode> UsingExpression { get; set; }
        public string TypeReturn { get; set; }

        public ExecuteStmtNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
