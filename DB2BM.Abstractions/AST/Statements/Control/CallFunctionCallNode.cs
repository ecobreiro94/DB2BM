using DB2BM.Abstractions.AST.Expressions.FunctionCalls;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements.Control
{
    public class CallFunctionCallNode : ControlStatementNode
    {
        public FunctionCallNode FunctionCall { get; set; }
        public CallFunctionCallNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
