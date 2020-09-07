using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST
{
    public class SortSpecifierNode : ASTNode
    {
        public ExpressionNode Key { get; set; }
        public SchemaQualifieldNode OpClass { get; set; }
        public OrderSpecificationNode OrderSpecification { get; set; }
        public NullOrderingNode NullOrdering { get; set; }
        public SortSpecifierNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
