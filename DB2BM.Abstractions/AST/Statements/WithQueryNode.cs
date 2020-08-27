using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements
{
    public class WithQueryNode : ASTNode
    {
        public IdNode Identifier { get; set; }
        public List<IdNode> Columns { get; set; }
        public StatementNode Statement { get; set; }
        public WithQueryNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
