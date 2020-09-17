using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Entities.UserDefined;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.BusinessGenerator
{
    class ModelTemplateParams
    {
        public string NameSpace { get; set; }

        public Table Table { get; set; }

        public UserDefinedEnumType Enum { get; set; }

        public UserDefinedType UDT { get; set; }
    }
}
