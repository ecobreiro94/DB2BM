using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions.FunctionCalls
{
    public abstract class XmlFunctionNode : FunctionCallNode
    {
        public XmlFunctionNode(int line, int column) : base(line, column)
        {
        }
    }

    public class XmlElementFunctionNode : XmlFunctionNode
    {
        public IdNode Name { get; set; }

        public List<ExpressionNode> Expressions { get; set; }

        public List<IdNode> AttNames { get; set; }

        public XmlElementFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class XmlForestFunctionNode : XmlFunctionNode
    {
        public List<ExpressionNode> Expressions { get; set; }

        public List<IdNode> Names { get; set; }

        public XmlForestFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class XmlPiFunctionNode : XmlFunctionNode
    {
        public IdNode Name { get; set; }

        public ExpressionNode Expression { get; set; }

        public XmlPiFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class XmlRootFunctionNode : XmlFunctionNode
    {
        public List<ExpressionNode> Expressions { get; set; }

        public XmlRootFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class XmlExistsFunctionNode : XmlFunctionNode
    {
        public List<ExpressionNode> Expressions { get; set; }

        public XmlExistsFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class XmlParseFunctionNode : XmlFunctionNode
    {
        public ExpressionNode Expression { get; set; }

        public XmlParseFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class XmlSerializeFunctionNode : XmlFunctionNode
    {
        public ExpressionNode Expression { get; set; }

        public DataTypeNode DataType { get; set; }

        public XmlSerializeFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class XmlTabletFunctionNode : XmlFunctionNode
    {
        public List<ExpressionNode> Expressions { get; set; }

        public List<IdNode> Names { get; set; }

        public List<XmlTableColumnNode> XmlTableColumns { get; set; }

        public XmlTabletFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class XmlTableColumnNode : XmlFunctionNode
    {
        public IdNode Name { get; set; }
        public DataTypeNode DataType { get; set; }
        public List<ExpressionNode> Expressions { get; set; }
        public XmlTableColumnNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
