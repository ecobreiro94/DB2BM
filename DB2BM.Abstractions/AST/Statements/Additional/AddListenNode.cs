using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements.Additional
{
    public class AddListenNode : AdditionalStatementNode
    {
        public IdNode Identifier { get; set; }
        public AddListenNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
