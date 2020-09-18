using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.Entities
{
    public enum RelationshipType { PrimaryKey, ForeingKey}

    public class Relationship
    {
        public Table Table { get; set; }

        public Table ReferencedTable { get; set; }

        public TableField Column { get; set; }

        public TableField ReferencedColumn { get; set; }

        public RelationshipType Type { get; set; }

        public override string ToString()
        {
            var text = "";
            if (Type == RelationshipType.PrimaryKey)
                text += "PRIMARY KEY ";
            else text += "FOREING KEY ";
            text += Table.Name + " " + Column.Name;
            return (Type == RelationshipType.PrimaryKey)?text : text + " reference " + ReferencedTable.Name + " " + ReferencedColumn.Name;
        }
    }
}
