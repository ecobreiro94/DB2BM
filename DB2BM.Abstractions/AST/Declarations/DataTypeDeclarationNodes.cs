using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Declarations
{
    public abstract class DataTypeDeclarationNode : ASTNode, IResult
    {
        public DataTypeDeclarationNode(int line, int column) : base(line, column)
        {
        }

        public abstract string TypeReturn { get; set; }
    }

    public class ModularTypeDeclarationNode : DataTypeDeclarationNode
    {
        public ModularTypeDeclarationNode(int line, int column) : base(line, column)
        {
        }

        public SchemaQualifieldNode Schema { get; set; }
        public override string TypeReturn { get; set; }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class ModularRowTypeDeclarationNode : DataTypeDeclarationNode
    {
        public ModularRowTypeDeclarationNode(int line, int column) : base(line, column)
        {
        }

        public SchemaQualifiednameNonTypeNode Schema { get; set; }
        public override string TypeReturn { get; set; }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
