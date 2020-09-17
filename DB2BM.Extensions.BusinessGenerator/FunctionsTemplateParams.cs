using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Entities;

namespace DB2BM.Extensions.BusinessGenerator
{
    public class InternalFunctionTemplateParams
    {
        public string Name { get; set; }
        public List<StoredProcedure> Functions { get; set; }
    }

    public class FunctionsTemplateParams
    {
        public string NameSpace { get; set; }

        public string ClassName { get; set; }

        public List<StoredProcedure> Functions { get; set; }

        public StoredProcedure Function { get; set; }

        public string LikeBody { get; set; }
    }
}
