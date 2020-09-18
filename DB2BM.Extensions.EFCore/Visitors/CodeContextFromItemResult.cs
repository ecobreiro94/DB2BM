using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.Entities;
using DB2BM.Extensions.BusinessGenerator.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.EFCore.Visitors
{
    public class CodeContextFromItemResult : CodeContext
    {
        public List<ExpressionNode> JoinExpressions { get; set; }
        public List<string> TablesName { get; set; }
        public int Count { get; set; }
    }
}
