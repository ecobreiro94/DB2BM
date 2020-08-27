using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions.AST.Statements
{
    public class ValuesStmtNode : SelectPrimaryNode
    {
        public List<ValuesValuesNode> Values { get; set; }
        public ValuesStmtNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class ValuesValuesNode : StatementNode, IResult
    {
        
        public List<ExpressionNode> Expressions { get; set; }
        public string TypeReturn { get; set; }

        public ValuesValuesNode(int line, int column) : base(line, column)
        {
        }

        public void SortValues()
        {
            Expressions.Sort((x, y) => ((x as ASTNode).Line <= (y as ASTNode).Line && (x as ASTNode).Column <= (y as ASTNode).Column) ? 1 : -1);
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
