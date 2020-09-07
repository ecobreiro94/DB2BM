using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions.FunctionCalls
{
    public abstract class StringValueFunctionNode : FunctionCallNode
    {
        public StringValueFunctionNode(int line, int column) : base(line, column)
        {
        }
    }

    public class TrimStringValueFunctionNode : StringValueFunctionNode
    {
        public ExpressionNode Chars { get; set; }

        public ExpressionNode Str { get; set; }
        
        public TrimStringValueFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class SubstringStringValueFunctionNode : StringValueFunctionNode
    {
        public List<ExpressionNode> Expressions { get; set; }
        public bool From { get; set; }
        public bool For { get; set; }
        public SubstringStringValueFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class PositionStringValueFunctionNode : StringValueFunctionNode
    {
        public ExpressionNode ExpressionB { get; set; }
        public ExpressionNode Expression { get; set; }
        public PositionStringValueFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class OverlayStringValueFunctionNode : StringValueFunctionNode
    {
        public List<ExpressionNode> Expressions { get; set; }
        public bool For { get; set; }
        public OverlayStringValueFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class CollationStringValueFunctionNode : StringValueFunctionNode
    {
        public ExpressionNode Expression { get; set; }
        public CollationStringValueFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
