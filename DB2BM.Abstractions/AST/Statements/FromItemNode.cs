using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.AST.Statements;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST
{
    public abstract class FromItemNode : ASTNode
    {
        public string Alias { get; set; }
        public FromItemNode(int line, int column) : base(line, column)
        {
        }
    }

    public class FromItemSimpleNode : FromItemNode
    {
        public FromItemNode Item { get; set; }
        public AliasClauseNode AliasClause { get; set; }
        public FromItemSimpleNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class FromItemCrossJoinNode : FromItemNode
    {
        public FromItemNode Item1 { get; set; }
        public FromItemNode Item2 { get; set; }

        public bool Inner { get; set; }
        public bool Left { get; set; }
        public bool Right { get; set; }
        public bool Full { get; set; }
        public bool Outer { get; set; }

        public FromItemCrossJoinNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class FromItemOnExpressionNode : FromItemCrossJoinNode
    {
        public ExpressionNode Expression { get; set; }

        public FromItemOnExpressionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class FromItemUsingNode : FromItemCrossJoinNode
    {
        public List<SchemaQualifieldNode> NamesInParens { get; set; }
        public FromItemUsingNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class FromItemNaturalNode : FromItemCrossJoinNode
    {
        public FromItemNaturalNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
