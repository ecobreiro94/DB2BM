using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Entities.UserDefined;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.BusinessGenerator
{
    public class DbContextTemplateParam
    {
        public List<Table> Tables { get; set; }
        public List<Sequence> Sequences { get; set; }
        public List<BaseUserDefinedType> UserDefinedTypes { get; set; }
        public string Name { get; set; }
    }
}
