using DB2BM.Abstractions.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions
{
    public abstract class SemanticResult
    {
    }

    public class ErrorResult : SemanticResult
    {
        public string Menssage { get; set; }
        public ErrorResult(string msg)
        {
            Menssage = msg;
        }
    }

    public class FunctionResult : SemanticResult
    {
        public StoredProcedure Sp { get; set; }
    }
}
