using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.Attrs
{
    [AttributeUsage(AttributeTargets.Class)]
    public class  DbmsAttribute: Attribute
    {
        public string Name { get; }
        public DbmsAttribute(string name) { Name = name; }
    }
}
