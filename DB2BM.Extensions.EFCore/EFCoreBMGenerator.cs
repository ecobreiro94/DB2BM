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
        #region Campos y Propiedades Privados 

        private bool prepareCatalog;
        private DatabaseCatalog catalog;

        private TemplateGroupString functionsTemplate;
        private TemplateGroupString internalFunctionsTemplate;
        private TemplateGroupString modelTemplate;
        private TemplateGroupString dbContextTemplate;
        private TemplateGroupString dbContextExtensionTemplate;

        private Dictionary<string, string> TypesMapper
        {
            get => SemanticAnalyzer.TypesMapper;
        }

        #endregion

        #region Propiedades Públicas para la configuración del Generador 

        public ISemanticAnalyzer SemanticAnalyzer { get; set; }
        public ISyntacticAnalyzer SyntacticAnalyzer { get; set; }

        public string OutputPathService { get; private set; }
        public string OutputPathContext { get; private set; }
        public string OutputPathModel { get; private set; }

        #endregion

        #region Implementación de la interfaz IBMGenerator

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

        public void SetOutputPath(string path, bool isProject)
        {
            if (!isProject)
            {
                OutputPathContext = path;
                OutputPathModel = path;
                OutputPathService = path;
            }
            else
            {
                OutputPathContext = Path.Combine(path, "Context");
                OutputPathModel = Path.Combine(path, "Model");
                OutputPathService = Path.Combine(path, "Service");
            }
        }

        public void GenerateService(string className)
        {
            GenerateDatabaseFunctions(className, null);
        }

        public void GenerateContext()
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
            Write(OutputPathContext, Catalog.Name.ToPascal() + "DbContext.cs", r);
        }

        public void GenerateEntities()
        {
            foreach (var item in Catalog.Tables.Values)
            {
                var temp = ModelTemplate.GetInstanceOf("gen_model");
                temp.Add("db", new ModelTemplateParams() { NameSpace = Catalog.Name, Table = item });
                ModelTemplate.RegisterRenderer(typeof(string), new CSharpRenderer(), true);
                var r = temp.Render();
                Write(OutputPathModel, item.Name.ToPascal() + ".cs", r);
            }
            foreach (var item in Catalog.UserDefinedTypes.Values)
            {
                if (item is UserDefinedEnumType)
                {
                    var temp = ModelTemplate.GetInstanceOf("gen_enum");
                    temp.Add("db", new ModelTemplateParams() { NameSpace = Catalog.Name, Enum = (UserDefinedEnumType)item });
                    ModelTemplate.RegisterRenderer(typeof(string), new CSharpRenderer(), true);
                    var r = temp.Render();
                    Write(OutputPathModel, item.TypeName.ToPascal() + ".cs", r);
                }
                if (item is UserDefinedType)
                {
                    var temp = ModelTemplate.GetInstanceOf("gen_complex_type");
                    temp.Add("db", new ModelTemplateParams() { NameSpace = Catalog.Name, UDT = (UserDefinedType)item });
                    ModelTemplate.RegisterRenderer(typeof(string), new CSharpRenderer(), true);
                    var r = temp.Render();
                    Write(OutputPathModel, item.TypeName.ToPascal() + ".cs", r);
                }
            }
        }

        public void GenerateService(string className, List<string> functionNames)
        {
            GenerateDatabaseFunctions(className, functionNames);
        }

        #endregion

        #region Plantillas para la Generación de Código

        public TemplateGroupString FunctionsTemplate
        {
            get
            {
                if (functionsTemplate == null)
                    functionsTemplate = new TemplateGroupString(File.ReadAllText(@"Templates\functions.st4"));
                return functionsTemplate;
            } 
        }

        public TemplateGroupString DbContextExtensionTemplate
        {
            get
            {
                if (dbContextExtensionTemplate == null)
                    functionsTemplate = new TemplateGroupString(File.ReadAllText(@"Templates\additional_template.st4"));
                return functionsTemplate;
            }
        }

        public TemplateGroupString InternalFunctionsTemplate
        {
            get
            {
                if (internalFunctionsTemplate == null)
                    internalFunctionsTemplate = new TemplateGroupString(File.ReadAllText(@"Templates\partial_dbcontext.st4"));
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

        #endregion

        #region Métodos Privados

        private List<StoredProcedure> SelectFunctions(List<string> functionNames)
        {
            if (functionNames == null)
                return Catalog.StoredProcedures.Values.ToList();
            return Catalog.StoredProcedures.Values.Where(f => functionNames.Contains(f.Name)).ToList();
        }

        private void InsertErrors(IEnumerable<SemanticResult> errors, StoredProcedure Sp)
        {
            foreach (var error in errors)
                Sp.GeneratedCode += "//" + (error as ErrorResult).Menssage + "\n";
            Sp.GeneratedCode += "throw new NotImplementedException();";
        }

        private void GenerateDatabaseFunctions(string className, List<string> functionNames)
        {
            var functions = SelectFunctions(functionNames);
            var visitFunction = new List<StoredProcedure>();
            var internalFunctionUse = new List<StoredProcedure>();
            if (functions.Count == Catalog.StoredProcedures.Count)
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
                        var codeContext = f.AST.Accept(genCodeVisitor);
                        f.GeneratedCode = codeContext.Code;
                        internalFunctionUse.AddRange(codeContext.InternalFunctionUse);
                    }
                    visitFunction.Add(f);
                }
            }
            else
            {
                var functionsQueue = new Queue<StoredProcedure>(functions);
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
                        var codeContext = f.AST.Accept(genCodeVisitor);
                        f.GeneratedCode = codeContext.Code;
                        internalFunctionUse.AddRange(codeContext.InternalFunctionUse);
                    }
                }
            }
            var temp = FunctionsTemplate.GetInstanceOf("gen_functions");
            temp.Add("db", new FunctionsTemplateParams()
            {
                NameSpace = Catalog.Name,
                ClassName = className,
                Functions = visitFunction
            });
            FunctionsTemplate.RegisterRenderer(typeof(string), new CSharpRenderer(), true);
            var r = temp.Render();
            Write(OutputPathService, className + ".cs", r);
            GenerateInternalFunctions(internalFunctionUse);
        }

        private void GenerateInternalFunctions(List<StoredProcedure> internalFunctions)
        {
            var temp = DbContextExtensionTemplate.GetInstanceOf("gen_context");
            temp.Add("arg", Catalog.Name.ToPascal());
            DbContextExtensionTemplate.RegisterRenderer(typeof(string), new CSharpRenderer(), true);
            var r = temp.Render();
            Write(OutputPathContext, Catalog.Name.ToPascal() + "DbContext.Extensions.cs", r);

            if (internalFunctions?.Count > 0)
            {
                var functions = internalFunctions.Distinct();
                var bodys = JsonConvert.DeserializeObject<Dictionary<string, string>>
                    (File.ReadAllText("internal_functions_body.json"));
                foreach (var function in functions)
                {
                    if (bodys.ContainsKey(function.Name))
                    {
                        if (bodys[function.Name] != null)
                            function.GeneratedCode = bodys[function.Name];
                        else
                            function.GeneratedCode = "throw new NotImplementedException();";
                    }
                    else
                        function.GeneratedCode = "throw new NotImplementedException();";
                }
                temp = InternalFunctionsTemplate.GetInstanceOf("gen_context");
                temp.Add("arg", new InternalFunctionTemplateParams()
                {
                    Functions = functions.ToList(),
                    Name = Catalog.Name
                });
                InternalFunctionsTemplate.RegisterRenderer(typeof(string), new CSharpRenderer(), true);
                r = temp.Render();
                Write(OutputPathContext, Catalog.Name.ToPascal() + "DbContext.Service.cs", r);
            }
        }

        private void Write(string path, string csName, string code)
        {
            if (!File.Exists(path))
                Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, csName), code);
        }

        #endregion
    }
}
