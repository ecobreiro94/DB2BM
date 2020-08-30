using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Antlr4.StringTemplate;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Attrs;
using DB2BM.Abstractions.Entities;
using DB2BM.Extensions.Utils;
using DB2BM.Abstractions.Entities.UserDefined;
using Newtonsoft.Json;
using DB2BM.Utils;
using DB2BM.Extensions.EFCore.Utils;
using DB2BM.Abstractions;
using DB2BM.Extensions.EFCore.Visitors;

namespace DB2BM.Extensions
{
    [Orm("efcore")]
    public class ProjectGenerator : IGenerator
    {
        public DatabaseCatalog Catalog { get; set; }

        public string OutputPath { get; set; }

        
        private TemplateGroupString functionsTemplate;
        private TemplateGroupString internalFunctionsTemplate;
        private TemplateGroupString modelTemplate;
        private TemplateGroupString dbContextTemplate;

        public TemplateGroupString FunctionsTemplate
        {
            get
            {
                if (functionsTemplate == null)
                    functionsTemplate = new TemplateGroupString(File.ReadAllText(@"Templates\functions.st4"));
                return functionsTemplate;
            } 
        }

        public TemplateGroupString InternalFunctionsTemplate
        {
            get
            {
                if (internalFunctionsTemplate == null)
                    internalFunctionsTemplate = new TemplateGroupString(File.ReadAllText(@"Templates\internal_functions.st4"));
                return internalFunctionsTemplate;
            }
        }

        public TemplateGroupString ModelTemplate
        {
            get
            {
                if(modelTemplate == null)
                    modelTemplate = new TemplateGroupString(File.ReadAllText(@"Templates\model.st4"));
                return modelTemplate;
            }
        }

        public TemplateGroupString DbContextTemplate
        {
            get
            {
                if(dbContextTemplate == null)
                    dbContextTemplate = new TemplateGroupString(File.ReadAllText(@"Templates\dbcontext.st4"));
                return dbContextTemplate;
            }
        }

        private Dictionary<string, string> typeMapper;
        public Dictionary<string, string> TypesMapper
        {
            get
            {
                if (typeMapper == null)
                    typeMapper = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("typesMapper.json"));
                return typeMapper;
            }
        }

        private bool prepareCatalog;
        private void PrepareTables()
        {
            foreach (var t in Catalog.Tables.Values)
            {
                foreach (var f in t.Fields)
                {
                    var udtypes = Catalog.UserDefinedTypes.Values.ToList();
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
                        f.DestinyType = CSharpTools.GetCSharpType(f.OriginType, TypesMapper);
                }
            }
        }
        private void PrepareFunctions()
        {
            var userDefineds = Catalog.UserDefinedTypes.Values.ToList();
            var tables = Catalog.Tables.Values.ToList();
            foreach (var f in Catalog.StoreProcedures.Values)
            {
                var type = new string(f.ReturnType.Skip(1).ToArray());
                if (f.ReturnType.Length > 0 && f.ReturnType[0] == '_' && userDefineds.Exists(x => x.TypeName == type))
                    f.ReturnType = type + "[]";
                else if (f.ReturnType.Length > 0 && f.ReturnType[0] == '_' && tables.Exists(x => x.Name == type))
                    f.ReturnType = type.ToPascal() + "[]";
                else if (tables.Exists(x => x.Name == f.ReturnType))
                    f.ReturnType = f.ReturnType.ToPascal();
                else f.ReturnType = CSharpTools.GetCSharpType(f.ReturnType, TypesMapper);

                foreach (var p in f.Params)
                {
                    if (p.Name == null)
                        p.Name = "p" + p.OrdinalPosition;
                    type = new string(p.OriginType.Skip(1).ToArray());
                    if (p.OriginType.Length > 0 && p.OriginType[0] == '_' && userDefineds.Exists(x => x.TypeName == type))
                        p.DestinyType = type += "[]";
                    else if (p.OriginType.Length > 0 && p.OriginType[0] == '_' && tables.Exists(x => x.Name == type))
                        p.DestinyType = type.ToPascal() + "[]";
                    else p.DestinyType = CSharpTools.GetCSharpType(p.OriginType, TypesMapper);
                }

                var paramsOutMode = f.Params.FindAll(p => p.OutMode);
                if (paramsOutMode.Count == 1)
                    f.ReturnType = paramsOutMode.First().DestinyType;
                else if (paramsOutMode.Count > 1)
                {
                    var returnType = "";
                    foreach (var p in paramsOutMode)
                        returnType += (returnType == "") ? p.DestinyType : "," + p.DestinyType;
                    f.ReturnType = "(" + returnType + ")";
                }

                if (f.ReturnClause.ToLower().Contains("setof "))
                    f.ReturnType = "IEnumerable<" + f.ReturnType + ">";
            }
        }
        private void PrepareUdts()
        {
            var userDefineds = Catalog.UserDefinedTypes.Values.ToList();
            foreach (var u in userDefineds)
            {
                if (u is UserDefinedType)
                {
                    var udt = u as UserDefinedType;
                    foreach (var f in udt.Fields)
                    {
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
                        else f.DestinyType = CSharpTools.GetCSharpType(f.OriginType, TypesMapper);
                    }
                }
            }
        }
        private void PrepareCatalog()
        {
            if (!prepareCatalog)
            {
                PrepareTables();
                PrepareFunctions();
                PrepareUdts();
                prepareCatalog = true;
            }
        }

        public void GenerateSPs(string className)
        {
            PrepareCatalog();
            GenerateDatabaseFunctions(className, null);
            GenerateInternalFunctions();
        }

        private List<StoreProcedure> SelectFunctions(List<string> functionNames)
        {
            if(functionNames == null)
                return Catalog.StoreProcedures.Values.ToList();
            return Catalog.StoreProcedures.Values.Where(f => functionNames.Contains(f.Name)).ToList();
        }

        private void GenerateDatabaseFunctions(string className, List<string> functionNames)
        {
            PrepareCatalog();
            var temp = FunctionsTemplate.GetInstanceOf("gen_functions");

            var functions = SelectFunctions(functionNames);

            foreach (var f in functions)
            {
                var semanticVisitor = new SemanticVisitor(Catalog, f);
                semanticVisitor.VisitNode(f.Definition);
                var genCodeVisitor = new GenCodeVisitor(Catalog, f);
                f.BMDefinition = genCodeVisitor.VisitNode(f.Definition);
            }

            temp.Add("db", new FunctionsTemplateParams() { NameSpace = Catalog.Name, ClassName = className ,Functions = functions });
            FunctionsTemplate.RegisterRenderer(typeof(string), new CSharpRenderer(), true);
            var r = temp.Render();
            File.WriteAllText(OutputPath + @"\" + className +".cs", r);
        }

        private void GenerateInternalFunctions()
        {
            string doubleQuote = ((char)34).ToString();
            string simpleQuote = ((char)39).ToString();
            string backSlach = ((char)92).ToString();
            var a = "return new Regex(@" + doubleQuote + backSlach + "A" + doubleQuote + " + new Regex(@" + doubleQuote +
                backSlach + ".|" +
                backSlach + "$|" +
                backSlach + "^|" +
                backSlach + "{|" +
                backSlach + "[|" +
                backSlach + "(|" +
                backSlach + "||" +
                backSlach + ")|" +
                backSlach + "*|" +
                backSlach + "+|" +
                backSlach + "?|" +
                backSlach + backSlach + doubleQuote + ")" +
                ".Replace(toFind, ch => @" + doubleQuote + backSlach + doubleQuote + " + ch)" +
                ".Replace('_', '.')" +
                ".Replace(" + doubleQuote + "%" + doubleQuote + ", " + doubleQuote + ".*" + doubleQuote + ") + @" + doubleQuote + backSlach + "z" + doubleQuote + ", RegexOptions.Singleline)" +
                ".IsMatch(toSearch);";

            
            var temp = InternalFunctionsTemplate.GetInstanceOf("gen_doc");
            temp.Add("db", new FunctionsTemplateParams() { NameSpace = Catalog.Name, LikeBody = a });
            InternalFunctionsTemplate.RegisterRenderer(typeof(string), new CSharpRenderer(), true);
            var r = temp.Render();

            File.WriteAllText(OutputPath + @"\InternalFunctions.cs", r);
        }

        public void GenerateDbContext()
        {
            PrepareCatalog();
            var temp = DbContextTemplate.GetInstanceOf("gen_context");
            DbContextTemplate.RegisterRenderer(typeof(string), new CSharpRenderer(), true);
            var parameters = new DbContextTemplateParam()
            {
                Name = Catalog.Name,
                Sequences = Catalog.Sequences.Values.ToList(),
                Tables = Catalog.Tables.Values.ToList(),
                UserDefinedTypes = Catalog.UserDefinedTypes.Values.ToList()
            };
            temp.Add("db", parameters);
            var r = temp.Render();
            File.WriteAllText(this.OutputPath + @"\" + Catalog.Name.ToPascal() + "DbContext.cs", r);
        }

        public void GenerateEntities()
        {
            PrepareCatalog();
            foreach (var item in Catalog.Tables.Values)
            {
                var temp = ModelTemplate.GetInstanceOf("gen_model");
                temp.Add("db", new ModelTemplateParams() { NameSpace = Catalog.Name, Table = item });
                ModelTemplate.RegisterRenderer(typeof(string), new CSharpRenderer(), true);
                var r = temp.Render();
                File.WriteAllText(this.OutputPath + @"\" + item.Name.ToPascal() + ".cs", r);
            }
            foreach (var item in Catalog.UserDefinedTypes.Values)
            {
                if (item is UserDefinedEnumType)
                {
                    var temp = ModelTemplate.GetInstanceOf("gen_enum");
                    temp.Add("db", new ModelTemplateParams() { NameSpace = Catalog.Name, Enum = (UserDefinedEnumType)item });
                    ModelTemplate.RegisterRenderer(typeof(string), new CSharpRenderer(), true);
                    var r = temp.Render();
                    File.WriteAllText(this.OutputPath + @"\" + item.TypeName.ToPascal() + ".cs", r);
                }
                if (item is UserDefinedType)
                {
                    var temp = ModelTemplate.GetInstanceOf("gen_complex_type");
                    temp.Add("db", new ModelTemplateParams() { NameSpace = Catalog.Name, UDT = (UserDefinedType)item });
                    ModelTemplate.RegisterRenderer(typeof(string), new CSharpRenderer(), true);
                    var r = temp.Render();
                    File.WriteAllText(this.OutputPath + @"\" + item.TypeName.ToPascal() + ".cs", r);
                }
            }
        }

        public void GenerateSPs(string className, List<string> functionNames)
        {
            PrepareCatalog();
            GenerateDatabaseFunctions(className, functionNames);
        }
    }
}
