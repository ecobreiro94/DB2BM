using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Entities;

namespace DB2BM.Abstractions.Entities.UserDefined
{
    public class UserDefinedType : BaseUserDefinedType
    {
        public override bool IsUDT => true; 

        public override bool IsUDTEnum => false;

        public IEnumerable<UdtField> Fields { get; set; }
    }
}
