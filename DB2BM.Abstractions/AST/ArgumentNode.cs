using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST
{
    public class ArgumentNode : ASTNode
    {
        public ArgumentNode(int line, int column) : base(line, column)
        { }
        public IdNode Identifier { get; set; }
        public DataTypeNode DataType { get; set; }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
