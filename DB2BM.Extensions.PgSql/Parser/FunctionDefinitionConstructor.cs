using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DB2BM.Abstractions.AST;

namespace DB2BM.Extensions.PgSql.Parser
{
    public class FunctionDefinitionConstructor 
    {
        IParseTree tree;
        ASTGenerator astBuilder;
        bool parserErrors;
        public FunctionDefinitionConstructor(string functionDefinition)
        {
            var definition = functionDefinition;
           
            var input = new AntlrInputStream(definition);
            var lexer = new PlPgSqlLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new PlPgSqlParser(tokens);
            tree = parser.function_body();
            if (parser.NumberOfSyntaxErrors > 0)
                parserErrors = true;
            astBuilder = new ASTGenerator();
        }

        public ASTNode GetAST()
        {
            if (!parserErrors)
                return astBuilder.Visit(tree);
            else
            {
                var result = new Abstractions.AST.Statements.FunctionBlockNode(0, 0) {
                    Statements = new List<Abstractions.AST.Statements.StatementNode>()
                };
                result.Statements.Add(new Abstractions.AST.Statements.RaiseMessageStatementNode(0, 0) { LogLevel = "EXCEPTION", Message = "Not supperted yet" });
                return result;
            }
        }
    }
}
