using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.Attrs
{
    [AttributeUsage(AttributeTargets.Class)]
    public class  DbmsAttr: Attribute
    {
        public string Name { get; }
        public DbmsAttr(string name) { Name = name; }
    }
}
