﻿using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Statements.Control
{
    public class ReturnStmtNode : ControlStatementNode
    {
        public StatementNode Stmt { get; set; }
        public ExpressionNode Expression { get; set; }

        public ReturnStmtNode(int line, int column) : base(line, column)
        {

        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
