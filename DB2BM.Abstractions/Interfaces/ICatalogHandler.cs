using System.Collections.Generic;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Entities.UserDefined;

namespace DB2BM.Abstractions.Interfaces
{
    public interface ICatalogHandler
    {
        DbOption Options { get; set; }
 
        DatabaseCatalog GetCatalog();
    }
}
