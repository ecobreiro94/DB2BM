using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Statements.Data
{
    public class TruncateStmtNode : DataStatementNode
    {
        public List<SchemaQualifieldNode> SchemaQualifields { get; set; }
        public bool Cascade { get; set; }
        public bool Restrict { get; set; }
        public TruncateStmtNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
