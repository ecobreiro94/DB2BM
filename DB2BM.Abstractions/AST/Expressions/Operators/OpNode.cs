using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Expressions.Operators
{
    public class OpNode : ASTNode
    {
        string op;
        public string Operator
        {
            get
            {
                if ( op == "||")
                    return "+";
                else return op;
            }
            set
            {
                op = value;
            }
        }
        public IdNode Identifier { get; set; }

        public OpNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
