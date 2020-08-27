using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions.Atomics
{
    public class IdNode : ExpressionNode
    {
        public string Text { get; set; }
        public IdNode(int line, int column, string id) : base(line, column)
        {
            Text = id;
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class DollarNumberNode : IdNode
    {
        public DollarNumberNode(int line, int column, string id) : base(line, column, id)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
