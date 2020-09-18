using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Antlr4.StringTemplate;
using DB2BM.Abstractions;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Entities.UserDefined;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Extensions.BusinessGenerator.Utils;
using DB2BM.Extensions.BusinessGenerator.Visitors;
using DB2BM.Utils;

namespace DB2BM.Extensions.BusinessGenerator
{
    public enum ContextFile { Entities, InternalFunctionsSupport, ServiceSupport }

    public enum TemplateFile
    {
        Entity, ContextForEntities, ContextForServiceSupport, ContextForInternalFunctionsSupport, Service
    }

    public abstract class BusinessModelGenerator<TCodeGenerator> : IBMGenerator
        where TCodeGenerator : ICodeGenerator, new()
    {
        #region Campos y Propiedades Privados 

        string templatesPath;
        private bool preparedCatalog;
        protected DatabaseCatalog catalog;

        private TemplateGroupString functionsTemplate;
        private TemplateGroupString internalFunctionsTemplate;
        private TemplateGroupString modelTemplate;
        private TemplateGroupString dbContextTemplate;
        private TemplateGroupString dbContextExtensionTemplate;

        public BusinessModelGenerator()
        {
            templatesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Templates");
        }

        protected abstract Dictionary<string, string> TypesMapper { get; }

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
                if (!preparedCatalog && catalog != null)
                {
                    PrepareCatalog(catalog);
                    preparedCatalog = true;
                }
                return catalog;
            }
            set
            {
                catalog = value;
                preparedCatalog = false;
            }
        }

        public virtual void SetOutputPath(string path, bool isProject)
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

        public void GenerateContext()
        {
            var temp = DbContextTemplate.GetInstanceOf("gen_context");
            DbContextTemplate.RegisterRenderer(typeof(string), new CodeRenderer(), true);
            var parameters = new DbContextTemplateParam()
            {
                Name = Catalog.Name,
                Sequences = Catalog.Sequences.Values.ToList(),
                Tables = Catalog.Tables.Values.ToList(),
                UserDefinedTypes = Catalog.UserDefinedTypes.Values.ToList()
            };
            temp.Add("db", parameters);
            var r = temp.Render();
            Write(OutputPathContext, GetContextFilename(ContextFile.Entities), r);
        }

        public void GenerateEntities()
        {
            foreach (var item in Catalog.Tables.Values)
            {
                var temp = ModelTemplate.GetInstanceOf("gen_model");
                temp.Add("db", new ModelTemplateParams() { NameSpace = Catalog.Name, Table = item });
                ModelTemplate.RegisterRenderer(typeof(string), new CodeRenderer(), true);
                var r = temp.Render();
                Write(OutputPathModel, GetEntityFilename(item), r);
            }

            foreach (var item in Catalog.UserDefinedTypes.Values)
            {
                Template temp;
                switch (item)
                {
                    case UserDefinedEnumType enumItem:
                        temp = ModelTemplate.GetInstanceOf("gen_enum");
                        temp.Add("db", new ModelTemplateParams() { NameSpace = Catalog.Name, Enum = enumItem });
                        break;

                    case UserDefinedType udtItem:
                        temp = ModelTemplate.GetInstanceOf("gen_complex_type");
                        temp.Add("db", new ModelTemplateParams() { NameSpace = Catalog.Name, UDT = udtItem });
                        break;

                    default:
                        throw new NotSupportedException("Unsupported user defined type");
                }

                ModelTemplate.RegisterRenderer(typeof(string), new CodeRenderer(), true);
                var r = temp.Render();
                Write(OutputPathModel, GetUdtFilename(item), r);
            }
        }

        public void GenerateService(string className)
        {
            GenerateDatabaseFunctions(className, null);
        }

        public void GenerateService(string className, List<string> functionNames)
        {
            GenerateDatabaseFunctions(className, functionNames);
        }

        #endregion

        #region Plantillas para la Generación de Código

        public TemplateGroupString ServiceTemplate
        {
            get
            {
                if (functionsTemplate == null)
                    functionsTemplate = new TemplateGroupString(File.ReadAllText(Path.Combine(templatesPath, GetTemplateFilename(TemplateFile.Service))));
                return functionsTemplate;
            }
        }

        public TemplateGroupString ContextForServiceSupportTemplate
        {
            get
            {
                if (dbContextExtensionTemplate == null)
                    functionsTemplate = new TemplateGroupString(File.ReadAllText(Path.Combine(templatesPath, GetTemplateFilename(TemplateFile.ContextForServiceSupport))));
                return functionsTemplate;
            }
        }

        public TemplateGroupString ContextForInternalFunctionsTemplate
        {
            get
            {
                if (internalFunctionsTemplate == null)
                    internalFunctionsTemplate = new TemplateGroupString(File.ReadAllText(Path.Combine(templatesPath, GetTemplateFilename(TemplateFile.ContextForInternalFunctionsSupport))));
                return internalFunctionsTemplate;
            }
        }

        public TemplateGroupString ModelTemplate
        {
            get
            {
                if (modelTemplate == null)
                    modelTemplate = new TemplateGroupString(File.ReadAllText(Path.Combine(templatesPath, GetTemplateFilename(TemplateFile.Entity))));
                return modelTemplate;
            }
        }

        public TemplateGroupString DbContextTemplate
        {
            get
            {
                if (dbContextTemplate == null)
                    dbContextTemplate = new TemplateGroupString(File.ReadAllText(Path.Combine(templatesPath, GetTemplateFilename(TemplateFile.ContextForEntities))));
                return dbContextTemplate;
            }
        }

        #endregion


        #region Métodos Protegidos

        protected abstract void PrepareCatalog(DatabaseCatalog catalog);

        protected abstract string GetContextFilename(ContextFile contextFile);

        protected abstract string GetEntityFilename(Table item);

        protected abstract string GetUdtFilename(BaseUserDefinedType item);

        protected abstract string GetServiceFilename(string className);

        protected abstract string GetTemplateFilename(TemplateFile templateFile);

        protected abstract void InsertErrors(IEnumerable<SemanticResult> errors, StoredProcedure Sp);

        protected abstract Dictionary<string, string> GetInternalFunctionBodies();

        protected abstract string GetDefaultBody();

        #endregion

        #region Métodos Privados

        private List<StoredProcedure> SelectFunctions(List<string> functionNames)
        {
            if (functionNames == null)
                return Catalog.StoredProcedures.Values.ToList();
            return Catalog.StoredProcedures.Values.Where(f => functionNames.Contains(f.Name)).ToList();
        }

        private void GenerateDatabaseFunctions(string className, List<string> functionNames)
        {
            var functions = SelectFunctions(functionNames);
            var visitFunction = new List<StoredProcedure>();
            var internalFunctionUse = new List<StoredProcedure>();

            var codeGenerator = new TCodeGenerator()
            {
                Catalog = Catalog
            };

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
                        var codeContext = codeGenerator.GenerateBody(f);
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
                        var codeContext = codeGenerator.GenerateBody(f);
                        f.GeneratedCode = codeContext.Code;
                        internalFunctionUse.AddRange(codeContext.InternalFunctionUse);
                    }
                }
            }

            foreach (var f in visitFunction)
                codeGenerator.PreparaParams(f);

            var temp = ServiceTemplate.GetInstanceOf("gen_functions");
            temp.Add("db", new FunctionsTemplateParams()
            {
                NameSpace = Catalog.Name,
                ClassName = className,
                Functions = visitFunction
            });
            ServiceTemplate.RegisterRenderer(typeof(string), new CodeRenderer(), true);
            var r = temp.Render();
            Write(OutputPathService, GetServiceFilename(className), r);

            foreach (var f in internalFunctionUse)
                codeGenerator.PreparaParams(f);

            GenerateInternalFunctions(internalFunctionUse);
        }

        private void GenerateInternalFunctions(List<StoredProcedure> internalFunctions)
        {
            var temp = ContextForServiceSupportTemplate.GetInstanceOf("gen_context");
            temp.Add("arg", Catalog.Name.ToPascal());
            ContextForServiceSupportTemplate.RegisterRenderer(typeof(string), new CodeRenderer(), true);
            var r = temp.Render();
            Write(OutputPathContext, GetContextFilename(ContextFile.InternalFunctionsSupport), r);

            if (internalFunctions?.Count > 0)
            {
                var functions = internalFunctions.Distinct();
                var bodies = GetInternalFunctionBodies();
                foreach (var function in functions)
                {
                    if (bodies.ContainsKey(function.Name) && bodies[function.Name] != null)
                        function.GeneratedCode = bodies[function.Name];
                    else
                        function.GeneratedCode = GetDefaultBody();
                }
                temp = ContextForInternalFunctionsTemplate.GetInstanceOf("gen_context");
                temp.Add("arg", new InternalFunctionTemplateParams()
                {
                    Functions = functions.ToList(),
                    Name = Catalog.Name
                });
                ContextForInternalFunctionsTemplate.RegisterRenderer(typeof(string), new CodeRenderer(), true);
                r = temp.Render();
                Write(OutputPathContext, Catalog.Name.ToPascal() + GetContextFilename(ContextFile.ServiceSupport), r);
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
