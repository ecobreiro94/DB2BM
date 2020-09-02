using DB2BM.Abstractions.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DB2BM.Abstractions.Entities
{
    public class StoreProcedure
    {
        public string Name { get; set; }

        public string SpecificName { get; set; }

        public List<Parameter> Params { get; set; }

        public string ReturnType { get; set; }

        public string ReturnClause { get; set; }

        public bool IsInternal { get; set; }
        
        public string LanguageDefinition { get; set; }

        public ASTNode Definition { get; set; }

        public string PLDefinition { get; set; }

        public string BMDefinition { get; set; }

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
