using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST
{
    public class FrameClauseNode : ASTNode
    {
        public bool Range { get; set; }
        public bool Rows { get; set; }
        public bool Groups { get; set; }

        public bool ExcludeCurrentRow{ get; set; }
        public bool ExcludeGroup { get; set; }
        public bool ExcludeTies { get; set; }
        public bool ExcludeNoOthers { get; set; }

        public List<FrameBoundNode> FrameBounds { get; set; }

        public FrameClauseNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class FrameBoundNode : ASTNode
    {
        public bool CurrentNow { get; set; }
        public ExpressionNode Expression { get; set; }
        public bool Preceding { get; set; }
        public FrameBoundNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
