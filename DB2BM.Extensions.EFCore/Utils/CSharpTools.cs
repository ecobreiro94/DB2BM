using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DB2BM.Extensions.Utils
{
    public static class CSharpTools
    {
        public static string GetCSharpType(string type, Dictionary<string, string> typesMapper)
        {
            if (typesMapper.ContainsKey(type))
                return typesMapper[type];

            else if (type.Length > 0 && type[0] == '_')
            {
                var simpleType = new string(type.Skip(1).ToArray());
                if (typesMapper.ContainsKey(simpleType))
                    return typesMapper[simpleType] + "[]";
            }

            return type;
        }
    }
}
