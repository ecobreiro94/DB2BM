using DB2BM.Abstractions.AST;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.Utils
{
    public class Scope : ICloneable
    {
        public Dictionary<ASTNode, string> Variables { get; set; }

        /// <summary>
        /// Key: Name,
        /// Value: Type
        /// </summary>
        public Dictionary<string, string> VariablesDeclaration { get; set; }

        Scope(Dictionary<ASTNode, string> v, Dictionary<string, string> vd)
        {
            Variables = new Dictionary<ASTNode, string>();
            foreach (var item in v)
            {
                Variables.Add(item.Key, item.Value);
            }
            VariablesDeclaration = new Dictionary<string, string>();
            foreach (var item in vd)
            {
                VariablesDeclaration.Add(item.Key, item.Value);
            }
        }

        public Scope()
        {
            Variables = new Dictionary<ASTNode, string>();
            VariablesDeclaration = new Dictionary<string, string>();
        }

        public object Clone()
        {
            return new Scope(Variables, VariablesDeclaration);
        }

        public bool AddVariable(string realName,ASTNode node)
        {
            if (Variables.ContainsKey(node))
                return false;
            Variables.Add(node, realName);
            if (VariablesDeclaration.ContainsKey(realName) && node is IdNode)
                (node as IdNode).TypeReturn = VariablesDeclaration[realName];
            return true;
        }
        public bool AddDeclaration(string name, string type)
        {
            if(VariablesDeclaration.ContainsKey(name))
                return false;
            VariablesDeclaration.Add(name, type);
            return true;
        }
        public bool Contains(string id)
        {
            return VariablesDeclaration.ContainsKey(id);
        }
    }
}
