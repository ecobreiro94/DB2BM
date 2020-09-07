using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.Entities.UserDefined
{
    public abstract class BaseUserDefinedType
    {
        public string TypeName { get; set; }

        public virtual bool IsUDT { get; }

        public virtual bool IsUDTEnum { get; }
    }
}
