using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Statements.Addicional
{
    public class AddAnalizeNode : AddicionalStatementNode
    {
        public List<AnalizeModeNode> Modes { get; set; }
        public List<TableColsNode> TableColsList { get; set; }
        public AddAnalizeNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class AnalizeModeNode : ASTNode
    {
        public ExpressionNode Value { get; set; }
        public bool Verbose { get; set; }
        public AnalizeModeNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class TableColsNode : ASTNode
    {
        public SchemaQualifieldNode SchemaQualifield { get; set; }
        public List<IdNode> Identifiers { get; set; }
        public TableColsNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
