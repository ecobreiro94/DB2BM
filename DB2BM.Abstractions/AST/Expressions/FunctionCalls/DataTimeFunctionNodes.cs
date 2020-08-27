using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions.FunctionCalls
{
    public abstract class DataTimeFunctionNode : FunctionCallNode
    {
        public DataTimeFunctionNode(int line, int column) : base(line, column)
        {
        }
    }

    public class CurrentDateFunctionNode : DataTimeFunctionNode
    {
        public CurrentDateFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class CurrentTimeFunctionNode : DataTimeFunctionNode
    {
        public int Length { get; set; }
        public CurrentTimeFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class CurrentTimestampFunctionNode : DataTimeFunctionNode
    {
        public int Length { get; set; }
        public CurrentTimestampFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class LocalTimeFunctionNode : DataTimeFunctionNode
    {
        public int Length { get; set; }
        public LocalTimeFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class LocalTimestampFunctionNode : DataTimeFunctionNode
    {
        public int Length { get; set; }
        public LocalTimestampFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
