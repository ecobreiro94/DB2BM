using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements.Additional
{
    public abstract class CopyStmtNode : AdditionalStatementNode
    {
        public CopyStmtNode(int line, int column) : base(line, column)
        {
        }
    }
}
