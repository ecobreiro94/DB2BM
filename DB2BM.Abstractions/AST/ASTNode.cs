using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST
{
    public abstract class ASTNode : IVisitable
    {
        public int Line { get; }

        public int Column { get; }

        public ASTNode(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public abstract TResult Accept<TResult>(ASTVisitor<TResult> visitor);
    }
}
