using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Statements.Addicional
{
    public class ExplainStmtNode : AddicionalStatementNode
    {
        public List<string> Options { get; set; }
        public StatementNode ExplainQuery { get; set; }
        public ExplainStmtNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
