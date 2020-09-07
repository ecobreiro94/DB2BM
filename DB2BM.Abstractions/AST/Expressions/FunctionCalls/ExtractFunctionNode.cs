using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Expressions.FunctionCalls
{
    public class ExtractFunctionNode : FunctionCallNode
    {
        public IdNode Identifier { get; set; }
        public string CharacterString { get; set; }
        public ExpressionNode Expression { get; set; }
        public ExtractFunctionNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
