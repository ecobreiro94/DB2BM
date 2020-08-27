using System.Collections.Generic;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Entities.UserDefined;

namespace DB2BM.Abstractions.Interfaces
{
    public interface ICatalogHandler
    {
        DbOption Options { get; set; }
        //Dictionary<string, string> TypeMapper { get; set; }

        //// Retorna todas las tablas de la base de datos
        //IEnumerable<Table> GetTables();

        ////Retornas todas las funciones definidas por el usuario en la base de datos
        //IEnumerable<Function> GetFunctions();

        ////Retorna todas las funciones internas de la base de datos
        //IEnumerable<Function> GetInternalFunctions();

        ////Retorna todas las relaciones existentes entre las tablas de la base de datos
        //IEnumerable<Relation> GetRelations();

        ////Returna todas los udf definidos en la base de datos
        //IEnumerable<BaseUserDefined> GetUserDefineds();

        //Retorna la entidad base de datos como tal 
        DatabaseCatalog GetCatalog();
    }
}
