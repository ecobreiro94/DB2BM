using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.Entities.UserDefined
{
    public class UserDefinedEnumType : BaseUserDefinedType
    {
        public override bool IsUDT => false;

        public override bool IsUDTEnum => true;

        public IEnumerable<string> Options { get; set; }
    }
}
