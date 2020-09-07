using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Interfaces;

namespace DB2BM.Abstractions.AST.Declarations
{
    public abstract class TypeDeclarationNode : ASTNode
    {
        public TypeDeclarationNode(int line, int column) : base(line, column)
        {
        }
    }
}
