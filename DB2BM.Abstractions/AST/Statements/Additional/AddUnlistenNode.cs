using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Statements.Additional
{
    public class AddUnlistenNode : AdditionalStatementNode
    {
        public IdNode Identifier { get; set; }
        public AddUnlistenNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
