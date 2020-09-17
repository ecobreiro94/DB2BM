using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.BusinessGenerator.Visitors
{
    public class CodeContext
    {
        public List<StoredProcedure> InternalFunctionUse { get; set; }

        public string Code { get; set; }

        public bool UserFunctionCall { get; set; }
    }

}
