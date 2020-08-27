using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Statements.Addicional
{
    public class ShowStmtNode : AddicionalStatementNode
    {
        public IdNode PrimaryIdentifier { get; set; }
        public IdNode SecundaryIdentifier { get; set; }

        public bool All { get; set; }
        public bool TimeZone { get; set; }
        public bool TransactionIsilationLevel { get; set; }
        public bool SessionAuthorization { get; set; }
        public ShowStmtNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
