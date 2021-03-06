﻿using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Entities.UserDefined;
using DB2BM.Extensions.EFCore.Visitors;
using DB2BM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DB2BM.Extensions.Utils
{
    public static class CatalogExtension
    {
        private static string GetType(BaseField field, Dictionary<string, string> typesMapper)
        {
            if (typesMapper.ContainsKey(field.OriginType))
                return typesMapper[field.OriginType];

            else if (field.OriginType.Length > 0 && field.OriginType[0] == '_')
            {
                var simpleType = new string(field.OriginType.Skip(1).ToArray());
                if (typesMapper.ContainsKey(simpleType))
                {
                    field.IsNullable = false;
                    return typesMapper[simpleType] + "[]";
                }
            }

            return field.OriginType;
        }

        private static bool prepareCatalog;
        private static void PrepareTables(this DatabaseCatalog catalog, Dictionary<string, string> typesMapper)
        {
            foreach (var t in catalog.Tables.Values)
            {
                if (t.Name == "film")
                {
                }
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
                        f.IsNullable = false;
                        continue;
                    }
                    if (f.OriginType != null && f.OriginType.Length > 0 && f.OriginType[0] == '_')
                    {
                        var type = new string(f.OriginType.Skip(1).ToArray());
                        if (udtypes.Exists(x => x.TypeName == type))
                        {
                            f.DestinyType = type += "[]";
                            f.IsNullable = false;
                            f.OwnsMany = true;
                            continue;
                        }
                    }
                    if (f.DestinyType == null || f.DestinyType == f.OriginType)
                        f.DestinyType = GetType(f, typesMapper);

                    if (EFCoreCodeGenVisitor.TypesMapper.ContainsKey(f.DestinyType) &&
                           !EFCoreCodeGenVisitor.TypesMapper[f.DestinyType].Contains('?'))
                    {
                        f.IsNullable = false;
                    }
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
                            f.IsNullable = false;
                        }
                        else f.DestinyType = GetType(f, typesMapper);
                        if (EFCoreCodeGenVisitor.TypesMapper.ContainsKey(f.DestinyType) &&
                           !EFCoreCodeGenVisitor.TypesMapper[f.DestinyType].Contains('?'))
                        {
                            f.IsNullable = false;
                        }
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

                    var name = $"R_{r.ReferencedTable.Name}".ToPascal();
                    table.Fields.Add(new TableField()
                    {
                        DestinyType = r.ReferencedTable.Name.ToPascal(),
                        Name = name,
                        GenName = name,
                        FieldRelation = $"{r.Table.Name}s",
                        Attribute = AttributeField.RelationManyOne
                    });
                    table.Fields.First(f => f.Name == r.Column.Name).Attribute = AttributeField.ForeingKey;
                    var referenceTable = catalog.Tables.Values.First(t => t.Name == r.ReferencedTable.Name);
                    name = $"{r.Table.Name}s".ToPascal();
                    referenceTable.Fields.Add(new TableField()
                    {
                        DestinyType = $"ICollection<{r.Table.Name.ToPascal()}>",
                        Name = name,
                        GenName = name,
                        FieldRelation = $"R_{r.ReferencedTable.Name}",
                        Attribute = AttributeField.RelationOneMany
                    });
                }
            }
        }

        private static void PrepareFunctions(DatabaseCatalog catalog, Dictionary<string, string> dbmsTypesMapper)
        {
            var userDefineds = catalog.UserDefinedTypes.Values.ToList();
            var tables = catalog.Tables.Values.ToList();
            foreach (var sp in catalog.StoredProcedures.Values)
            {
                var type = new string(sp.ReturnType.Skip(1).ToArray());
                if (sp.ReturnType.Length > 0 && sp.ReturnType[0] == '_' && userDefineds.Exists(x => x.TypeName == type))
                    sp.ReturnType = type + "[]";
                else if (sp.ReturnType.Length > 0 && sp.ReturnType[0] == '_' && tables.Exists(x => x.Name == type))
                    sp.ReturnType = type.ToPascal() + "[]";
                else if (sp.ReturnType.Length > 0 && sp.ReturnType[0] == '_' && dbmsTypesMapper.ContainsKey(type))
                    sp.ReturnType = type + "[]";
                else if (dbmsTypesMapper.ContainsKey(sp.ReturnType))
                    sp.ReturnType = dbmsTypesMapper[sp.ReturnType];
                else
                {
                    if (tables.Any(t => t.Name == sp.ReturnType))
                        sp.ReturnType = sp.ReturnType.ToPascal();
                }
                foreach (var p in sp.Params)
                {
                    if (p.Name == null)
                        p.Name = "p" + p.OrdinalPosition;
                    type = new string(p.OriginType.Skip(1).ToArray());
                    if (p.OriginType.Length > 0 && p.OriginType[0] == '_' && userDefineds.Exists(x => x.TypeName == type))
                        p.DestinyType = type += "[]";
                    else if (p.OriginType.Length > 0 && p.OriginType[0] == '_' && tables.Exists(x => x.Name == type))
                        p.DestinyType = type.ToPascal() + "[]";
                    else if (p.OriginType.Length > 0 && p.OriginType[0] == '_' && dbmsTypesMapper.ContainsKey(type))
                        p.DestinyType = dbmsTypesMapper[type] + "[]";
                    else if (dbmsTypesMapper.ContainsKey(p.OriginType))
                        p.DestinyType = dbmsTypesMapper[p.OriginType];
                }

                var paramsOutMode = sp.Params.FindAll(p => p.OutMode);
                if (paramsOutMode.Count == 1)
                    sp.ReturnType = paramsOutMode.First().DestinyType;
                else if (paramsOutMode.Count > 1)
                {
                    var returnType = "";
                    foreach (var p in paramsOutMode)
                        returnType += (returnType == "") ? 
                            EFCore.Visitors.EFCoreCodeGenVisitor.TypesMapper[p.DestinyType] :
                            "," + EFCore.Visitors.EFCoreCodeGenVisitor.TypesMapper[p.DestinyType];
                    sp.ReturnType = "(" + returnType + ")";
                }

                if (sp.ReturnIsSet)
                {
                    if (EFCore.Visitors.EFCoreCodeGenVisitor.TypesMapper.ContainsKey(sp.ReturnType))
                        sp.ReturnType = "IEnumerable<" + EFCore.Visitors.EFCoreCodeGenVisitor.TypesMapper[sp.ReturnType] + ">";
                    else sp.ReturnType = "IEnumerable<" + sp.ReturnType + ">";
                }
            }
            foreach (var function in catalog.InternalFunctions.Values)
            {
                if (dbmsTypesMapper.ContainsKey(function.ReturnType))
                    function.ReturnType = dbmsTypesMapper[function.ReturnType];
                foreach (var parameter in function.Params)
                {
                    if (dbmsTypesMapper.ContainsKey(parameter.OriginType))
                        parameter.DestinyType = dbmsTypesMapper[parameter.OriginType];
                    else
                        parameter.DestinyType = parameter.OriginType;
                    if (parameter.Name == null || parameter.Name == "")
                        parameter.Name = "p" + parameter.OrdinalPosition;
                }
            }
        }

        public static void PrepareCatalog(this DatabaseCatalog catalog, Dictionary<string, string> dbmsTypesMapper)
        {
            PrepareTables(catalog, dbmsTypesMapper);
            PrepareUdts(catalog, dbmsTypesMapper);
            PrepareFunctions(catalog, dbmsTypesMapper);
            PushRelations(catalog);
        }
    }
}
