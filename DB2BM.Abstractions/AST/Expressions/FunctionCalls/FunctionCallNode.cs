using DB2BM.Abstractions.AST;
using DB2BM.Abstractions.AST.AfterOps;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Expressions.FunctionCalls
{
    public abstract class FunctionCallNode : ExpressionNode
    {
        public FunctionCallNode(int line, int column) : base(line, column)
        {
        }
    }

    public class BasicFunctionCallNode : FunctionCallNode
    {
        public SchemaQualifiednameNonTypeNode SchemaQualifiednameNonType { get; set; }

        public string Qualifier { get; set; }

        public List<VexOrNamedNotationNode> VexOrNamedNotations { get; set; }

        public List<OrderByClauseNode> OrderByClause { get; set; }

        public bool WithinGroup { get; set; }

        public ExpressionNode Filter { get; set; }

        public IdNode Identifier { get; set; }

        public WindowsDefinitionNode WindowsDefinition { get; set; }

        public BasicFunctionCallNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
