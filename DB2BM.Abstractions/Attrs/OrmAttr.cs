using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.Attrs
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OrmAttr : Attribute
    {
        public string Name { get; } 
        public OrmAttr(string name) { Name = name; }
    }
}
