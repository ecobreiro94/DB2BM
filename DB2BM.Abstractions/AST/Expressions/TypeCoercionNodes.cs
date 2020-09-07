using DB2BM.Abstractions.AST.Types;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Expressions
{
    public abstract class TypeCoercionNode : ExpressionNode
    {
        public TypeCoercionNode(int line, int column) : base(line, column)
        {
        }
    }
    public class BaseTypeCoercionNode : TypeCoercionNode
    {
        public DataTypeNode DataType { get; set; }
        public string Id { get; set; }

        public BaseTypeCoercionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public class IntervalTypeCoercionNode : TypeCoercionNode
    {
        public string Id { get; set; }
        public IntervalFieldNode IntervalField { get; set; }
        public int Length { get; set; }
        public IntervalTypeCoercionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
