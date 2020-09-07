using DB2BM.Abstractions.AST.Declarations;
using DB2BM.Abstractions.AST.Types;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST
{
    public class DataTypeNode : DataTypeDeclarationNode
    {
        public PredefinedTypeNode Type { get; set; }
        public List<ArrayTypeNode> ArrayType { get; set; }
        public override string TypeReturn { get; set; }

        public DataTypeNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
