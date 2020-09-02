using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Entities.UserDefined;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Expressions.Atomics
{
    public enum IdentifierType
    {
        Variable,
        Table,
        TableField,
        Udt,
        UdtField
    }

    public class IdNode : ExpressionNode
    {
        public string Text { get; set; }
        public IdNode(int line, int column, string id) : base(line, column)
        {
            Text = id;
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        private IdentifierType type;
        public IdentifierType Type
        {
            get => (entity == null)?IdentifierType.Variable : type;
            private set { }
        }

        private object entity;
        public Table Table
        {
            get => entity as Table;
            set
            {
                entity = value;
                type = IdentifierType.Table;
            }
        }
        public BaseUserDefinedType Udt
        {
            get => entity as BaseUserDefinedType;
            set
            {
                entity = value;
                type = IdentifierType.Udt;
            }
        }
        public TableField TableField
        {
            get => entity as TableField;
            set
            {
                entity = value;
                type = IdentifierType.TableField;
            }
        }
        public UdtField UdtField
        {
            get => entity as UdtField;
            set
            {
                entity = value;
                type = IdentifierType.UdtField;
            }
        }
    }

    public class DollarNumberNode : IdNode
    {
        public DollarNumberNode(int line, int column, string id) : base(line, column, id)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
