using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.AfterOps
{
    public class AfterOpsForNode : AfterOpsNode
    {
        public bool Update { get; set; }
        public bool NoKeyUpdate { get; set; }
        public bool Share { get; set; }
        public bool KeyShare { get; set; }
        public List<SchemaQualifieldNode> SchemaQualifields { get; set; }

        public AfterOpsForNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
