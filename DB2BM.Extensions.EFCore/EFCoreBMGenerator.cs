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
    public class EFCoreBMGenerator : IBMGenerator
    {
        private bool prepareCatalog;
        private DatabaseCatalog catalog;
        public DatabaseCatalog Catalog
        {
            get
            {
                if (!prepareCatalog && catalog != null)
                {
                    catalog.PrepareCatalog(TypesMapper);
                    prepareCatalog = true;
                }
                return catalog;
            }
            set
            {
                catalog = value;
                prepareCatalog = false;
            }
        }

        public ISemanticAnalyzer SemanticAnalyzer { get; set; }
        public ISyntacticAnalyzer SyntacticAnalyzer { get; set; }

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
            get => SemanticAnalyzer.TypesMapper;
        }

        public void GenerateService(string className)
        {
            GenerateDatabaseFunctions(className, null);
        }

        private List<StoreProcedure> SelectFunctions(List<string> functionNames)
        {
            if(functionNames == null)
                return Catalog.StoreProcedures.Values.ToList();
            return Catalog.StoreProcedures.Values.Where(f => functionNames.Contains(f.Name)).ToList();
        }
        private void InsertErrors(IEnumerable<SemanticResult> errors, StoreProcedure Sp)
        {
            foreach (var error in errors)
                Sp.BMDefinition += "//" + (error as ErrorResult).Menssage + "\n";
        }

        private void GenerateDatabaseFunctions(string className, List<string> functionNames)
        {
            var temp = FunctionsTemplate.GetInstanceOf("gen_functions");
            var functions = SelectFunctions(functionNames);
            var visitFunction = new List<StoreProcedure>();
            var internalFunctionUse = new List<StoreProcedure>();
            if (functions.Count == Catalog.StoreProcedures.Count)
            {
                foreach (var f in functions)
                {
                    SyntacticAnalyzer.Parse(f);
                    var semanticResult = SemanticAnalyzer.Check(f);
                    var errors = semanticResult.Where(result => result is ErrorResult).ToList();
                    if (errors.Count > 0)
                        InsertErrors(errors.Where(e => e is ErrorResult), f);
                    else
                    {
                        var genCodeVisitor = new EFCoreCodeGenVisitor(Catalog, f);
                        var codeContext = f.Definition.Accept(genCodeVisitor);
                        f.BMDefinition = codeContext.Code;
                        internalFunctionUse.AddRange(codeContext.InternalFunctionUse);
                    }
                    visitFunction.Add(f);
                }
            }
            else
            {
                var functionsQueue = new Queue<StoreProcedure>(functions);
                while (functionsQueue.Count > 0)
                {
                    var f = functionsQueue.Dequeue();
                    visitFunction.Add(f);
                    SyntacticAnalyzer.Parse(f);
                    var semanticResult = SemanticAnalyzer.Check(f);
                    var errors = semanticResult.Where(e => e is ErrorResult).ToList();
                    if (errors.Count > 0)
                        InsertErrors(errors, f);
                    else
                    {
                        var newFunctions = semanticResult.Where(result => result is FunctionResult)
                            .Select(result => (result as FunctionResult).Sp);
                        foreach (var newFunction in newFunctions)
                        {
                            if (!visitFunction.Contains(newFunction) && !functionsQueue.Contains(newFunction))
                                functionsQueue.Enqueue(newFunction);
                        }
                        var genCodeVisitor = new EFCoreCodeGenVisitor(Catalog, f);
                        var codeContext = f.Definition.Accept(genCodeVisitor);
                        f.BMDefinition = codeContext.Code;
                        internalFunctionUse.AddRange(codeContext.InternalFunctionUse);
                    }
                }
            }
            temp.Add("db", new FunctionsTemplateParams()
                                                {
                                                    NameSpace = Catalog.Name,
                                                    ClassName = className ,
                                                    Functions = visitFunction
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

        public void GenerateService(string className, List<string> functionNames)
        {
            GenerateDatabaseFunctions(className, functionNames);
        }
    }
}
