﻿using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.AfterOps
{
    public class OrderByClauseNode : AfterOpsNode
    {
        public List<SortSpecifierNode> SortSpecifiers { get; set; }
        public OrderByClauseNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
