using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Attrs;
using DB2BM.Abstractions.Entities;

namespace DB2BM.Abstractions.Interfaces
{
    public interface ISyntacticAnalyzer
    {
        void Parse(StoredProcedure sp);
    }
}
