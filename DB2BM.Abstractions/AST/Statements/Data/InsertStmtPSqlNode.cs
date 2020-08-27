using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Statements.Data
{
    public class InsertStmtPSqlNode : DataStatementNode
    {
        public WithClauseNode WithClause { get; set; }
        public SchemaQualifieldNode InsertTableName { get; set; }
        public IdNode Alias { get; set; }
        public List<IndirectionIdentifierNode> InsertColumns { get; set; }

        public SelectStatementNode SelectStmt { get; set; }
        public ConflictObjectNode ConflictObject { get; set; }
        public ConflictActionNode ConflictAction { get; set; }
        public SelectListNode SelectList { get; set; }
        public InsertStmtPSqlNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
