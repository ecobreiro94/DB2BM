using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM
{
    public enum CatalogItem { Tables, SPs }
    public enum GenerationItem { All, Entities, DbCtx, SPs }

class CommandLineOptions
{
    [Option('h', "host", Required = true, HelpText = "Nombre del servidor de base de datos")]
    public string Host { get; set; }

    [Option('d', "db", Required = true, HelpText = "Nombre de la base de datos")]
    public string DataBase { get; set; }

    [Option('u', "user", Required = true, HelpText = "Nombre del usuario para acceder a la base de datos")]
    public string User { get; set; }

    [Option('p', "pass", Required = true, HelpText = "Password del usuario para acceder a la base de datos")]
    public string Password { get; set; }

    [Option('l', "list", Min = 1, Max = 2, HelpText = "Listar catálogo (tables, sps: StoredProcedures")]
    public IEnumerable<CatalogItem> ListCatalog { get; set; }

    [Option('g', "gen", Min = 1, Max = 3, HelpText = "Generar código (all, entities, dbctx: DB Context, sps: Stored Procedures")]
    public IEnumerable<GenerationItem> Generate { get; set; }

    [Option("splist", HelpText = "Archivo JSON con la lista de SPs a generar")]
    public string StoredProcedures { get; set; }

    [Option("outpath", HelpText = "Directorio de salida")]
    public string OutputPath { get; set; }

    [Option('o', "orm", Required = true, HelpText = "ORM a implementar")]
    public string Orm { get; set; }

    [Option('m', "dbms", Required = true, HelpText = "Gestor de Base de datos")]
    public string Dbms { get; set; }        
}
}
