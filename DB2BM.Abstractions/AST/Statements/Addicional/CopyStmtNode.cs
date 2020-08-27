using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements.Addicional
{
    public abstract class CopyStmtNode : AddicionalStatementNode
    {
        public CopyStmtNode(int line, int column) : base(line, column)
        {
        }
    }
}
