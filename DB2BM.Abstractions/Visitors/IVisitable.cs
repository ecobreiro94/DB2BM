using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.Visitors
{
    public interface IVisitable 
    {
        TResult Accept<TResult>(ASTVisitor<TResult> visitor);
    }
}
