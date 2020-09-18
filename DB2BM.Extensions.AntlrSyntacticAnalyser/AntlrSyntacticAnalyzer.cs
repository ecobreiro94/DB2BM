using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DB2BM.Abstractions.AST;
using DB2BM.Abstractions.AST.Statements;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.AntlrSyntacticAnalyser
{
    public abstract class AntlrSyntacticAnalyzer<TLexer, TParser, TASTBuilder> : ISyntacticAnalyzer
        where TLexer : Antlr4.Runtime.Lexer
        where TParser : Antlr4.Runtime.Parser
        where TASTBuilder : IParseTreeVisitor<ASTNode>, new()
    {
        public void Parse(StoredProcedure sp)
        {
            var input = new AntlrInputStream(sp.OriginalCode);

            //new TLexer(input)
            var lexerType = typeof(TLexer);
            var lexer = (TLexer)lexerType.GetConstructor(new[] { typeof(ICharStream) }).Invoke(new[] { input });

            var tokens = new CommonTokenStream(lexer);

            //new TParser(tokens)
            var parserType = typeof(TParser);
            var parser = (TParser)parserType.GetConstructor(new[] { typeof(ITokenStream) }).Invoke(new[] { tokens });

            var tree = GetParseTree(parser);

            var astBuilder = new TASTBuilder();
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

        protected abstract IParseTree GetParseTree(TParser parser);
    }

}
