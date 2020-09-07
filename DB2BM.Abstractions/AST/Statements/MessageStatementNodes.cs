using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements
{
    public abstract class MessageStatementNode : StatementNode
    {
        public MessageStatementNode(int line, int column) : base(line, column)
        {
        }
    }

    public class RaiseUsing
    {
        public List<string> RaiseParams { get; set; }
        public List<ExpressionNode> Expressions { get; set; }
    }

    public class RaiseMessageStatementNode : MessageStatementNode
    {
        public string LogLevel { get; set; }
        public string Message { get; set; }
        public RaiseUsing RaiseUsing { get; set; }
        public IdNode Identifier { get; set; }
        public List<ExpressionNode> Expressions { get; set; }
        public RaiseMessageStatementNode(int line, int column) : base(line, column)
        {

        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class AssertMessageStatementNode : MessageStatementNode
    {
        public List<ExpressionNode> Expressions { get; set; }
        public AssertMessageStatementNode(int line, int column) : base(line, column)
        {

        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
