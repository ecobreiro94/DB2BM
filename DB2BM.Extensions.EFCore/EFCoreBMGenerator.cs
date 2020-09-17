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
using DB2BM.Extensions.BusinessGenerator.Utils;
using DB2BM.Abstractions;
using DB2BM.Extensions.EFCore.Visitors;
using System.Reflection;
using DB2BM.Abstractions.Visitors;
using DB2BM.Extensions.BusinessGenerator;

namespace DB2BM.Extensions
{
    [Orm("efcore")]
    public class EFCoreBMGenerator : BusinessModelGenerator<EFCoreCodeGenVisitor>
    {
        protected override Dictionary<string, string> TypesMapper => SemanticAnalyzer.TypesMapper;

        protected override void PrepareCatalog(DatabaseCatalog catalog)
        {
            catalog.PrepareCatalog(TypesMapper);
        }

        protected override string GetContextFilename(ContextFile contextFile)
        {
            switch (contextFile)
            {
                case ContextFile.Entities:
                    return $"{Catalog.Name.ToPascal()}DbContext.cs";

                case ContextFile.InternalFunctionsSupport:
                    return $"{Catalog.Name.ToPascal()}DbContext.Extensions.cs";

                case ContextFile.ServiceSupport:
                    return $"{Catalog.Name.ToPascal()}DbContext.Service.cs";

                default:
                    throw new NotSupportedException("Unsupported context");
            };
        }

        protected override string GetEntityFilename(Table item) => item.Name.ToPascal() + ".cs";

        protected override string GetUdtFilename(BaseUserDefinedType item) => item.TypeName.ToPascal() + ".cs";

        protected override string GetServiceFilename(string className) => className + ".cs";

        protected override string GetTemplateFilename(TemplateFile templateFile)
        {
            switch (templateFile)
            {
                case TemplateFile.Entity:
                    return "model.st4";

                case TemplateFile.ContextForEntities:
                    return "dbcontext.st4";

                case TemplateFile.ContextForServiceSupport:
                    return "additional_template.st4";

                case TemplateFile.ContextForInternalFunctionsSupport:
                    return "partial_dbcontext.st4";

                case TemplateFile.Service:
                    return "functions.st4";

                default:
                    throw new NotSupportedException("Unsupported template");
            };
        }

        protected override void InsertErrors(IEnumerable<SemanticResult> errors, StoredProcedure Sp)
        {
            foreach (var error in errors)
                Sp.GeneratedCode += "//" + (error as ErrorResult).Menssage + "\n";
            Sp.GeneratedCode += "throw new NotImplementedException();";
        }

        protected override Dictionary<string, string> GetInternalFunctionBodies()
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>
                                (File.ReadAllText("internal_functions_body.json"));
        }

        protected override string GetDefaultBody() => "throw new NotImplementedException();";

    }


}
