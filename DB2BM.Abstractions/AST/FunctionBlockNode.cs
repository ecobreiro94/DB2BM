using DB2BM.Abstractions.AST.Declarations;
using DB2BM.Abstractions.Visitors;
using System.Collections.Generic;

namespace DB2BM.Abstractions.AST.Statements
{
    public class FunctionBlockNode : ASTNode
    {
        public List<DeclarationNode> Declarations { get; set; }
        public List<StatementNode> Statements { get; set; }
        public ExceptionStatementNode ExceptionStatement { get; set; }

        public FunctionBlockNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
