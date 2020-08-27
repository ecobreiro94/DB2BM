using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions
{
    public class DatetimeOverlapsNode : ExpressionNode
    {
        public ExpressionNode Expression1 { get; set; }

        public ExpressionNode Expression2 { get; set; }

        public ExpressionNode Expression3 { get; set; }

        public ExpressionNode Expression4 { get; set; }
        public DatetimeOverlapsNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
