using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DB2BM.Abstractions.Entities;

namespace DB2BM.Utils
{
    public class ReviewEntities
    {
        public static void ReviewKeys(DatabaseCatalog catalog)
        {
            foreach (var t in catalog.Tables)
            {
                var keys = catalog.Relationships.Where(r =>
                    r.Table.Name == t.Value.Name && (r.Type == RelationshipType.PrimaryKey || r.Type == RelationshipType.ForeingKey)).ToList();
                if (keys.Count > 1)
                {
                    t.Value.MultipleKeys = true;

                    var keysNames = new HashSet<string>();
                    foreach (var k in keys)
                        keysNames.Add(k.Column.Name);

                    t.Value.KeysName = new List<string>(keysNames);
                }
                else if (keys.Count == 1)
                {
                    t.Value.MultipleKeys = false;
                    t.Value.KeysName = new List<string>() { keys[0].Column.Name };
                }
            }
        }
        public static void PushRelations(DatabaseCatalog dataBase)
        {
            ReviewKeys(dataBase);
            
            foreach (var r in dataBase.Relationships)
            {
                if (r.Type == RelationshipType.PrimaryKey)
                {
                    dataBase.Tables.Values.First(t => t.Name == r.Table.Name)
                        .Fields.First(f => f.Name == r.Column.Name)
                        .Attribute = AttributeField.Property;
                }
                if (r.Type == RelationshipType.ForeingKey)
                {
                    var table = dataBase.Tables.Values.First(t => t.Name == r.Table.Name);

                    var name = $"R_{r.ReferenceTable.Name}".ToPascal();
                    table.Fields.Add(new TableField()
                    {
                        DestinyType = r.ReferenceTable.Name.ToPascal(),
                        Name = name,
                        GenName = name,
                        FieldRelation = $"{r.Table.Name}s",
                        Attribute = AttributeField.RelationManyOne
                    });
                    table.Fields.First(f => f.Name == r.Column.Name).Attribute = AttributeField.ForeingKey;
                    var referenceTable = dataBase.Tables.Values.First(t => t.Name == r.ReferenceTable.Name);
                    name = $"{r.Table.Name}s".ToPascal();
                    referenceTable.Fields.Add(new TableField()
                    {
                        DestinyType = $"ICollection<{r.Table.Name.ToPascal()}>",
                        Name = name,
                        GenName = name,
                        FieldRelation = $"R_{r.ReferenceTable.Name}",
                        Attribute = AttributeField.RelationOneMany
                    });
                }
            }
        }
    }
}
