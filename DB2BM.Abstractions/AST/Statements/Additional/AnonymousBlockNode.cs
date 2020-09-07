﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements.Additional
{
    public abstract class AnonymousBlockNode : AdditionalStatementNode
    {
        public AnonymousBlockNode(int line, int column) : base(line, column)
        {
        }
    }
}
