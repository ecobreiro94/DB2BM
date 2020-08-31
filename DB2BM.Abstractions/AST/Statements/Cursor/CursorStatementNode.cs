using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.AST.Statements.Base;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Statements.Cursor
{

    public class FetchMoveIndirectionNode
    {
        public bool Next { get; set; }
        public bool Prior { get; set; }
        public bool First{ get; set; }
        public bool Last{ get; set; }

        public bool All{ get; set; }
        public bool Absolute { get; set; }
        public bool Relative { get; set; }
        public bool Forward{ get; set; }
        public bool Backward{ get; set; }
        public int Count { get; set; }
        
    }

    public class CursorStatementNode : StatementNode, IResult
    {
        public VarNode Var { get; set; }

        public FetchMoveIndirectionNode FetchMoveIndirection { get; set; }

        public bool Open { get; set; }

        public bool Fetch { get; set; }

        public bool Move { get; set; }

        public bool Close { get; set; }

        public bool From { get; set; }
        public bool In { get; set; }

        public List<OptionNode> Options { get; set; }

        public StatementNode Stmt { get; set; }

        public List<SchemaQualifieldNode> IntoTable { get; set; }

        public string TypeReturn { get; set; }

        public CursorStatementNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
