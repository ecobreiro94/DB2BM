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
    public class EFCoreCodeGenerator : IGenerator
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

        public Dictionary<string, string> TypesMapper
        {
            get => SemanticVisitor.TypesMapper;
        }

        public void GenerateSPs(string className)
        {
            Catalog.PrepareCatalog(TypesMapper);
            GenerateDatabaseFunctions(className, null);
        }

        private List<StoreProcedure> SelectFunctions(List<string> functionNames)
        {
            if(functionNames == null)
                return Catalog.StoreProcedures.Values.ToList();
            return Catalog.StoreProcedures.Values.Where(f => functionNames.Contains(f.Name)).ToList();
        }
        private void InsertErrors(List<string> errors, StoreProcedure Sp)
        {
            foreach (var error in errors)
                Sp.BMDefinition += error + "\n";
        }

        private void GenerateDatabaseFunctions(string className, List<string> functionNames)
        {
            var temp = FunctionsTemplate.GetInstanceOf("gen_functions");

            var functions = SelectFunctions(functionNames);
            var internalFunctionUse = new List<StoreProcedure>();
            foreach (var f in functions)
            {
                var semanticVisitor = new SemanticVisitor(Catalog, f);
                var errors = f.Definition.Accept(semanticVisitor);
                if (errors.Count > 0)
                    InsertErrors(errors, f);
                else
                {
                    var genCodeVisitor = new EFCoreCodeGenVisitor(Catalog, f);
                    var codeContext = f.Definition.Accept(genCodeVisitor);
                    f.BMDefinition = codeContext.Code;
                    internalFunctionUse.AddRange(codeContext.InternalFunctionUse);

                }
            }
            temp.Add("db", new FunctionsTemplateParams()
                                                {
                                                    NameSpace = Catalog.Name,
                                                    ClassName = className ,
                                                    Functions = functions
                                                });
            FunctionsTemplate.RegisterRenderer(typeof(string), new CSharpRenderer(), true);
            var r = temp.Render();
            File.WriteAllText(OutputPath + @"\" + className +".cs", r);
            GenerateInternalFunctions(internalFunctionUse);
        }

        private void GenerateInternalFunctions(List<StoreProcedure> internalFunctions)
        {
            //var temp = InternalFunctionsTemplate.GetInstanceOf("gen_doc");
            //temp.Add("db", new FunctionsTemplateParams() { NameSpace = Catalog.Name, LikeBody = a });
            //InternalFunctionsTemplate.RegisterRenderer(typeof(string), new CSharpRenderer(), true);
            //var r = temp.Render();

            //File.WriteAllText(OutputPath + @"\InternalFunctions.cs", r);
        }

        public void GenerateDbContext()
        {
            Catalog.PrepareCatalog(TypesMapper);
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
            Catalog.PrepareCatalog(TypesMapper);
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
            Catalog.PrepareCatalog(TypesMapper);
            GenerateDatabaseFunctions(className, functionNames);
        }
    }
}
