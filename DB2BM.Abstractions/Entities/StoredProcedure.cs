using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DB2BM.Abstractions.AST;

namespace DB2BM.Abstractions.Entities
{
    public class StoredProcedure
    {
        public string Name { get; set; }

        public string SpecificName { get; set; }

        public List<Parameter> Params { get; set; }

        public string ReturnType { get; set; }

        public bool ReturnIsSet { get; set; }

        public bool IsInternal { get; set; }
        
        public string LanguageDefinition { get; set; }

        public ASTNode AST { get; set; }

        public string OriginalCode { get; set; }

        public string GeneratedCode { get; set; }

        public bool ExistsParams =>  Params.Count != 0;

        public List<Parameter> INParams { get => Params.Where(p => !p.OutMode).ToList(); }

        public override string ToString()
        {
            var consoleString = ReturnType + " " + Name + "( ";
            foreach (var p in Params)
            {
                consoleString += p.ToString() + ", ";
            }
            consoleString += ")";
            return consoleString;
        }

    }
}
