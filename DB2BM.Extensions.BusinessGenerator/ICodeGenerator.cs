using DB2BM.Abstractions.Entities;
using DB2BM.Extensions.BusinessGenerator.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.BusinessGenerator
{
    public interface ICodeGenerator
    {
        DatabaseCatalog Catalog { get; set; }

        CodeContext GenerateBody(StoredProcedure sp);

        void PreparaParams(StoredProcedure sp);
    }
}
