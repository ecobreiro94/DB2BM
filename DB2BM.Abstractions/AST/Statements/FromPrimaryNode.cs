using System.Collections.Generic;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.AST.Expressions.FunctionCalls;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Statements
{
    public abstract class FromPrimaryNode : ASTNode
    {
        public FromPrimaryNode(int line, int column) : base(line, column)
        {
        }
    }

    public class FromPrimary1Node : FromPrimaryNode
    {
        public SchemaQualifieldNode SchemaQualifield { get; set; }

        public AliasClauseNode AliasClause { get; set; }

        public IdNode Methods { get; set; }

        public List<ExpressionNode> Expressions { get; set; }

        public bool Repeatable { get; set; }

        public FromPrimary1Node(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class FromPrimary2Node : FromPrimaryNode
    {
        public SelectStatementNode TableSubquery { get; set; }

        public AliasClauseNode AliasClause { get; set; }

        public FromPrimary2Node(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class FromPrimary3Node : FromPrimaryNode
    {
        public FunctionCallNode FunctionCall { get; set; }

        public List<FromFunctionColumnDefNode> ParamsDef { get; set; }

        public Expressions.Atomics.IdNode IdAlias { get; set; }

        public List<Expressions.Atomics.IdNode> ColumnAlias { get; set; }

        public FromPrimary3Node(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class FromPrimary4Node : FromPrimaryNode
    {
        public List<FunctionCallNode> FunctionCalls { get; set; }

        public List<FromFunctionColumnDefNode> ColumnsDef { get; set; }

        public IdNode IdAlias { get; set; }

        public List<IdNode> ColumnAlias { get; set; }

        public FromPrimary4Node(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
