using System;
using DB2BM.Extensions.PgSql;
using CommandLine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using DB2BM.Abstractions;
using DB2BM.Abstractions.Entities;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Attrs;
using DB2BM.Utils;

namespace DB2BM
{
    class Program
    {
        static IServiceProvider ConfigureServiceProvider(string orm, string dbms)
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            var catalogHandlerType = typeof(ICatalogHandler);
            var generatorType = typeof(IBMGenerator);
            var syntacticAnalyzerType = typeof(ISyntacticAnalyzer);
            var semanticAnalyzerType = typeof(ISemanticAnalyzer);
            var files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.dll");

            var catalogHandlerTypes = new List<Type>();
            var generatorTypes = new List<Type>();
            var syntacticAnalyzerTypes = new List<Type>();
            var semanticAnalyzerTypes = new List<Type>();
            foreach (var f in files)
            {
                var assembly = Assembly.LoadFrom(f);

                catalogHandlerTypes.AddRange(assembly.GetTypes().Where(
                    t => t.GetInterfaces().Contains(catalogHandlerType) &&
                    (t.GetCustomAttribute<DbmsAttribute>()?.Name == dbms)));

                generatorTypes.AddRange(assembly.GetTypes().Where(
                    t => t.GetInterfaces().Contains(generatorType) &&
                    (t.GetCustomAttribute<OrmAttribute>()?.Name == orm)));

                syntacticAnalyzerTypes.AddRange(assembly.GetTypes().Where(
                    t => t.GetInterfaces().Contains(syntacticAnalyzerType) &&
                    (t.GetCustomAttribute<DbmsAttribute>()?.Name == dbms)));

                semanticAnalyzerTypes.AddRange(assembly.GetTypes().Where(
                    t => t.GetInterfaces().Contains(semanticAnalyzerType)));
            }

            var msg = "";
            if (catalogHandlerTypes.Count > 1 || syntacticAnalyzerTypes.Count > 1)
                msg += "Existe mas de una Extensión para " + dbms + ".";
            else if (catalogHandlerTypes.Count == 0 || syntacticAnalyzerTypes.Count == 0)
                msg += "No existe una Extensión para " + dbms + ".";
            if (generatorTypes.Count > 1)
                msg += "Existe mas de una Extensión para " + orm + ".";
            else if (generatorTypes.Count == 0)
                msg += "No existe una Extensión para " + orm + ".";
            else if (semanticAnalyzerTypes.Count > 1)
                msg += "Existe mas de una Extension para el Análisis Semántico.";
            else if (semanticAnalyzerTypes.Count == 0)
                msg += "No existe una Extensión para el Análisis Semántico.";
            if (msg != "") throw new Exception(msg);

            serviceCollection = serviceCollection.AddSingleton(catalogHandlerType, catalogHandlerTypes.Single());
            serviceCollection = serviceCollection.AddSingleton(generatorType, generatorTypes.Single());
            serviceCollection = serviceCollection.AddSingleton(syntacticAnalyzerType, syntacticAnalyzerTypes.Single());
            serviceCollection = serviceCollection.AddSingleton(semanticAnalyzerType, semanticAnalyzerTypes.Single());
            return serviceCollection.BuildServiceProvider();
        }

        static void Main(string[] args)
        {
            var cmdLineParser = new Parser(s =>
            {
                s.AutoHelp = false;
                s.AutoVersion = true;
                s.CaseInsensitiveEnumValues = true;
                s.HelpWriter = Parser.Default.Settings.HelpWriter;
            });
            cmdLineParser.ParseArguments<CommandLineOptions>(args).WithParsed<CommandLineOptions>(o =>
            {
                var databaseOptions = new DbOption()
                {
                    DataBaseName = o.DataBase,
                    Host = o.Host,
                    Password = o.Password,
                    User = o.User
                };
                IServiceProvider serviceProvider;
                try
                {
                    serviceProvider = ConfigureServiceProvider(o.Orm, o.Dbms);
                    var catalogHandler = serviceProvider.GetService<ICatalogHandler>();

                    catalogHandler.Options = databaseOptions;

                    DatabaseCatalog catalog;
                    try
                    {
                        catalog = catalogHandler.GetCatalog();
                    }
                    catch (Exception e)
                    {
                        throw new Exception("No se puede establecer conexión con la Base de Datos.");
                    }

                    if (o.ListCatalog.Any(i => i == CatalogItem.Tables))
                    {
                        Console.WriteLine("Tablas y vistas");
                        foreach (var table in catalog.Tables.Values)
                        {
                            Console.WriteLine(table);
                        }
                    }

                if (o.ListCatalog.Any(i => i == CatalogItem.SPs))
                {
                    foreach (var function in catalog.StoredProcedures.Values)
                    {
                        Console.WriteLine(function);
                    }
                }

                    if (o.Generate.Any() && o.OutputPath != null)
                    {
                        var syntacticAnalyzer = serviceProvider.GetService<ISyntacticAnalyzer>();

                        var semanticAnalyzer = serviceProvider.GetService<ISemanticAnalyzer>();
                        semanticAnalyzer.Catalog = catalog;

                        var generator = serviceProvider.GetService<IBMGenerator>();
                        generator.Catalog = catalog;
                        generator.SyntacticAnalyzer = syntacticAnalyzer;
                        generator.SemanticAnalyzer = semanticAnalyzer;
                        generator.SetOutputPath(o.OutputPath, o.Project);

                        var serviceName = $"{catalog.Name.ToPascal()}Service";

                        if (o.Generate.Contains(GenerationItem.All))
                        {
                            generator.GenerateEntities();
                            generator.GenerateContext();
                            var parameters = string.IsNullOrEmpty(o.StoredProcedures) ?
                                null :
                                JsonConvert.DeserializeObject<FunctionsGenerationOptions>(File.ReadAllText(o.StoredProcedures));
                            if (parameters?.FunctionsNames == null || parameters.FunctionsNames.Count == 0)
                                generator.GenerateService((parameters?.ClassName != null) ? parameters.ClassName : serviceName);
                            else
                                generator.GenerateService((parameters?.ClassName != null) ? parameters.ClassName : serviceName,
                                    parameters.FunctionsNames);
                        }
                        else
                        {
                            if (o.Generate.Contains(GenerationItem.Entities))
                            {
                                generator.GenerateEntities();
                            }
                            if (o.Generate.Contains(GenerationItem.DbCtx))
                            {
                                generator.GenerateContext();
                            }
                            if (o.Generate.Contains(GenerationItem.SPs))
                            {
                                var parameters = string.IsNullOrEmpty(o.StoredProcedures) ?
                                    null :
                                    JsonConvert.DeserializeObject<FunctionsGenerationOptions>(File.ReadAllText(o.StoredProcedures));
                                if (parameters?.FunctionsNames == null || parameters.FunctionsNames.Count == 0)
                                    generator.GenerateService((parameters != null) ? parameters.ClassName : serviceName);
                                else
                                    generator.GenerateService((parameters != null) ? parameters.ClassName : serviceName,
                                        parameters.FunctionsNames);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            });
        }


    }

}
