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
            var generatorType = typeof(IGenerator);
            var sintacticAnaliceType = typeof(ISyntacticAnalyzer);

            var files = Directory.GetFiles( Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.dll");

            var catalogHandlerTypes = new List<Type>();
            var generatorTypes = new List<Type>();
            var syntacticAnalizerTypes = new List<Type>(); 

            foreach (var f in files)
            {
                var assembly = Assembly.LoadFrom(f);

                catalogHandlerTypes.AddRange(assembly.GetTypes().Where(
                    t => t.GetInterfaces().Contains(catalogHandlerType) &&
                    (t.GetCustomAttribute<DbmsAttr>()?.Name == dbms)));

                generatorTypes.AddRange(assembly.GetTypes().Where(
                    t => t.GetInterfaces().Contains(generatorType) &&
                    (t.GetCustomAttribute<OrmAttr>()?.Name == orm)));

                syntacticAnalizerTypes.AddRange(assembly.GetTypes().Where(
                    t => t.GetInterfaces().Contains(sintacticAnaliceType) &&
                    (t.GetCustomAttribute<DbmsAttr>()?.Name == dbms)));
            }

            var msg = "";
            if (catalogHandlerTypes.Count > 1 || syntacticAnalizerTypes.Count > 1)
                msg += "Existe mas de una Extensión para " + dbms + "." ;
            else if(catalogHandlerTypes.Count == 0 || syntacticAnalizerTypes.Count == 0)
                msg += "No existe una Extensión para " + dbms + "." ;
            if (generatorTypes.Count > 1)
                msg += "Existe mas de una Extensión para " + orm + "." ;
            else if (generatorTypes.Count == 0)
                msg += "No existe una Extensión para " + orm + "." ;

            if (msg != "") throw new Exception(msg);

            serviceCollection = serviceCollection.AddSingleton(catalogHandlerType, catalogHandlerTypes.Single());
            serviceCollection = serviceCollection.AddSingleton(generatorType, generatorTypes.Single());
            serviceCollection = serviceCollection.AddSingleton(sintacticAnaliceType, syntacticAnalizerTypes.Single());

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
                        foreach (var function in catalog.StoreProcedures.Values)
                        {
                            Console.WriteLine(function);
                        }
                    }

                    if (o.Generate.Any() && o.OutputPath != null)
                    {
                        var generator = serviceProvider.GetService<IGenerator>();
                        generator.Catalog = catalog;
                        generator.OutputPath = o.OutputPath;

                        var syntacticAnalyzer = serviceProvider.GetService<ISyntacticAnalyzer>();

                        var serviceName = $"{catalog.Name.ToPascal()}Service";

                        
                        if (o.Generate.Contains(GenerationItem.All))
                        {
                            syntacticAnalyzer.Parser(generator.Catalog);
                            generator.GenerateEntities();
                            generator.GenerateDbContext();
                            var parameters = string.IsNullOrEmpty(o.StoredProcedures) ?
                                null :
                                JsonConvert.DeserializeObject<FunctionsGenerationOptions>(File.ReadAllText(o.StoredProcedures));
                            if (parameters?.FunctionsNames == null || parameters.FunctionsNames.Count == 0)
                                generator.GenerateAllFunctions((parameters.ClassName == null) ? serviceName : parameters.ClassName);
                            else
                                generator.GenerateFunctions((parameters.ClassName == null) ? serviceName : parameters.ClassName, parameters.FunctionsNames);
                        }
                        else
                        {
                            if (o.Generate.Contains(GenerationItem.Entities))
                            {
                                generator.GenerateEntities();
                            }
                            if (o.Generate.Contains(GenerationItem.DbCtx))
                            {
                                generator.GenerateDbContext();
                            }
                            if (o.Generate.Contains(GenerationItem.SPs))
                            {
                                syntacticAnalyzer.Parser(generator.Catalog);
                                var parameters = string.IsNullOrEmpty(o.StoredProcedures) ? 
                                    null :
                                    JsonConvert.DeserializeObject<FunctionsGenerationOptions>(File.ReadAllText(o.StoredProcedures));
                                if (parameters?.FunctionsNames == null || parameters.FunctionsNames.Count == 0)
                                    generator.GenerateAllFunctions((parameters.ClassName == null) ? serviceName : parameters.ClassName);
                                else
                                    generator.GenerateFunctions((parameters.ClassName == null) ? serviceName : parameters.ClassName, parameters.FunctionsNames);
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
