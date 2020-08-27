using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions.Operators.Binarys
{

    public abstract class LikeNode : BinaryExpressionNode
    {
        public ExpressionNode Escape { get; set; }
        public LikeNode(int line, int column) : base(line, column)
        {
        }
    }
    public class LikeBinaryNode : LikeNode
    {
        public LikeBinaryNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class NotLikeBinaryNode : LikeNode
    {
        public NotLikeBinaryNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class ILikeBinaryNode : LikeNode
    {
        public ILikeBinaryNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class NotILikeBinaryNode : LikeNode
    {
        public NotILikeBinaryNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public class SimilarToBinaryNode : LikeNode
    {
        public SimilarToBinaryNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class NotSimilarToBinaryNode : LikeNode
    {
        public NotSimilarToBinaryNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
