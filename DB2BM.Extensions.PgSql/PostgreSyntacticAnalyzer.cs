using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using DB2BM.Abstractions.AST.Statements;
using DB2BM.Abstractions.Attrs;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Extensions.PgSql.Parser;

namespace DB2BM.Extensions.PgSql
{
    [Dbms("postgre")]
    public class PostgreSyntacticAnalyzer : ISyntacticAnalyzer
    {        
        public void Parse(StoredProcedure sp)
        {
            var input = new AntlrInputStream(sp.OriginalCode);
            var lexer = new PlPgSqlLexer(input);       //Lexer generado por ANTLR
            var tokens = new CommonTokenStream(lexer);
            var parser = new PlPgSqlParser(tokens);    //Parser generado por ANTLR
            var tree = parser.function_body();         //Árbol de Sintáxis creado por el Parser
            
            var astBuilder = new ASTGenerator();       //Generador del AST usando el Patrón Visitor

            sp.AST = parser.NumberOfSyntaxErrors == 0 ? 
                astBuilder.Visit(tree) :               //Generación del AST Genérico
                new FunctionBlockNode(0, 0)            //AST de un SP que dispara una excepción
                {
                    Statements = new List<StatementNode>()
                    {
                        new RaiseMessageStatementNode(0, 0)
                        {
                            LogLevel = "EXCEPTION", Message = "Not implemented"
                        }
                    }
                };
        }
    }
}
