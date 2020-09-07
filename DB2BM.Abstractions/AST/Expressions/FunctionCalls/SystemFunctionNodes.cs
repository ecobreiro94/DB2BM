using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions.FunctionCalls
{
    public abstract class SystemFunctionNode : FunctionCallNode
    {
        public SystemFunctionNode(int line, int column) : base(line, column)
        {
        }
    }

    public class CurrentCatalogSystemFunctionNode : SystemFunctionNode
    {
        public CurrentCatalogSystemFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public class CurrentSchemaSystemFunctionNode : SystemFunctionNode
    {
        public CurrentSchemaSystemFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class CurrentUserSystemFunctionNode : SystemFunctionNode
    {
        public CurrentUserSystemFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class SessionUserSystemFunctionNode : SystemFunctionNode
    {
        public SessionUserSystemFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class UserSystemFunctionNode : SystemFunctionNode
    {
        public UserSystemFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class CastSpesificationSystemFunction : SystemFunctionNode
    {
        public ExpressionNode Expression { get; set; }

        public DataTypeNode DataType { get; set; }

        public CastSpesificationSystemFunction(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
