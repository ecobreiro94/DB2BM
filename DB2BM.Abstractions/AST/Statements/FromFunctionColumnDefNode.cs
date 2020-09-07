using System.Collections.Generic;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Statements
{
    public class FromFunctionColumnDefNode : ASTNode
    {
        public List<IdNode> ColumnsAlias { get; set; }
        public List<DataTypeNode> DataTypes { get; set; }

        public FromFunctionColumnDefNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
