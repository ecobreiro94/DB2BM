using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.EFCore.Visitors
{
    public class CodeContext
    {
        public List<StoreProcedure> InternalFunctionUse { get; set; }

        public string Code { get; set; }

        public bool UserFunctionCall { get; set; }
    }

    public class CodeContextFromItemResult : CodeContext
    {
        public List<ExpressionNode> JionExpressions { get; set; }
        public List<string> TablesName { get; set; }
        public int Count { get; set; }
    }
}
