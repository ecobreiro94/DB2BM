using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DB2BM.Abstractions.AST;
using DB2BM.Abstractions.AST.Statements;
using DB2BM.Abstractions.Attrs;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Extensions.AntlrSyntacticAnalyser;
using DB2BM.Extensions.PgSql.Parser;

namespace DB2BM.Extensions.PgSql
{
    [Dbms("postgre")]
    public class PostgreSyntacticAnalyzer : AntlrSyntacticAnalyzer<PlPgSqlLexer, PlPgSqlParser, ASTBuilder>
    {
        protected override IParseTree GetParseTree(PlPgSqlParser parser)
        {
            return parser.function_body();
        }
    }
}
