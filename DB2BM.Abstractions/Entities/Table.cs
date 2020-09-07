using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.Entities
{
    public class Table
    {
        public string Name { get; set; }

        public List<TableField> Fields { get; set; }

        public override string ToString()
        {
            var consoleString = Name + "(";
            foreach (var f in Fields)
            {
                consoleString += f.ToString() + ",";
            }
            consoleString += ")";
            return consoleString;
        }

        public bool MultipleKeys { get; set; }

        public bool HasNonKeys
        {
            get => (KeysName == null || KeysName.Count == 0) ? true : false;
        }

        public List<string> KeysName { get; set; }

        public IEnumerable<TableField> NoRelationalField
        {
            get
            {
                foreach (var f in Fields)
                {
                    if (f.IsProperty || f.IsPrimaryKey || f.IsForeingKey) yield return f;
                }
            }
        }
    }
}
