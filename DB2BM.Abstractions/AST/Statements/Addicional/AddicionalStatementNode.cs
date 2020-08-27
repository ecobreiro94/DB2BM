using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements.Addicional
{
    public abstract class AddicionalStatementNode : StatementNode
    {
        public AddicionalStatementNode(int line, int column) : base(line, column)
        {
        }
    }
}
