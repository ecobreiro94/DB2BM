using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.Attrs
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OrmAttribute : Attribute
    {
        public string Name { get; } 
        public OrmAttribute(string name) { Name = name; }
    }
}
