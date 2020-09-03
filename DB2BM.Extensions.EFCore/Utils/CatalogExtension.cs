using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Entities.UserDefined;
using DB2BM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DB2BM.Extensions.Utils
{
    public static class CatalogExtension
    {
        private static string GetType(string type, Dictionary<string, string> typesMapper)
        {
            if (typesMapper.ContainsKey(type))
                return typesMapper[type];

            else if (type.Length > 0 && type[0] == '_')
            {
                var simpleType = new string(type.Skip(1).ToArray());
                if (typesMapper.ContainsKey(simpleType))
                    return typesMapper[simpleType] + "[]";
            }

            return type;
        }

        private static bool prepareCatalog;
        private static void PrepareTables(this DatabaseCatalog catalog, Dictionary<string, string> typesMapper)
        {
            foreach (var t in catalog.Tables.Values)
            {
                foreach (var f in t.Fields)
                {
                    f.Table = t;
                    var udtypes = catalog.UserDefinedTypes.Values.ToList();
                    var udt = udtypes.Find(x => x.TypeName == f.OriginType);
                    if (udt != null)
                    {
                        if (udt is UserDefinedType)
                            f.IsUDT = true;
                        else
                            f.IsUDTEnum = true;
                        f.DestinyType = f.OriginType;
                        continue;
                    }
                    if (f.OriginType != null && f.OriginType.Length > 0 && f.OriginType[0] == '_')
                    {
                        var type = new string(f.OriginType.Skip(1).ToArray());
                        if (udtypes.Exists(x => x.TypeName == type))
                        {
                            f.DestinyType = type += "[]";
                            f.OwnsMany = true;
                            continue;
                        }
                    }
                    if (f.DestinyType == null || f.DestinyType == f.OriginType)
                        f.DestinyType = GetType(f.OriginType, typesMapper);
                }
            }
        }

        private static void PrepareUdts(this DatabaseCatalog catalog, Dictionary<string, string> typesMapper)
        {
            var userDefineds = catalog.UserDefinedTypes.Values.ToList();
            foreach (var u in userDefineds)
            {
                if (u is UserDefinedType)
                {
                    var udt = u as UserDefinedType;
                    foreach (var f in udt.Fields)
                    {
                        f.Udt = udt;
                        var t = userDefineds.Find(x => x.TypeName == f.OriginType);
                        var type = new string(f.OriginType.Skip(1).ToArray());
                        if (t != null)
                        {
                            if (t is UserDefinedType) f.IsUDT = true;
                            else f.IsUDTEnum = true;
                        }
                        else if (f.OriginType.Length > 0 && f.OriginType[0] == '_' && userDefineds.Exists(x => x.TypeName == type))
                        {
                            f.DestinyType = type += "[]";
                            f.OwnsMany = true;
                        }
                        else f.DestinyType = GetType(f.OriginType, typesMapper);
                    }
                }
            }
        }

        private static void ReviewKeys(DatabaseCatalog catalog)
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
        private static void PushRelations(DatabaseCatalog catalog)
        {
            ReviewKeys(catalog);

            foreach (var r in catalog.Relationships)
            {
                if (r.Type == RelationshipType.PrimaryKey)
                {
                    catalog.Tables.Values.First(t => t.Name == r.Table.Name)
                        .Fields.First(f => f.Name == r.Column.Name)
                        .Attribute = AttributeField.Property;
                }
                if (r.Type == RelationshipType.ForeingKey)
                {
                    var table = catalog.Tables.Values.First(t => t.Name == r.Table.Name);

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
                    var referenceTable = catalog.Tables.Values.First(t => t.Name == r.ReferenceTable.Name);
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

        private static void PrepareFunctions(DatabaseCatalog catalog, Dictionary<string, string> typesMapper)
        {
            var userDefineds = catalog.UserDefinedTypes.Values.ToList();
            var tables = catalog.Tables.Values.ToList();
            foreach (var sp in catalog.StoreProcedures.Values)
            {
                var type = new string(sp.ReturnType.Skip(1).ToArray());
                if (sp.ReturnType.Length > 0 && sp.ReturnType[0] == '_' && userDefineds.Exists(x => x.TypeName == type))
                    sp.ReturnType = type + "[]";
                else if (sp.ReturnType.Length > 0 && sp.ReturnType[0] == '_' && tables.Exists(x => x.Name == type))
                    sp.ReturnType = type.ToPascal() + "[]";
                else if (sp.ReturnType.Length > 0 && sp.ReturnType[0] == '_' && typesMapper.ContainsKey(type))
                    sp.ReturnType = type + "[]";
                else if (typesMapper.ContainsKey(sp.ReturnType))
                    sp.ReturnType = typesMapper[sp.ReturnType];

                foreach (var p in sp.Params)
                {
                    if (p.Name == null)
                        p.Name = "p" + p.OrdinalPosition;
                    type = new string(p.OriginType.Skip(1).ToArray());
                    if (p.OriginType.Length > 0 && p.OriginType[0] == '_' && userDefineds.Exists(x => x.TypeName == type))
                        p.DestinyType = type += "[]";
                    else if (p.OriginType.Length > 0 && p.OriginType[0] == '_' && tables.Exists(x => x.Name == type))
                        p.DestinyType = type.ToPascal() + "[]";
                    else if (p.OriginType.Length > 0 && p.OriginType[0] == '_' && typesMapper.ContainsKey(type))
                        p.DestinyType = type + "[]";
                    else if (typesMapper.ContainsKey(p.OriginType))
                        p.DestinyType = typesMapper[p.OriginType];
                }

                var paramsOutMode = sp.Params.FindAll(p => p.OutMode);
                if (paramsOutMode.Count == 1)
                    sp.ReturnType = paramsOutMode.First().DestinyType;
                else if (paramsOutMode.Count > 1)
                {
                    var returnType = "";
                    foreach (var p in paramsOutMode)
                        returnType += (returnType == "") ? p.DestinyType : "," + p.DestinyType;
                    sp.ReturnType = "(" + returnType + ")";
                }

                if (sp.ReturnClause.ToLower().Contains("setof "))
                    sp.ReturnType = "IEnumerable<" + sp.ReturnType + ">";
            }
            foreach (var function in catalog.InternalFunctions.Values)
            {
                if (typesMapper.ContainsKey(function.ReturnType))
                    function.ReturnType = typesMapper[function.ReturnType];
                foreach (var parameter in function.Params)
                {
                    if (typesMapper.ContainsKey(parameter.OriginType))
                        parameter.DestinyType = typesMapper[parameter.OriginType];
                    else
                        parameter.DestinyType = parameter.OriginType;
                }
            }
        }

        public static void PrepareCatalog(this DatabaseCatalog catalog,Dictionary<string, string> typesMapper)
        {
            PrepareTables(catalog, typesMapper);
            PrepareUdts(catalog, typesMapper);
            PrepareFunctions(catalog, typesMapper);
            PushRelations(catalog);
        }
    }
}
