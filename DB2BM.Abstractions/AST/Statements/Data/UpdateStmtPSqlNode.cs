using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Statements.Data
{
    public class UpdateStmtPSqlNode : DataStatementNode
    {
        public WithClauseNode WithClause { get; set; }

        public SchemaQualifieldNode UpdateTableName { get; set; }

        public IdNode Alias { get; set; }

        public List<UpdateSetNode> UpdateSets { get; set; }

        public List<FromItemNode> FromItems { get; set; }

        public ExpressionNode Expression { get; set; }

        public IdNode Cursor { get; set; }

        public SelectListNode SelectList { get; set; }

        public UpdateStmtPSqlNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
