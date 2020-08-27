﻿using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Statements.Addicional
{
    public class AddReindexNode : AddicionalStatementNode
    {
        public SchemaQualifieldNode SchemaQualifield { get; set; }
        public AddReindexNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}