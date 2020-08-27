using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.AST.Statements.Data;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements.Addicional
{
    public class AddPrepareNode : AddicionalStatementNode
    {
        public IdNode Identifier { get; set; }

        public List<DataTypeNode> Types { get; set; }

        public DataStatementNode DataStatement { get; set; }

        public AddPrepareNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
