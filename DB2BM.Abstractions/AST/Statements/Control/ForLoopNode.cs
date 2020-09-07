using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements.Control
{
    public class ForAliasLoopNode : LoopStartNode
    {
        public IdNode Identifier { get; set; }

        public List<ExpressionNode> Expressions { get; set; }

        public ForAliasLoopNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class ForIdListLoopNode : LoopStartNode
    {
        public List<IdNode> Identifiers { get; set; }

        public StatementNode Stmt { get; set; }

        public ForIdListLoopNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class ForCursorLoopNode : LoopStartNode
    {
        public IdNode Cursor { get; set; }

        public IdNode Identifier { get; set; }

        public List<OptionNode> Options { get; set; }

        public ForCursorLoopNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class ForeachLoopNode : LoopStartNode
    {
        public List<IdNode> Identifiers { get; set; }

        public ExpressionNode Expression { get; set; }

        public ForeachLoopNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
