using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.AST.Expressions.Atomics;

namespace DB2BM.Abstractions.AST.Statements
{
    public abstract class SelectBodyNode : StatementNode 
    {
        public SelectBodyNode(int line, int column) : base(line, column)
        {
        }

        public List<FromItemNode> FromItems { get; set; }

        public bool Where { get; set; }

        public bool Having { get; set; }

        public List<ExpressionNode> Expressions { get; set; }

        public SelectListNode SelectList { get; set; }

        public SchemaQualifieldNode IntoTable { get; set; }

        public GroupByClauseNode GroupByClause { get; set; }

        public List<IdNode> Identifiers { get; set; }

        public List<WindowsDefinitionNode> WindowsDefinitions { get; set; }

    }
}
