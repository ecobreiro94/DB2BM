using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Statements.Base
{
    public class AssingStmtNode : BaseStatementNode
    {
        public StatementNode Stmt { get; set; }
        public VarNode Var { get; set; }
        public string Symbol { get; set; }
        public AssingStmtNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
