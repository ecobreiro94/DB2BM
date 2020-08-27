﻿using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements.Addicional
{
    public class AddClusterNode : AddicionalStatementNode
    {
        public bool On { get; set; }

        public IdNode Identifier { get; set; }

        public SchemaQualifieldNode SchemaQualifield { get; set; }
        public AddClusterNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }


}
