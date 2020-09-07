using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements.Additional
{
    public abstract class AdditionalStatementNode : StatementNode
    {
        public AdditionalStatementNode(int line, int column) : base(line, column)
        {
        }
    }
}
