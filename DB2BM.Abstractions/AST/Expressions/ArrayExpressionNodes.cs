using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Statements;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions
{
    public abstract class ArrayExpressionNode : ExpressionNode
    {
        public ArrayExpressionNode(int line, int column) : base(line, column)
        {
        }
    }

    public class ArrayToSelectNode : ArrayExpressionNode
    {

        public SelectStatementNode SelectStmt { get; set; }
        
        public ArrayToSelectNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class ArrayElementsNode : ArrayExpressionNode
    {
        public List<ExpressionNode> Elements { get; set; }
        public ArrayElementsNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
