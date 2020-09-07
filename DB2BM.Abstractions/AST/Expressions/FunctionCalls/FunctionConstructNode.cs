using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions.FunctionCalls
{
    public class FunctionConstructNode : FunctionCallNode
    {
        public List<ExpressionNode> Expressions { get; set; }
        public bool FirstOrDeFault { get; set; }
        public bool Row { get; set; }
        public bool Coalesce { get; set; }
        public bool Greatest { get; set; }
        public bool Grouping { get; set; }
        public bool Least { get; set; }
        public bool Nullif { get; set; }
        public bool XmlConcat { get; set; }

        public FunctionConstructNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
