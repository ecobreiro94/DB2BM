using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;
using System;

namespace DB2BM.Abstractions.AST.Declarations
{
    public class OrdinalTypeDeclarationNode : TypeDeclarationNode
    {
        public DataTypeDeclarationNode DataType { get; set; }

        public SchemaQualifieldNode CollateIdentifier { get; set; }

        public ExpressionNode Expression { get; set; }

        public OrdinalTypeDeclarationNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
