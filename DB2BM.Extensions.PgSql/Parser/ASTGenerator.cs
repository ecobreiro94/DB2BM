using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Misc;
using DB2BM.Abstractions.AST;
using DB2BM.Abstractions.AST.AfterOps;
using DB2BM.Abstractions.AST.Declarations;
using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.AST.Expressions.Atomics;
using DB2BM.Abstractions.AST.Expressions.FunctionCalls;
using DB2BM.Abstractions.AST.Expressions.Operators;
using DB2BM.Abstractions.AST.Expressions.Operators.Binarys;
using DB2BM.Abstractions.AST.Expressions.Operators.Binarys.Arithmetics;
using DB2BM.Abstractions.AST.Expressions.Operators.Binarys.Comparison;
using DB2BM.Abstractions.AST.Expressions.Operators.Binarys.Logicals;
using DB2BM.Abstractions.AST.Expressions.Operators.Ternarys;
using DB2BM.Abstractions.AST.Expressions.Operators.Unarys;
using DB2BM.Abstractions.AST.Expressions.Operators.Unarys.Comparison;
using DB2BM.Abstractions.AST.Expressions.Operators.Unarys.Logicals;
using DB2BM.Abstractions.AST.Statements;
using DB2BM.Abstractions.AST.Statements.Additional;
using DB2BM.Abstractions.AST.Statements.Base;
using DB2BM.Abstractions.AST.Statements.Control;
using DB2BM.Abstractions.AST.Statements.Cursor;
using DB2BM.Abstractions.AST.Statements.Data;
using DB2BM.Abstractions.AST.Types;

namespace DB2BM.Extensions.PgSql.Parser
{
    public class ASTGenerator : PlPgSqlParserBaseVisitor<ASTNode>
    {
        //ok
        public override ASTNode VisitAdditional_statement([NotNull] PlPgSqlParser.Additional_statementContext context)
        {
            AdditionalStatementNode result = null;
            if (context.anonymous_block() != null)
                return Visit(context.anonymous_block());
            else if (context.explain_statement() != null)
                return Visit(context.explain_statement());
            else if (context.copy_statement() != null)
                return Visit(context.copy_statement());
            else if (context.show_statement() != null)
                return Visit(context.show_statement());
            else if (context.LISTEN() != null)
                return new AddListenNode(context.Start.Line, context.Start.Column)
                {
                    Identifier = Visit(context.identifier(0)) as IdNode
                };
            else if (context.UNLISTEN() != null)
                return new AddUnlistenNode(context.Start.Line, context.Start.Column)
                {
                    Identifier = Visit(context.identifier(0)) as IdNode
                };
            else if (context.ANALYZE() != null)
            {
                result = new AddAnalizeNode(context.ANALYZE().Symbol.Line, context.ANALYZE().Symbol.Column)
                {
                    Modes = new List<AnalizeModeNode>()
                };
                foreach (var m in context.analyze_mode())
                    (result as AddAnalizeNode).Modes.Add(Visit(m) as AnalizeModeNode);
                if (context.table_cols_list() != null)
                {
                    var _result = (result as AddAnalizeNode);
                    _result.TableColsList = new List<TableColsNode>();
                    foreach (var tc in context.table_cols_list().table_cols())
                    {
                        _result.TableColsList.Add(Visit(tc) as TableColsNode);
                    }
                    result = _result;
                }
            }
            else if (context.CLUSTER() != null)
            {
                result = new AddClusterNode(context.CLUSTER().Symbol.Line, context.CLUSTER().Symbol.Column);
                if (context.ON() != null) (result as AddClusterNode).On = true;
                if (context.schema_qualified_name() != null)
                    (result as AddClusterNode).SchemaQualifield =
                        Visit(context.schema_qualified_name()) as SchemaQualifieldNode;
                if (context.identifier() != null)
                    (result as AddClusterNode).Identifier =
                        Visit(context.identifier(0)) as IdNode;
            }
            else if (context.DEALLOCATE() != null)
                return new AddDeallocatteNode(context.DEALLOCATE().Symbol.Line, context.DEALLOCATE().Symbol.Column)
                {
                    Identifier = (context.identifier() != null)? Visit(context.identifier(0)) as IdNode : null
                };
            else if (context.REINDEX() != null)
                return new AddReindexNode(context.REINDEX().Symbol.Line, context.REINDEX().Symbol.Column)
                {
                    SchemaQualifield = Visit(context.schema_qualified_name()) as SchemaQualifieldNode
                };
            else if (context.RESET() != null)
            {
                result = new AddResetNode(context.RESET().Symbol.Line, context.RESET().Symbol.Column)
                {
                    Identifiers = new List<IdNode>()
                };
                foreach (var id in context.identifier())
                    (result as AddResetNode).Identifiers.Add(Visit(id) as IdNode);
            }
            else if (context.REFRESH() != null)
            {
                return new AddRefreshNode(context.REFRESH().Symbol.Line, context.REFRESH().Symbol.Column)
                {
                    SchemaQualifield = Visit(context.schema_qualified_name()) as SchemaQualifieldNode
                };
            }
            else if (context.PREPARE() != null)
            {
                result = new AddPrepareNode(context.PREPARE().Symbol.Line, context.PREPARE().Symbol.Column)
                {
                    Identifier = Visit(context.identifier(0)) as IdNode,
                    Types = new List<DataTypeNode>(),
                    DataStatement = Visit(context.data_statement()) as DataStatementNode
                };
                foreach (var t in context.data_type())
                    (result as AddPrepareNode).Types.Add(Visit(t) as DataTypeNode);
            }
            else if (context.REASSIGN() != null)
            {
                result = new AddReassignNode(context.REASSIGN().Symbol.Line, context.REASSIGN().Symbol.Column)
                {
                    UserNames = new List<UserNameNode>()
                };
                foreach (var un in context.user_name())
                    (result as AddReassignNode).UserNames.Add(
                        new UserNameNode(un.Start.Line, un.Start.Column)
                        {
                            Identifier = Visit(un.identifier()) as IdNode
                        });
            }
            return result;
        }

        //ok
        public override ASTNode VisitAfter_ops([NotNull] PlPgSqlParser.After_opsContext context)
        {
            if (context.orderby_clause() != null)
                return Visit(context.orderby_clause());
            else if (context.LIMIT() != null)
            {
                if (context.vex() != null)
                    return new AfterOpsLimitNode(context.Start.Line, context.Start.Column)
                    {
                        Expression = Visit(context.vex()) as ExpressionNode
                    };
                else return new AfterOpsLimitNode(context.Start.Line, context.Start.Column) { All = true };
            }
            else if (context.OFFSET() != null)
            {
                return new AfterOpsOffsetNode(context.Start.Line, context.Start.Column)
                {
                    Expression = Visit(context.vex()) as ExpressionNode
                };
            }
            else if (context.FETCH() != null)
            {
                return new AfterOpsFetchNode(context.Start.Line, context.Start.Column)
                {
                    Expression = (context.vex() != null) ? Visit(context.vex()) as ExpressionNode : null,
                    First = context.FIRST() != null,
                    Row = context.ROW() != null
                };
            }
            else if (context.FOR() != null)
            {
                return new AfterOpsForNode(context.Start.Line, context.Start.Column)
                {
                    Update = context.UPDATE() != null && context.NO() == null,
                    NoKeyUpdate = context.NO() != null && context.UPDATE() != null, 
                    Share = context.SHARE() != null && context.KEY() == null,
                    KeyShare = context.SHARE() != null && context.KEY() != null
                };
            }
            return null;
        }
        
        //ok
        public override ASTNode VisitAlias_clause([NotNull] PlPgSqlParser.Alias_clauseContext context)
        {
            var result = new AliasClauseNode(context.Start.Line, context.Start.Column)
            {
                Alias = Visit(context.alias) as IdNode,
                ColumnsAlias = new List<IdNode>()
            };
            foreach (var ca in context._column_alias)
            {
                result.ColumnsAlias.Add(Visit(ca) as IdNode);
            }
            return result;
        }

        //ok
        public override ASTNode VisitAll_op_ref([NotNull] PlPgSqlParser.All_op_refContext context)
        {
            return new AllOpRefNode(context.Start.Line, context.Start.Column)
            {
                AllSimpleOp = Visit(context.all_simple_op()) as AllSimpleOpNode,
                Identifier = (context.identifier() != null)? Visit(context.identifier()) as IdNode: null
            };
        }

        //ok
        public override ASTNode VisitAll_simple_op([NotNull] PlPgSqlParser.All_simple_opContext context)
        {
            var result = new AllSimpleOpNode(context.Start.Line, context.Start.Column);

            if (context.op_chars() != null)
                return Visit(context.op_chars());
            else if (context.EQUAL() != null) result.Symbol = context.EQUAL().GetText();
            else if (context.NOT_EQUAL() != null) result.Symbol = context.NOT_EQUAL().GetText();
            else if (context.LTH() != null) result.Symbol = context.LTH().GetText();
            else if (context.LEQ() != null) result.Symbol = context.LEQ().GetText();
            else if (context.GTH() != null) result.Symbol = context.GTH().GetText();
            else if (context.GEQ() != null) result.Symbol = context.GEQ().GetText();
            else if (context.PLUS() != null) result.Symbol = context.PLUS().GetText();
            else if (context.MINUS() != null) result.Symbol = context.MINUS().GetText();
            else if (context.MULTIPLY() != null) result.Symbol = context.MULTIPLY().GetText();
            else if (context.DIVIDE() != null) result.Symbol = context.DIVIDE().GetText();
            else if (context.MODULAR() != null) result.Symbol = context.MODULAR().GetText();
            else result.Symbol = context.EXP().GetText();
            return result;
        }

        //ok
        public override ASTNode VisitAnalyze_mode([NotNull] PlPgSqlParser.Analyze_modeContext context)
        {
            return new AnalizeModeNode(context.Start.Line, context.Start.Column)
            {
                Verbose = context.VERBOSE() != null,
                Value = (context.boolean_value() != null)? Visit(context.boolean_value()) as ExpressionNode : null
            };
        }

        //ok
        public override ASTNode VisitAnonymous_block([NotNull] PlPgSqlParser.Anonymous_blockContext context)
        {
            return base.Visit(context);
        }
        
        //ok
        public override ASTNode VisitArguments_list([NotNull] PlPgSqlParser.Arguments_listContext context)
        {
            return base.Visit(context);
        }
        
        //ok
        public override ASTNode VisitArray_elements([NotNull] PlPgSqlParser.Array_elementsContext context)
        {
            var arrayVex = new List<(int, ExpressionNode)>();
            var arrayElements = new List<(int, ArrayElementsNode)>();
            if (context.vex() != null && context.vex().Length > 0)
            {
                foreach (var exp in context.vex())
                {
                    arrayVex.Add((exp.Start.Line + exp.Start.Column, Visit(exp) as ExpressionNode));
                }
            }
            if (context.array_elements() != null && context.array_elements().Length > 0)
            {
                foreach (var ae in context.array_elements())
                {
                    arrayElements.Add((ae.Start.Line + ae.Start.Column, Visit(ae) as ArrayElementsNode));
                }
            }
            var arrayResult = new List<ExpressionNode>();
            int j = 0;
            int k = 0;
            for (int i = 0; i < arrayElements.Count + arrayVex.Count; i++)
            {
                if (j < arrayVex.Count)
                {
                    if (k < arrayElements.Count)
                    {
                        if (arrayVex[j].Item1 < arrayElements[k].Item1)
                        {
                            arrayResult.Add(arrayVex[j].Item2);
                            j++;
                        }
                        else
                        {
                            arrayResult.Add(arrayElements[k].Item2);
                            k++;
                        }
                    }
                    else
                    {
                        arrayResult.Add(arrayVex[j].Item2);
                        j++;
                    }
                }
                else
                {
                    if (k < arrayElements.Count)
                    {
                        arrayResult.Add(arrayElements[k].Item2);
                        k++;
                    }
                }
            }
            return new ArrayElementsNode(context.Start.Line, context.Start.Column) { Elements = arrayResult};
        }
        
        //ok
        public override ASTNode VisitArray_expression([NotNull] PlPgSqlParser.Array_expressionContext context)
        {
            if (context.array_elements() != null)
                return Visit(context.array_elements());
            else return new ArrayToSelectNode(context.Start.Line, context.Start.Column)
            {SelectStmt = Visit(context.table_subquery().select_stmt()) as SelectStatementNode };
        }
       
        //ok
        public override ASTNode VisitArray_type([NotNull] PlPgSqlParser.Array_typeContext context)
        {
            return base.Visit(context);
        }

        //ok
        public override ASTNode VisitAssign_stmt([NotNull] PlPgSqlParser.Assign_stmtContext context)
        {
            return new AssingStmtNode(context.Start.Line, context.Start.Column)
            {
                Var = Visit(context.var()) as VarNode,
                Symbol = (context.COLON_EQUAL() != null)? context.COLON_EQUAL().Symbol.Text : context.EQUAL().Symbol.Text,
                Stmt = (context.perform_stmt() != null)? Visit(context.perform_stmt()) as StatementNode : 
                    Visit(context.select_stmt_no_parens()) as StatementNode
            };
        }

        //ok
        public override ASTNode VisitBase_statement([NotNull] PlPgSqlParser.Base_statementContext context)
        {
            BaseStatementNode result = null;
            if (context.assign_stmt() != null)
            {
                return Visit(context.assign_stmt());
            }
            else if (context.EXECUTE() != null)
            {
                result = new ExecuteStmtNode(context.Start.Line, context.Start.Column)
                {
                    Expression = Visit(context.vex()) as ExpressionNode,
                    UsingExpression = new List<ExpressionNode>()
                };
                foreach (var expr in context.using_vex().vex())
                {
                    (result as ExecuteStmtNode).UsingExpression.Add(Visit(expr) as ExpressionNode);
                }
            }
            else if (context.perform_stmt() != null)
                return Visit(context.perform_stmt());
            
            return result;
        }

        //ok
        public override ASTNode VisitBoolean_value([NotNull] PlPgSqlParser.Boolean_valueContext context)
        {
            if (context.TRUE() != null)
                return new BoolNode(context.Start.Line, context.Start.Column, true);
            else if (context.FALSE() != null)
                return new BoolNode(context.Start.Line, context.Start.Column, false);
            else return new IntNode(context.Start.Line, context.Start.Column, int.Parse(context.NUMBER_LITERAL().GetText()));
        }
        
        //ok
        public override ASTNode VisitCascade_restrict([NotNull] PlPgSqlParser.Cascade_restrictContext context)
        {
            return base.VisitCascade_restrict(context);
        }
        
        //ok
        public override ASTNode VisitCase_expression([NotNull] PlPgSqlParser.Case_expressionContext context)
        {
            var result = new CaseExpressionNode(context.Start.Line, context.Start.Column)
            {
                Else = context.ELSE() != null,
                Expressions = new List<ExpressionNode>()
            };
            foreach (var exp in context.vex())
            {
                result.Expressions.Add(Visit(exp) as ExpressionNode);
            }
            return result;
        }

        //ok
        public override ASTNode VisitCase_statement([NotNull] PlPgSqlParser.Case_statementContext context)
        {
            var result = new CaseStmtNode(context.Start.Line, context.Start.Column)
            {
                Cases = new List<CaseElement>()
            };

            PlPgSqlParser.VexContext[] expressions = new PlPgSqlParser.VexContext[0];
            if (context.vex() != null)
                expressions = context.vex();

            PlPgSqlParser.Function_statementsContext[] stmt = new PlPgSqlParser.Function_statementsContext[0];
            if(context.function_statements() != null)
                stmt = context.function_statements();

            var end = stmt.Length;
            var startExpressions = 0;
            if (context.ELSE() != null)
            {
                result.ElseStmts = new List<StatementNode>();
                foreach (var s in stmt[--end].function_statement())
                    result.ElseStmts.Add(Visit(s) as StatementNode);
            }

            var when = context.WHEN();
            if (expressions[0].Start.Column < when[0].Symbol.Column || expressions[0].Start.Line <= when[0].Symbol.Line)
                result.HeaderExpression = Visit(expressions[startExpressions++]) as ExpressionNode;
            
            for (int i = 0; i < end; i++)
            {
                var currentsExps = new List<ExpressionNode>();
                while (startExpressions < expressions.Length &&
                //    (expressions[startExpressions].Start.Column < stmt[i].Start.Column ||
                    expressions[startExpressions].Start.Line <= stmt[i].Start.Line)//)
                    currentsExps.Add(Visit(expressions[startExpressions++]) as ExpressionNode);
                var c = new CaseElement()
                {
                    Expressions = currentsExps,
                    Stmts = new List<StatementNode>()
                };
                foreach (var s in stmt[i].function_statement())
                    c.Stmts.Add(Visit(s) as StatementNode);
                result.Cases.Add(c);
            }
            return result;
        }

        //ok
        public override ASTNode VisitCast_specification([NotNull] PlPgSqlParser.Cast_specificationContext context)
        {
            return base.VisitCast_specification(context);
        }

        //ok
        public override ASTNode VisitCharacter_string([NotNull] PlPgSqlParser.Character_stringContext context)
        {
            return new VarcharNode(context.Start.Line, context.Start.Column, 
                (context.Character_String_Literal() != null) ? context.Character_String_Literal().GetText() : context.GetText());
        }

        //ok
        public override ASTNode VisitCollate_identifier([NotNull] PlPgSqlParser.Collate_identifierContext context)
        {
            return base.VisitCollate_identifier(context);
        }

        //ok
        public override ASTNode VisitCol_label([NotNull] PlPgSqlParser.Col_labelContext context)
        {
            if (context.id_token() != null)
            {
                return Visit(context.id_token());
            }
            else if (context.tokens_reserved() != null)
            {
                return new IdNode(context.tokens_reserved().Start.Line,
                    context.tokens_reserved().Start.Column, context.tokens_reserved().GetText());
            }
            else if (context.tokens_nonreserved() != null)
            {
                return new IdNode(context.tokens_nonreserved().Start.Line,
                    context.tokens_nonreserved().Start.Column, context.tokens_nonreserved().GetText());
            }
            else
            {
                return new IdNode(context.tokens_nonreserved_except_function_type().Start.Line,
                    context.tokens_nonreserved_except_function_type().Start.Column, context.tokens_nonreserved_except_function_type().GetText());
            }
        }
        
        //ok
        public override ASTNode VisitComparison_mod([NotNull] PlPgSqlParser.Comparison_modContext context)
        {
            return new ComparisonModNode(context.Start.Line, context.Start.Column)
            {
                All = context.ALL() != null,
                Any = context.ANY() != null,
                Some = context.SOME() != null,
                Expression = (context.vex() != null) ? Visit(context.vex()) as ExpressionNode : null,
                SelectStmtNonParens = (context.select_stmt_no_parens() != null) ?
                    Visit(context.select_stmt_no_parens()) as SelectStmtNonParensNode : null,
            };
        }

        //ok
        public override ASTNode VisitConflict_action([NotNull] PlPgSqlParser.Conflict_actionContext context)
        {
            var result = new ConflictActionNode(context.Start.Line, context.Start.Column)
            {
                Expression = (context.vex() != null) ? Visit(context.vex()) as ExpressionNode : null,
                UpdateSets = (context.update_set() != null) ? new List<UpdateSetNode>() : null
            };
            if(context.update_set() != null)
                foreach (var us in context.update_set())
                {
                    result.UpdateSets.Add(Visit(us) as UpdateSetNode);
                }
            return result;
        }

        //ok
        public override ASTNode VisitConflict_object([NotNull] PlPgSqlParser.Conflict_objectContext context)
        {
            var result = new ConflictObjectNode(context.Start.Line, context.Start.Column)
            {
                SortSpecifiers = (context.index_sort() != null)? new List<SortSpecifierNode>() : null,
                IndexWhere = (context.index_where() != null)? Visit(context.index_where().vex()) as ExpressionNode : null,
                Identifier = (context.identifier() != null)? Visit(context.identifier()) as IdNode : null
            };
            if (context.index_sort() != null)
                foreach (var ss in context.index_sort().sort_specifier_list().sort_specifier())
                {
                    result.SortSpecifiers.Add(Visit(ss) as SortSpecifierNode);
                }
            return result;
        }

        //ok
        public override ASTNode VisitControl_statement([NotNull] PlPgSqlParser.Control_statementContext context)
        {
            if (context.return_stmt() != null)
                return Visit(context.return_stmt());
            else if (context.CALL() != null)
                return new CallFunctionCallNode(context.Start.Line, context.Start.Column)
                {
                    FunctionCall = Visit(context.function_call()) as FunctionCallNode
                };
            else if (context.if_statement() != null)
                return Visit(context.if_statement());
            else if (context.case_statement() != null)
            {
                return Visit(context.case_statement());
            }
            else if(context.loop_statement() != null)
            {
                return Visit(context.loop_statement());
            }
            return null;
        }

        public override ASTNode VisitCopy_from_statement([NotNull] PlPgSqlParser.Copy_from_statementContext context)
        {
            return base.VisitCopy_from_statement(context);
        }

        public override ASTNode VisitCopy_option([NotNull] PlPgSqlParser.Copy_optionContext context)
        {
            return base.VisitCopy_option(context);
        }

        public override ASTNode VisitCopy_option_list([NotNull] PlPgSqlParser.Copy_option_listContext context)
        {
            return base.VisitCopy_option_list(context);
        }

        public override ASTNode VisitCopy_statement([NotNull] PlPgSqlParser.Copy_statementContext context)
        {
            return base.VisitCopy_statement(context);
        }

        public override ASTNode VisitCopy_to_statement([NotNull] PlPgSqlParser.Copy_to_statementContext context)
        {
            return base.VisitCopy_to_statement(context);
        }

        public override ASTNode VisitCreate_table_as_statement([NotNull] PlPgSqlParser.Create_table_as_statementContext context)
        {
            return base.VisitCreate_table_as_statement(context);
        }

        public override ASTNode VisitCreate_view_statement([NotNull] PlPgSqlParser.Create_view_statementContext context)
        {
            return base.VisitCreate_view_statement(context);
        }

        //ok
        public override ASTNode VisitCursor_statement([NotNull] PlPgSqlParser.Cursor_statementContext context)
        {
            var result = new CursorStatementNode(context.Start.Line, context.Start.Column);

            if (context.option() != null && context.option().Length > 0)
            {
                result.Options = new List<OptionNode>();
                foreach (var opt in context.option())
                    result.Options.Add(Visit(opt) as OptionNode);
            }

            if (context.select_stmt() != null)
                result.Stmt = Visit(context.select_stmt()) as SelectStatementNode;

            if (context.execute_stmt() != null)
                result.Stmt = Visit(context.execute_stmt()) as ExecuteStmtNode;

            if (context.var() != null)
                result.Var = Visit(context.var()) as VarNode;

            if (context.into_table() != null)
            {
                result.IntoTable = new List<SchemaQualifieldNode>();
                foreach (var item in context.into_table().schema_qualified_name())
                    result.IntoTable.Add(Visit(item) as SchemaQualifieldNode);
            }
            result.Open = context.OPEN() != null;
            result.Fetch = context.FETCH() != null;
            result.Move = context.MOVE() != null;
            result.Close = context.CLOSE() != null;
            result.From = context.FROM() != null;
            result.In = context.IN() != null;
            var fmd = context.fetch_move_direction();

            if (fmd != null)
            {
                result.FetchMoveIndirection = new FetchMoveIndirectionNode()
                {
                    Next = fmd.NEXT() != null,
                    Prior = fmd.PRIOR() != null,
                    First = fmd.FIRST() != null,
                    Last = fmd.LAST() != null,
                    All = fmd.ALL() != null,
                    Absolute = fmd.ABSOLUTE() != null,
                    Relative = fmd.RELATIVE() != null,
                    Forward = fmd.FORWARD() != null,
                    Backward = fmd.BACKWARD() != null
                };
                if (fmd.NUMBER_LITERAL() != null)
                    result.FetchMoveIndirection.Count = int.Parse(fmd.NUMBER_LITERAL().GetText());
                else if (fmd.signed_number_literal() != null)
                    result.FetchMoveIndirection.Count = int.Parse(fmd.signed_number_literal().GetText());
            }

            return result;
        }

        //ok
        public override ASTNode VisitData_statement([NotNull] PlPgSqlParser.Data_statementContext context)
        {
            if (context.select_stmt() != null)
                return Visit(context.select_stmt());
            else if (context.insert_stmt_for_psql() != null)
                return Visit(context.insert_stmt_for_psql());
            else if (context.update_stmt_for_psql() != null)
                return Visit(context.update_stmt_for_psql());
            else if (context.delete_stmt_for_psql() != null)
                return Visit(context.delete_stmt_for_psql());
            else if (context.truncate_stmt() != null)
                return Visit(context.truncate_stmt());  
            return null;
        }

        //ok
        public override ASTNode VisitData_type([NotNull] PlPgSqlParser.Data_typeContext context)
        {
            var dataType = new DataTypeNode(context.Start.Line, context.Start.Column)
            {
                Type = Visit(context.predefined_type()) as PredefinedTypeNode
            };
            if (context.ARRAY() != null)
            {
                dataType.ArrayType = new List<ArrayTypeNode>();
                foreach (var at in context.array_type())
                {
                    dataType.ArrayType.Add(
                        new ArrayTypeNode(at.Start.Line, at.Start.Column)
                        {
                            Number = (at.NUMBER_LITERAL() != null) ? Int32.Parse(at.NUMBER_LITERAL().GetText()) : 0 
                        });
                }
            }
            return dataType;
        }

        //ok
        public override ASTNode VisitData_type_dec([NotNull] PlPgSqlParser.Data_type_decContext context)
        {
            if (context.data_type() != null)
                return Visit(context.data_type());
            else if (context.schema_qualified_name() != null)
                return new ModularTypeDeclarationNode(context.Start.Line, context.Start.Column)
                {
                    Schema = Visit(context.schema_qualified_name()) as SchemaQualifieldNode
                };
            else 
                return new ModularRowTypeDeclarationNode(context.Start.Line, context.Start.Column)
                {
                    Schema = Visit(context.schema_qualified_name_nontype()) as SchemaQualifiednameNonTypeNode
                };
        }
        
        //ok
        public override ASTNode VisitDatetime_overlaps([NotNull] PlPgSqlParser.Datetime_overlapsContext context)
        {
            return new DatetimeOverlapsNode(context.Start.Line, context.Start.Column)
            {
                Expression1 = Visit(context.vex(0)) as ExpressionNode,
                Expression2 = Visit(context.vex(1)) as ExpressionNode,
                Expression3 = Visit(context.vex(2)) as ExpressionNode,
                Expression4 = Visit(context.vex(3)) as ExpressionNode
            };
        }

        //ok
        public override ASTNode VisitDate_time_function([NotNull] PlPgSqlParser.Date_time_functionContext context)
        {
            if (context.CURRENT_DATE() != null)
                return new CurrentDateFunctionNode(context.Start.Line, context.Start.Column);
            else if(context.CURRENT_TIME() != null)
                return new CurrentTimeFunctionNode(context.Start.Line, context.Start.Column)
                {
                    Length = (context.type_length() != null)? int.Parse(context.type_length().NUMBER_LITERAL().GetText()):0
                };
            else if (context.CURRENT_TIMESTAMP() != null)
                return new CurrentTimestampFunctionNode(context.Start.Line, context.Start.Column)
                {
                    Length = (context.type_length() != null) ? int.Parse(context.type_length().NUMBER_LITERAL().GetText()) : 0
                };
            else if (context.LOCALTIME() != null)
                return new LocalTimeFunctionNode(context.Start.Line, context.Start.Column)
                {
                    Length = (context.type_length() != null) ? int.Parse(context.type_length().NUMBER_LITERAL().GetText()) : 0
                };
            else 
                return new LocalTimestampFunctionNode(context.Start.Line, context.Start.Column)
                {
                    Length = (context.type_length() != null) ? int.Parse(context.type_length().NUMBER_LITERAL().GetText()) : 0
                };
        }

        //ok
        public override ASTNode VisitDeclaration([NotNull] PlPgSqlParser.DeclarationContext context)
        {
            return new DeclarationNode(context.Start.Line, context.Start.Column)
            {
                Identifier = Visit(context.identifier()) as IdNode,
                TypeDeclaration = Visit(context.type_declaration()) as TypeDeclarationNode
            };
        }

        //ok
        public override ASTNode VisitDeclarations([NotNull] PlPgSqlParser.DeclarationsContext context)
        {
            return null;
        }

        //ok
        public override ASTNode VisitDeclare_statement([NotNull] PlPgSqlParser.Declare_statementContext context)
        {
            return new DeclareStatementNode(context.Start.Line, context.Start.Column)
            {
                Identifier = Visit(context.identifier()) as IdNode,
                SelectStmt = Visit(context.select_stmt()) as SelectStatementNode
            };
        }

        //ok
        public override ASTNode VisitDelete_stmt_for_psql([NotNull] PlPgSqlParser.Delete_stmt_for_psqlContext context)
        {
            var result = new DeleteStmtPSqlNode(context.Start.Line, context.Start.Column)
            {
                WithClause = (context.with_clause() != null)? Visit(context.with_clause()) 
                        as WithClauseNode : null,
                DeleteTableName = Visit(context.delete_table_name) as SchemaQualifieldNode,
                Alias = (context.alias != null)? Visit(context.alias) as IdNode : null,
                FromItems = (context.from_item() != null)? new List<FromItemNode>() : null,
                Expression = (context.vex() != null)? Visit(context.vex()) as ExpressionNode : null,
                Cursor = (context.cursor != null)? Visit(context.cursor)  as IdNode : null,
                SelectList = (context.select_list() != null)? Visit(context.select_list())
                        as SelectListNode : null
            };
            if (context.from_item() != null)
                foreach (var fi in context.from_item())
                    result.FromItems.Add(Visit(fi) as FromItemNode);

            return result;
        }

        public override ASTNode VisitDiagnostic_option([NotNull] PlPgSqlParser.Diagnostic_optionContext context)
        {
            return base.VisitDiagnostic_option(context);
        }

        public override ASTNode VisitDollar_number([NotNull] PlPgSqlParser.Dollar_numberContext context)
        {
            return base.VisitDollar_number(context);
        }

        //ok
        public override ASTNode VisitException_statement([NotNull] PlPgSqlParser.Exception_statementContext context)
        {
            if (context == null) return null;
            var result = new ExceptionStatementNode(context.Start.Line, context.Start.Column)
            {
                Statements = new List<List<StatementNode>>(),
                Expressions = new List<ExpressionNode>()
            };

            foreach (var stmts in context.function_statements())
            {
                var currentStatements = new List<StatementNode>();
                foreach (var stmt in stmts.function_statement())
                {
                    currentStatements.Add(Visit(stmt) as StatementNode);
                }
                result.Statements.Add(currentStatements);
            }

            foreach (var expr in context.vex())
                result.Expressions.Add(Visit(expr) as ExpressionNode);
            return result;
        }

        //ok
        public override ASTNode VisitExecute_statement([NotNull] PlPgSqlParser.Execute_statementContext context)
        {
            var result = new ExecuteStatementNode(context.Start.Line, context.Start.Column)
            {
                Identifier = Visit(context.identifier()) as IdNode,
                Expressions = (context.vex() != null)? new List<ExpressionNode>() : null 
            };
            if (context.vex() != null)
            {
                foreach (var exp in context.vex())
                {
                    result.Expressions.Add(Visit(exp) as ExpressionNode);
                }
            }
            return result;
        }

        //ok
        public override ASTNode VisitExecute_stmt([NotNull] PlPgSqlParser.Execute_stmtContext context)
        {
            var result = new ExecuteStmtNode(context.Start.Line, context.Start.Column)
            {
                Expression = Visit(context.vex()) as ExpressionNode,
                UsingExpression = new List<ExpressionNode>()
            };
            foreach (var expr in context.using_vex().vex())
            {
                result.UsingExpression.Add(Visit(expr) as ExpressionNode);
            }
            return result;
        }

        public override ASTNode VisitExplain_option([NotNull] PlPgSqlParser.Explain_optionContext context)
        {
            return base.VisitExplain_option(context);
        }

        //ok
        public override ASTNode VisitExplain_query([NotNull] PlPgSqlParser.Explain_queryContext context)
        {
            if (context.data_statement() != null)
                return Visit(context.data_statement());
            else if (context.values_stmt() != null)
                return Visit(context.values_stmt());
            else if (context.execute_statement() != null)
                return Visit(context.execute_statement());
            else if (context.declare_statement() != null)
                return Visit(context.declare_statement());
            else if (context.create_table_as_statement() != null)
                return Visit(context.create_table_as_statement());
            else return Visit(context.create_view_statement());
        }

        //ok
        public override ASTNode VisitExplain_statement([NotNull] PlPgSqlParser.Explain_statementContext context)
        {
            var result = new ExplainStmtNode(context.Start.Line, context.Start.Column)
            {
                ExplainQuery = Visit(context.explain_query()) as StatementNode,
                Options = (context.explain_option() != null) ? new List<string>() : null
            };
            if (context.explain_option() != null)
            {
                foreach (var item in context.explain_option())
                {
                    result.Options.Add(item.GetText());
                }
            }
            return result;
        }

        //ok
        public override ASTNode VisitExtract_function([NotNull] PlPgSqlParser.Extract_functionContext context)
        {
            return new ExtractFunctionNode(context.Start.Line, context.Start.Column)
            {
                Identifier = (context.identifier() != null)?
                    Visit(context.identifier()) as IdNode : null,
                Expression = (context.vex() != null)? Visit(context.vex()) as ExpressionNode : null,
                CharacterString = (context.character_string() != null)? context.character_string().GetText() : null
            };
        }

        //ok
        public override ASTNode VisitFetch_move_direction([NotNull] PlPgSqlParser.Fetch_move_directionContext context)
        {
            return base.VisitFetch_move_direction(context);
        }

        //ok
        public override ASTNode VisitFilter_clause([NotNull] PlPgSqlParser.Filter_clauseContext context)
        {
            return base.VisitFilter_clause(context);
        }

        //ok
        public override ASTNode VisitFrame_bound([NotNull] PlPgSqlParser.Frame_boundContext context)
        {
            return new FrameBoundNode(context.Start.Line, context.Start.Column)
            {
                CurrentNow = context.CURRENT() != null,
                Expression = (context.vex() != null)? Visit(context.vex()) as ExpressionNode :  null,
                Preceding = context.PRECEDING() != null
            };
        }

        //ok
        public override ASTNode VisitFrame_clause([NotNull] PlPgSqlParser.Frame_clauseContext context)
        {
            var result = new FrameClauseNode(context.Start.Line, context.Start.Column)
            {
                Range = context.RANGE() != null, 
                Rows = context.ROWS() != null,
                Groups =  context.GROUPS() != null,
                FrameBounds = new List<FrameBoundNode>(),
                ExcludeCurrentRow = context.EXCLUDE() != null && context.CURRENT() != null,
                ExcludeGroup = context.EXCLUDE() != null && context.GROUP() != null,
                ExcludeTies = context.EXCLUDE() !=null && context.TIES() != null,
                ExcludeNoOthers = context.EXCLUDE() != null && context.OTHERS() != null
            };
            foreach (var item in context.frame_bound())
            {
                result.FrameBounds.Add(Visit(item) as FrameBoundNode);
            }
            return result;
        }

        //ok
        public override ASTNode VisitFrom_function_column_def([NotNull] PlPgSqlParser.From_function_column_defContext context)
        {
            var result = new FromFunctionColumnDefNode(context.Start.Line, context.Start.Column)
            {
                ColumnsAlias = new List<IdNode>(),
                DataTypes = new List<DataTypeNode>()
            };

            foreach (var ca in context._column_alias)
                result.ColumnsAlias.Add(Visit(ca) as IdNode);
            foreach (var dt in context.data_type())
                result.DataTypes.Add(Visit(dt) as DataTypeNode);

            return result;
        }
        
        //ok
        public override ASTNode VisitFrom_item([NotNull] PlPgSqlParser.From_itemContext context)
        {
            if (context.from_primary() != null)
                return Visit(context.from_primary());
            else if (context.CROSS() != null)
                return new FromItemCrossJoinNode(context.Start.Line, context.Start.Column)
                {
                    Item1 = Visit(context.from_item(0)) as FromItemNode,
                    Item2 = Visit(context.from_item(1)) as FromItemNode,
                };
            else if (context.ON() != null)
                return new FromItemOnExpressionNode(context.Start.Line, context.Start.Column)
                {
                    Item1 = Visit(context.from_item(0)) as FromItemNode,
                    Item2 = Visit(context.from_item(1)) as FromItemNode,
                    Expression = Visit(context.vex()) as ExpressionNode,
                    Full = context.FULL() != null,
                    Inner = context.INNER() != null,
                    Left = context.LEFT() != null,
                    Right = context.RIGHT() != null,
                    Outer = context.OUTER() != null
                };
            else if (context.USING() != null)
            {
                var result = new FromItemUsingNode(context.Start.Line, context.Start.Column)
                {
                    Item1 = Visit(context.from_item(0)) as FromItemNode,
                    Item2 = Visit(context.from_item(1)) as FromItemNode,
                    NamesInParens = new List<SchemaQualifieldNode>(),
                    Full = context.FULL() != null,
                    Inner = context.INNER() != null,
                    Left = context.LEFT() != null,
                    Right = context.RIGHT() != null,
                    Outer = context.OUTER() != null
                };
                foreach (var name in context.names_in_parens().names_references().schema_qualified_name())
                {
                    result.NamesInParens.Add(Visit(name) as SchemaQualifieldNode);
                }
                return result;
            }
            else if (context.ON() != null)
                return new FromItemNaturalNode(context.Start.Line, context.Start.Column)
                {
                    Item1 = Visit(context.from_item(0)) as FromItemNode,
                    Item2 = Visit(context.from_item(1)) as FromItemNode,
                    Full = context.FULL() != null,
                    Inner = context.INNER() != null,
                    Left = context.LEFT() != null,
                    Right = context.RIGHT() != null,
                    Outer = context.OUTER() != null
                };
            else
            {
                return new FromItemSimpleNode(context.Start.Line, context.Start.Column)
                {
                    Item = Visit(context.from_item(0)) as FromItemNode,
                    AliasClause = (context.alias_clause() != null) ? Visit(context.alias_clause())
                            as AliasClauseNode : null
                };
            }
        }

        //ok
        public override ASTNode VisitFrom_primary([NotNull] PlPgSqlParser.From_primaryContext context)
        {
            if (context.schema_qualified_name() != null)
            {
                var result = new FromPrimary1Node(context.Start.Line, context.Start.Column)
                {
                    SchemaQualifield = Visit(context.schema_qualified_name())
                              as SchemaQualifieldNode,
                    AliasClause = (context.alias_clause() != null) ? Visit(context.alias_clause())
                              as AliasClauseNode : null,
                    Methods = (context.method != null) ? Visit(context.method) as IdNode : null,
                    Repeatable = context.REPEATABLE() != null,
                    Expressions = (context.vex() != null) ? new List<ExpressionNode>() : null
                };
                if (context.vex() != null)
                    foreach (var expr in context.vex())
                        result.Expressions.Add(Visit(expr) as ExpressionNode);
                return result;
            }
            else if (context.table_subquery() != null)
            {
                return new FromPrimary2Node(context.Start.Line, context.Start.Column)
                {
                    TableSubquery = Visit(context.table_subquery().select_stmt()) as SelectStatementNode,
                    AliasClause = Visit(context.alias_clause()) as AliasClauseNode
                };
            }
            else if (context.ROWS() != null)
            {
                var result = new FromPrimary4Node(context.Start.Line, context.Start.Column)
                {
                    FunctionCalls = new List<FunctionCallNode>(),
                    ColumnsDef = (context.from_function_column_def() != null) ? new List<FromFunctionColumnDefNode>() : null,
                    IdAlias = (context.alias != null) ? Visit(context.alias) as IdNode : null,
                    ColumnAlias = (context._column_alias != null) ? new List<IdNode>() : null
                };
                foreach (var fc in context.function_call())
                    result.FunctionCalls.Add(Visit(fc) as FunctionCallNode);
                if (context.from_function_column_def() != null)
                    foreach (var cd in context.from_function_column_def())
                        result.ColumnsDef.Add(Visit(cd) as FromFunctionColumnDefNode);
                if (context._column_alias != null)
                    foreach (var ca in context._column_alias)
                        result.ColumnAlias.Add(Visit(ca) as IdNode);
                return result;
            }
            else
            {
                var result = new FromPrimary3Node(context.Start.Line, context.Start.Column)
                {
                    FunctionCall = Visit(context.function_call(0)) as FunctionCallNode,
                    ParamsDef = (context.from_function_column_def() != null)? 
                        new List<FromFunctionColumnDefNode>() : null, 
                    IdAlias = (context.alias != null)? Visit(context.alias) as IdNode : null,
                    ColumnAlias = (context._column_alias != null)? new List<IdNode>(): null
                };
                if (context.from_function_column_def() != null)
                    foreach (var cd in context.from_function_column_def())
                        result.ParamsDef.Add(Visit(cd) as FromFunctionColumnDefNode);
                if (context._column_alias != null)
                    foreach (var ca in context._column_alias)
                        result.ColumnAlias.Add(Visit(ca) as IdNode);
                return result;
            }
        }

        //ok
        public override ASTNode VisitFunction_block([NotNull] PlPgSqlParser.Function_blockContext context)
        {
            var functionBlock = new FunctionBlockNode(context.Start.Line, context.Start.Column)
            {
                ExceptionStatement = (context.exception_statement() != null) ? 
                Visit(context.exception_statement()) as ExceptionStatementNode : null
            };
            if (context.declarations() != null)
            {
                functionBlock.Declarations = new List<DeclarationNode>();
                foreach (var declaration in context.declarations().declaration())
                    functionBlock.Declarations.Add(Visit(declaration) as DeclarationNode);
            }
            functionBlock.Statements = new List<StatementNode>();
            foreach (var stmt in context.function_statements().function_statement())
                functionBlock.Statements.Add(Visit(stmt) as StatementNode);
            
            return functionBlock;
        }
        
        //ok
        public override ASTNode VisitFunction_body([NotNull] PlPgSqlParser.Function_bodyContext context)
        {
            return VisitFunction_block(context.function_block());
        }

        //ok
        public override ASTNode VisitFunction_call([NotNull] PlPgSqlParser.Function_callContext context)
        {
            if (context.function_construct() != null)
                return Visit(context.function_construct());
            else if (context.extract_function() != null)
                return Visit(context.extract_function());
            else if (context.system_function() != null)
                return Visit(context.system_function());
            else if (context.date_time_function() != null)
                return Visit(context.date_time_function());
            else if (context.string_value_function() != null)
                return Visit(context.string_value_function());
            else if (context.xml_function() != null)
                return Visit(context.xml_function());
            else
            {
                var result = new BasicFunctionCallNode(context.Start.Line, context.Start.Column)
                {
                    SchemaQualifiednameNonType = Visit(context.schema_qualified_name_nontype())
                        as SchemaQualifiednameNonTypeNode,
                    Qualifier = (context.set_qualifier() != null) ? context.set_qualifier().GetText() : null,
                    VexOrNamedNotations = (context.vex_or_named_notation() != null)? new List<VexOrNamedNotationNode>() : null,
                    OrderByClause = (context.orderby_clause() != null)? new List<OrderByClauseNode>() : null,
                    Filter = (context.filter_clause() != null)? Visit(context.filter_clause().vex()) as ExpressionNode : null,
                    WithinGroup = context.WITHIN() != null,
                    Identifier = (context.identifier() != null)? Visit(context.identifier()) as IdNode : null,
                    WindowsDefinition = (context.window_definition() != null)? 
                        Visit(context.window_definition()) as WindowsDefinitionNode : null
                };
                if (context.vex_or_named_notation() != null)
                    foreach (var item in context.vex_or_named_notation())
                        result.VexOrNamedNotations.Add(Visit(item) as VexOrNamedNotationNode);
                if (context.orderby_clause() != null)
                    foreach (var item in context.orderby_clause())
                        result.OrderByClause.Add(Visit(item) as OrderByClauseNode);
                return result;
            }
        }

        //ok
        public override ASTNode VisitFunction_construct([NotNull] PlPgSqlParser.Function_constructContext context)
        {
            var result = new FunctionConstructNode(context.Start.Line, context.Start.Column)
            {
                Expressions = new List<ExpressionNode>(),
                Row = context.ROW() != null,
                Coalesce = context.COALESCE() != null,
                Greatest = context.GREATEST() != null, 
                Grouping = context.GROUPING() != null,
                Least = context.LEAST() != null,
                Nullif = context.NULLIF() != null, 
                XmlConcat = context.XMLCONCAT() != null
            };
            foreach (var exp in context.vex())
                result.Expressions.Add(Visit(exp) as ExpressionNode);
            return result;
        }

        //ok
        public override ASTNode VisitFunction_statement([NotNull] PlPgSqlParser.Function_statementContext context)
        {
            if (context.function_block() != null)
                return Visit(context.function_block());
            else if (context.base_statement() != null)
                return Visit(context.base_statement());
            else if (context.control_statement() != null)
                return Visit(context.control_statement());
            else if (context.cursor_statement() != null)
                return Visit(context.cursor_statement());
            else if (context.message_statement() != null)
                return Visit(context.message_statement());
            else if (context.data_statement() != null)
                return Visit(context.data_statement());
            else if (context.transaction_statement() != null)
                return Visit(context.transaction_statement());
            else if (context.additional_statement() != null)
                return Visit(context.additional_statement());
            return null;
        }

        //ok
        public override ASTNode VisitFunction_statements([NotNull] PlPgSqlParser.Function_statementsContext context)
        {
            return base.VisitFunction_statements(context);
        }

        //ok
        public override ASTNode VisitGroupby_clause([NotNull] PlPgSqlParser.Groupby_clauseContext context)
        {
            var result = new GroupByClauseNode(context.Start.Line, context.Start.Column)
            {
                Expressions = new List<ExpressionNode>()
            };

            foreach (var g in context.grouping_element_list().grouping_element())
                if (g.vex() != null)
                    result.Expressions.Add(Visit(g.vex()) as ExpressionNode);
            return result;
        }
        //ok
        public override ASTNode VisitGrouping_element([NotNull] PlPgSqlParser.Grouping_elementContext context)
        {
            return base.VisitGrouping_element(context);
        }
        //ok
        public override ASTNode VisitGrouping_element_list([NotNull] PlPgSqlParser.Grouping_element_listContext context)
        {
            return base.VisitGrouping_element_list(context);
        }

        //ok
        public override ASTNode VisitIdentifier([NotNull] PlPgSqlParser.IdentifierContext context)
        {
            if (context.id_token() != null) return Visit(context.id_token());
            else if (context.tokens_nonreserved() != null)
                return new IdNode(context.Start.Line,
                    context.Start.Column, context.tokens_nonreserved().GetText());
            else return new IdNode(context.Start.Line, context.Start.Column, context.tokens_nonreserved_except_function_type().GetText());
        }

        //ok
        public override ASTNode VisitIdentifier_list([NotNull] PlPgSqlParser.Identifier_listContext context)
        {
            return base.VisitIdentifier_list(context);
        }

        //ok
        public override ASTNode VisitIdentifier_nontype([NotNull] PlPgSqlParser.Identifier_nontypeContext context)
        {
            if (context.id_token() != null) return Visit(context.id_token());
            else if (context.tokens_nonreserved() != null)
                return new IdentifierNonTypeNode(context.Start.Line,
                    context.Start.Column, context.tokens_nonreserved().GetText());
            else return new IdentifierNonTypeNode(context.Start.Line,
                    context.Start.Column, context.tokens_reserved_except_function_type().GetText());
        }

        //ok
        public override ASTNode VisitId_token([NotNull] PlPgSqlParser.Id_tokenContext context)
        {
            if (context.Identifier() != null)
            {
                return new IdNode(context.Identifier().Symbol.Line,
                    context.Identifier().Symbol.Column, context.Identifier().Symbol.Text);
            }
            else if (context.QuotedIdentifier() != null)
            {
                return new IdNode(context.QuotedIdentifier().Symbol.Line,
                    context.QuotedIdentifier().Symbol.Column, context.QuotedIdentifier().Symbol.Text);
            }
            else
            {
                return new IdNode(context.Start.Line,
                    context.Start.Column, context.tokens_nonkeyword().GetText());
            }
        }

        public override ASTNode VisitIf_not_exists([NotNull] PlPgSqlParser.If_not_existsContext context)
        {
            return base.VisitIf_not_exists(context);
        }

        //ok
        public override ASTNode VisitIf_statement([NotNull] PlPgSqlParser.If_statementContext context)
        {
            var statements = context.function_statements();
            var end = statements.Length - 1;
            PlPgSqlParser.Function_statementsContext elseStmts = null;
            if (context.ELSE() != null)
                elseStmts = statements[end--];
            var result = new IfStmtNode(context.Start.Line, context.Start.Column)
            {
                ElseStatements = (context.ELSE() != null) ? new List<StatementNode>() : null,
                Expressions = new List<ExpressionNode>(),
                Statements = new List<List<StatementNode>>()
            };

            foreach (var ex in context.vex())
                result.Expressions.Add(Visit(ex) as ExpressionNode);

            if (elseStmts != null)
                foreach (var s in elseStmts.function_statement())
                    result.ElseStatements.Add(Visit(s) as StatementNode);

            for (int i = 0; i <= end; i++)
            {
                var lStmts = new List<StatementNode>();
                foreach (var s in statements[i].function_statement())
                    lStmts.Add(Visit(s) as StatementNode);
                result.Statements.Add(lStmts);
            }
            return result;
        }

        public override ASTNode VisitIndex_sort([NotNull] PlPgSqlParser.Index_sortContext context)
        {
            return base.VisitIndex_sort(context);
        }

        public override ASTNode VisitIndex_where([NotNull] PlPgSqlParser.Index_whereContext context)
        {
            return base.VisitIndex_where(context);
        }

        //ok
        public override ASTNode VisitIndirection([NotNull] PlPgSqlParser.IndirectionContext context)
        {
            var result = new IndirectionNode(context.Start.Line, context.Start.Column);
            result.ColLabel = (context.col_label() != null)?Visit(context.col_label()) as IdNode : null;
            if (context.vex() != null && context.vex().Length > 0)
            {
                result.Expressions = new List<ExpressionNode>();

                foreach (var expr in context.vex())
                {
                    result.Expressions.Add(VisitVex(expr) as ExpressionNode);
                }
            }
            return result;
        }

        //ok
        public override ASTNode VisitIndirection_identifier([NotNull] PlPgSqlParser.Indirection_identifierContext context)
        {
            var result = new IndirectionIdentifierNode(context.Start.Line, context.Start.Column)
            {
                Identifier = Visit(context.identifier()) as IdNode
            };
            if (context.indirection_list() != null)
            {
                result.Indirections = new List<IndirectionNode>();
                foreach (var i in context.indirection_list().indirection())
                {
                    result.Indirections.Add(Visit(i) as IndirectionNode);
                }
            }
            return result;
        }

        public override ASTNode VisitIndirection_list([NotNull] PlPgSqlParser.Indirection_listContext context)
        {
            return base.VisitIndirection_list(context);
        }
        
        //ok
        public override ASTNode VisitIndirection_var([NotNull] PlPgSqlParser.Indirection_varContext context)
        {
            var result = new IndirectionVarNode(context.Start.Line, context.Start.Column)
            {
                Identifiers = (context.identifier() != null)?
                    Visit(context.identifier()) as IdNode :
                    new DollarNumberNode(context.dollar_number().Start.Line, context.dollar_number().Start.Column, context.dollar_number().GetText()),
                Indirections = (context.indirection_list() != null)? new List<IndirectionNode>() : null
            };
            if (context.indirection_list() != null)
            {
                foreach (var item in context.indirection_list().indirection())
                {
                    result.Indirections.Add(Visit(item) as IndirectionNode);
                }
            }
            return result;;
        }

        public override ASTNode VisitInsert_columns([NotNull] PlPgSqlParser.Insert_columnsContext context)
        {
            return base.VisitInsert_columns(context);
        }

        //ok
        public override ASTNode VisitInsert_stmt_for_psql([NotNull] PlPgSqlParser.Insert_stmt_for_psqlContext context)
        {
            var result = new InsertStmtPSqlNode(context.Start.Line, context.Start.Column)
            {
                WithClause = (context.with_clause() != null)? Visit(context.with_clause())
                        as WithClauseNode : null,
                InsertTableName = (context.insert_table_name != null)? Visit(context.insert_table_name)
                        as SchemaQualifieldNode : null,
                Alias = (context.alias != null)? Visit(context.alias) as IdNode : null,
                InsertColumns = (context.insert_columns() != null)? new List<IndirectionIdentifierNode>() : null,
                SelectStmt = Visit(context.select_stmt()) as SelectStatementNode,
                ConflictObject = (context.conflict_object() != null)? Visit(context.conflict_object()) 
                        as ConflictObjectNode : null,
                ConflictAction = (context.conflict_action() != null)? Visit(context.conflict_action()) 
                        as ConflictActionNode : null,
                SelectList = (context.select_list() != null)? Visit(context.select_list()) 
                        as SelectListNode : null
            };
            if (context.insert_columns() != null)
            {
                foreach (var ii in context.insert_columns().indirection_identifier())
                {
                    result.InsertColumns.Add(Visit(ii) as IndirectionIdentifierNode);
                }
            }
            return result;
        }

        //ok
        public override ASTNode VisitInterval_field([NotNull] PlPgSqlParser.Interval_fieldContext context)
        {
            var result = new IntervalFieldNode(context.Start.Line, context.Start.Column);

            if (context.YEAR() != null && context.MONTH() != null)
                result.Text = "YEAR TO MONTH";
            else if (context.DAY() != null && context.HOUR() != null)
                result.Text = "DAY TO HOUR";
            else if (context.DAY() != null && context.MINUTE() != null)
                result.Text = "DAY TO MINUTE";
            else if (context.DAY() != null && context.SECOND() != null)
                result.Text = "DAY TO SECOND";
            else if (context.HOUR() != null && context.MINUTE() != null)
                result.Text = "HOUR TO MINUTE";
            else if (context.HOUR() != null && context.SECOND() != null)
                result.Text = "HOUR TO SECOND";
            else if (context.MINUTE() != null && context.SECOND() != null)
                result.Text = "MINUTE TO SECOND";
            else if (context.YEAR() != null)
                result.Text = "YEAR";
            else if (context.MONTH() != null)
                result.Text = "MONTH";
            else if (context.DAY() != null)
                result.Text = "DAY";
            else if (context.HOUR() != null)
                result.Text = "HOUR";
            else if (context.MINUTE() != null)
                result.Text = "MINUTE";
            else if (context.SECOND() != null)
                result.Text = "SECOND";
            
            return result;
        }

        public override ASTNode VisitInto_table([NotNull] PlPgSqlParser.Into_tableContext context)
        {
            return base.VisitInto_table(context);
        }

        public override ASTNode VisitLock_mode([NotNull] PlPgSqlParser.Lock_modeContext context)
        {
            return base.VisitLock_mode(context);
        }

        public override ASTNode VisitLock_table([NotNull] PlPgSqlParser.Lock_tableContext context)
        {
            return base.VisitLock_table(context);
        }

        public override ASTNode VisitLog_level([NotNull] PlPgSqlParser.Log_levelContext context)
        {
            return base.VisitLog_level(context);
        }

        //ok
        public override ASTNode VisitLoop_start([NotNull] PlPgSqlParser.Loop_startContext context)
        {
            LoopStartNode result = null;

            if (context.WHILE() != null)
            {
                result = new WhileLoopNode(context.Start.Line, context.Start.Column)
                {
                    Expressions = new List<ExpressionNode>()
                };
                foreach (var expr in context.vex())
                {
                    (result as WhileLoopNode).Expressions.Add(Visit(expr) as ExpressionNode);
                }
            }
            else if (context.alias != null)
            {
                result = new ForAliasLoopNode(context.Start.Line, context.Start.Column)
                {
                    Identifier = new IdNode(context.alias.Start.Line, context.alias.Start.Column, context.alias.GetText()),
                    Expressions = new List<ExpressionNode>()
                };
                foreach (var expr in context.vex())
                {
                    (result as ForAliasLoopNode).Expressions.Add(Visit(expr) as ExpressionNode);
                }
            }
            else if (context.identifier_list() != null && context.FOR() != null)
            {
                result = new ForIdListLoopNode(context.Start.Line, context.Start.Column)
                {
                    Identifiers = new List<IdNode>()
                };
                foreach (var id in context.identifier_list().identifier())
                    (result as ForIdListLoopNode).Identifiers.Add(Visit(id) as IdNode);
                if (context.select_stmt() != null)
                    (result as ForIdListLoopNode).Stmt = Visit(context.select_stmt()) as SelectStatementNode;
                else if (context.execute_stmt() != null)
                    (result as ForIdListLoopNode).Stmt = Visit(context.execute_stmt()) as ExecuteStmtNode;
            }
            else if (context.cursor != null)
            {
                result = new ForCursorLoopNode(context.Start.Line, context.Start.Column)
                {
                    Cursor = new IdNode(context.cursor.Start.Line, context.cursor.Start.Column, context.cursor.GetText()),
                    Identifier = new IdNode(context.identifier(1).Start.Line, context.identifier(1).Start.Column, context.identifier(1).GetText()),
                    Options = new List<OptionNode>()
                };
                foreach (var opt in context.option())
                {
                    (result as ForCursorLoopNode).Options.Add(Visit(opt) as OptionNode);
                }
            }
            else if (context.FOREACH() != null)
            {
                result = new ForeachLoopNode(context.Start.Line, context.Start.Column)
                {
                    Expression = Visit(context.vex(context.vex().Length - 1)) as ExpressionNode,
                    Identifiers = new List<IdNode>()
                };

                foreach (var id in context.identifier_list().identifier())
                    (result as ForeachLoopNode).Identifiers.Add(Visit(id) as IdNode);
            }
            return result;
        }

        //ok
        public override ASTNode VisitLoop_statement([NotNull] PlPgSqlParser.Loop_statementContext context)
        {
            var result = new LoopStmtNode(context.Start.Line, context.Start.Column);
            if (context.function_statements() != null)
            {
                if (context.loop_start() != null)
                    result.LoopStart = Visit(context.loop_start()) as LoopStartNode;
                result.Statemets = new List<StatementNode>();
                foreach (var stmt in context.function_statements().function_statement())
                    result.Statemets.Add(Visit(stmt) as StatementNode);
            }
            else if (context.vex() != null)
                result.Expression = Visit(context.vex()) as ExpressionNode;
            result.Exit = context.EXIT() != null;
            result.Continue = context.CONTINUE() != null;
            return result;
        }

        //ok
        public override ASTNode VisitMessage_statement([NotNull] PlPgSqlParser.Message_statementContext context)
        {
            if (context.ASSERT() != null)
            {
                var result = new AssertMessageStatementNode(context.Start.Line, context.Start.Column)
                {
                    Expressions = new List<ExpressionNode>()
                };
                foreach (var expr in context.vex())
                    result.Expressions.Add(Visit(expr) as ExpressionNode);
                return result;
            }
            else
            {
                var result = new RaiseMessageStatementNode(context.Start.Line, context.Start.Column)
                {
                    LogLevel = (context.log_level() != null) ? context.log_level().GetText() : null,
                    Identifier = (context.identifier() != null) ? Visit(context.identifier()) as IdNode : null,
                    Message = (context.character_string() != null) ? context.character_string().GetText() : null,
                    Expressions = (context.vex() != null) ? new List<ExpressionNode>() : null
                };

                if(context.vex() != null)
                    foreach (var exp in context.vex())
                        result.Expressions.Add(Visit(exp) as ExpressionNode);
                    
                if (context.raise_using() != null)
                {
                    result.RaiseUsing = new RaiseUsing()
                    {
                        Expressions = new List<ExpressionNode>(),
                        RaiseParams = new List<string>()
                    };
                    foreach (var rp in context.raise_using().raise_param())
                    {
                        result.RaiseUsing.RaiseParams.Add(rp.GetText());
                    }
                    foreach (var exp in context.raise_using().vex())
                    {
                        result.RaiseUsing.Expressions.Add(Visit(exp) as ExpressionNode);
                    }
                }
                return result;
            }
            

        }

        public override ASTNode VisitNames_in_parens([NotNull] PlPgSqlParser.Names_in_parensContext context)
        {
            return base.VisitNames_in_parens(context);
        }

        public override ASTNode VisitNames_references([NotNull] PlPgSqlParser.Names_referencesContext context)
        {
            return base.VisitNames_references(context);
        }

        public override ASTNode VisitNotify_stmt([NotNull] PlPgSqlParser.Notify_stmtContext context)
        {
            return base.VisitNotify_stmt(context);
        }

        public override ASTNode VisitNull_ordering([NotNull] PlPgSqlParser.Null_orderingContext context)
        {
            return new NullOrderingNode(context.Start.Line, context.Start.Column)
            {
                First = context.FIRST() != null
            };
        }

        public override ASTNode VisitOnly_table_multiply([NotNull] PlPgSqlParser.Only_table_multiplyContext context)
        {
            return base.VisitOnly_table_multiply(context);
        }

        public override ASTNode VisitOn_commit([NotNull] PlPgSqlParser.On_commitContext context)
        {
            return base.VisitOn_commit(context);
        }

        public override ASTNode VisitOp([NotNull] PlPgSqlParser.OpContext context)
        {
            return new OpNode(context.Start.Line, context.Start.Column)
            {
                Identifier = (context.identifier() != null)? Visit(context.identifier()) as IdNode : null,
                Operator = (context.op_chars() != null) ? context.op_chars().GetText() : context.all_simple_op().GetText()
            };
        }

        public override ASTNode VisitOption([NotNull] PlPgSqlParser.OptionContext context)
        {
            return new OptionNode(context.Start.Line, context.Start.Column)
            {
                Expression = Visit(context.vex()) as ExpressionNode,
                Identifier = (context.identifier() != null)? 
                    new IdNode(context.identifier().Start.Line, context.identifier().Start.Column, context.identifier().GetText()) : null
            }; 
        }

        //ok
        public override ASTNode VisitOp_chars([NotNull] PlPgSqlParser.Op_charsContext context)
        {
            var result = new OpCharsNode(context.Start.Line, context.Start.Column);
            if (context.OP_CHARS() != null) result.Symbol = context.OP_CHARS().GetText();
            else if (context.LESS_LESS() != null) result.Symbol = context.LESS_LESS().GetText();
            else if (context.GREATER_GREATER() != null) result.Symbol = context.GREATER_GREATER().GetText();
            else result.Symbol = context.HASH_SIGN().GetText();
            return result;
        }
        //ok
        public override ASTNode VisitOrderby_clause([NotNull] PlPgSqlParser.Orderby_clauseContext context)
        {
            var result = new OrderByClauseNode(context.Start.Line, context.Start.Column)
            {
                SortSpecifiers = new List<SortSpecifierNode>()
            };
            foreach (var ss in context.sort_specifier_list().sort_specifier())
            {
                result.SortSpecifiers.Add(Visit(ss) as SortSpecifierNode);
            }
            return result;
        }

        //ok
        public override ASTNode VisitOrder_specification([NotNull] PlPgSqlParser.Order_specificationContext context)
        {
            return new OrderSpecificationNode(context.Start.Line, context.Start.Column)
            {
                Asc = context.ASC() != null,
                Using = (context.all_op_ref() != null)? Visit(context.all_op_ref()) as AllOpRefNode : null
            };
        }

        public override ASTNode VisitPartition_by_columns([NotNull] PlPgSqlParser.Partition_by_columnsContext context)
        {
            return base.VisitPartition_by_columns(context);
        }
        
        //ok
        public override ASTNode VisitPerform_stmt([NotNull] PlPgSqlParser.Perform_stmtContext context)
        {
            var result = new PerformStmtNode(context.Start.Line, context.Start.Column);
            if (context.set_qualifier() != null && context.set_qualifier().Length > 0)
            {
                if (context.set_qualifier().Length == 2)
                {
                    result.SetQualifier = context.set_qualifier(0).GetText();
                    result.SetQualifier1 = context.set_qualifier(1).GetText();
                }
                if(context.set_qualifier().Length == 1 && (context.INTERSECT() != null ||
                    context.UNION() != null || context.EXCEPT() != null))
                    result.SetQualifier1 = context.set_qualifier(0).GetText();
                else
                    result.SetQualifier = context.set_qualifier(0).GetText();
            }

            if (context.vex() != null)
            {
                result.Expressions = new List<ExpressionNode>();
                foreach (var expr in context.vex())
                    result.Expressions.Add(Visit(expr) as ExpressionNode);
            }

            result.SelectList = Visit(context.select_list()) as SelectListNode;

            if (context.from_item() != null)
            {
                result.FromItems = new List<FromItemNode>();
                foreach (var fi in context.from_item())
                {
                    result.FromItems.Add(Visit(fi) as FromItemNode);
                }
            }

            if (context.WHERE() != null)
                result.Where = true;

            if (context.groupby_clause() != null)
                result.GroupByClause = Visit(context.groupby_clause()) as GroupByClauseNode;

            if (context.HAVING() != null)
                result.Having = true;

            if (context.identifier() != null)
            {
                result.Identifiers = new List<IdNode>();
                foreach (var id in context.identifier())
                    result.Identifiers.Add(Visit(id) as IdNode);
            }

            if (context.window_definition() != null)
            {
                result.WindowsDefinitions = new List<WindowsDefinitionNode>();
                foreach (var wd in context.window_definition())
                    result.WindowsDefinitions.Add(Visit(wd) as WindowsDefinitionNode);
            }

            if (context.INTERSECT() != null)
                result.Intersect = true;
            else if (context.UNION() != null)
                result.Union = true;
            else if (context.EXCEPT() != null)
                result.Except = true;

            if (context.after_ops() != null)
            {
                result.AfterOps = new List<AfterOpsNode>();
                foreach (var ao in context.after_ops())
                    result.AfterOps.Add(Visit(ao) as AfterOpsNode);
            }
            return result;
        }

        public override ASTNode VisitPointer([NotNull] PlPgSqlParser.PointerContext context)
        {
            return base.VisitPointer(context);
        }

        public override ASTNode VisitPrecision_param([NotNull] PlPgSqlParser.Precision_paramContext context)
        {
            return base.VisitPrecision_param(context);
        }

        //ok
        public override ASTNode VisitPredefined_type([NotNull] PlPgSqlParser.Predefined_typeContext context)
        {
            if (context.BIGINT() != null)
                return new BigintTypeNode(context.BIGINT().Symbol.Line, context.BIGINT().Symbol.Column);
            else if (context.BIT() != null && context.VARYING() == null)
                return new BitTypeNode(context.BIT().Symbol.Line, context.BIT().Symbol.Column);
            else if (context.BIT() != null && context.VARYING() != null)
                return new BitVaryingTypeNode(context.BIT().Symbol.Line, context.BIT().Symbol.Column)
                {
                    Length = (context.type_length() != null) ? int.Parse(context.type_length().NUMBER_LITERAL().GetText()) : 0
                };
            else if (context.BOOLEAN() != null)
                return new BooleanTypeNode(context.BOOLEAN().Symbol.Line, context.BOOLEAN().Symbol.Column);
            else if (context.DEC() != null)
                return new DecTypeNode(context.DEC().Symbol.Line, context.DEC().Symbol.Column)
                {
                    Precision = (context.precision_param() != null) ?
                        int.Parse(context.precision_param().precision.Text) : 0,
                    Scale = (context.precision_param() != null && context.precision_param().scale != null) ?
                        int.Parse(context.precision_param().scale.Text) : 0,
                };
            else if (context.DECIMAL() != null)
                return new DecimalTypeNode(context.DECIMAL().Symbol.Line, context.DECIMAL().Symbol.Column)
                {
                    Precision = (context.precision_param() != null) ?
                        int.Parse(context.precision_param().precision.Text) : 0,
                    Scale = (context.precision_param() != null && context.precision_param().scale != null) ?
                        int.Parse(context.precision_param().scale.Text) : 0,
                };
            else if (context.DOUBLE() != null)
                return new DoublePrecisionTypeNode(context.DOUBLE().Symbol.Line, context.DOUBLE().Symbol.Column);
            else if (context.FLOAT() != null)
                return new FloatTypeNode(context.FLOAT().Symbol.Line, context.FLOAT().Symbol.Column)
                {
                    Precision = (context.precision_param() != null) ?
                        int.Parse(context.precision_param().precision.Text) : 0,
                    Scale = (context.precision_param() != null && context.precision_param().scale != null) ?
                        int.Parse(context.precision_param().scale.Text) : 0,
                };
            else if (context.INT() != null)
                return new IntTypeNode(context.INT().Symbol.Line, context.INT().Symbol.Column);
            else if (context.INTEGER() != null)
                return new IntegerTypeNode(context.INTEGER().Symbol.Line, context.INTEGER().Symbol.Column);
            else if (context.INTERVAL() != null)
                return new IntervalTypeNode(context.INTERVAL().Symbol.Line, context.INTERVAL().Symbol.Column)
                {
                    Interval = (context.interval_field() != null) ?
                        Visit(context.interval_field()) as IntervalFieldNode : null,
                    Lenght = (context.type_length() != null) ? int.Parse(context.type_length().NUMBER_LITERAL().GetText()) : 0
                };
            else if (context.CHAR() != null || context.CHARACTER() != null && context.VARYING() == null)
                return new CharTypeNode(context.Start.Line, context.Start.Column);
            else if (context.CHAR() != null || context.CHARACTER() != null && context.VARYING() != null)
                return new CharVaryingTypeNode(context.Start.Line, context.Start.Column)
                {
                    Length = (context.type_length() != null) ? int.Parse(context.type_length().NUMBER_LITERAL().GetText()) : 0
                };
            else if (context.NCHAR() != null && context.VARYING() == null)
                return new NCharTypeNode(context.Start.Line, context.Start.Column);
            else if (context.NCHAR() != null && context.VARYING() != null)
                return new NCharVaryingTypeNode(context.Start.Line, context.Start.Column)
                {
                    Length = (context.type_length() != null) ? int.Parse(context.type_length().NUMBER_LITERAL().GetText()) : 0
                };
            else if (context.NUMERIC() != null)
                return new NumericTypeNode(context.NUMERIC().Symbol.Line, context.NUMERIC().Symbol.Column)
                {
                    Precision = (context.precision_param() != null) ?
                        int.Parse(context.precision_param().precision.Text) : 0,
                    Scale = (context.precision_param() != null && context.precision_param().scale != null) ?
                        int.Parse(context.precision_param().scale.Text) : 0,
                };
            else if (context.REAL() != null)
                return new RealTypeNode(context.REAL().Symbol.Line, context.REAL().Symbol.Column);
            else if (context.SMALLINT() != null)
                return new SmallintTypeNode(context.SMALLINT().Symbol.Line, context.SMALLINT().Symbol.Column);
            else if (context.TIME() != null && context.TIME().Length > 0)
                return new TimeTypeNode(context.TIME(0).Symbol.Line, context.TIME(0).Symbol.Column)
                {
                    Length = (context.type_length() != null) ? int.Parse(context.type_length().NUMBER_LITERAL().GetText()) : 0
                };
            else if (context.TIMESTAMP() != null)
                return new TimeTypeNode(context.TIMESTAMP().Symbol.Line, context.TIMESTAMP().Symbol.Column)
                {
                    Length = (context.type_length() != null) ? int.Parse(context.type_length().NUMBER_LITERAL().GetText()) : 0
                };
            else if (context.VARCHAR() != null)
                return new VarcharTypeNode(context.VARCHAR().Symbol.Line, context.VARCHAR().Symbol.Column)
                {
                    Length = (context.type_length() != null) ? int.Parse(context.type_length().NUMBER_LITERAL().GetText()) : 0
                };
            else
            {
                var result = new OtherTypeNode(context.Start.Line, context.Start.Column)
                {
                    SchemaQualifiednameNonType = Visit(context.schema_qualified_name_nontype())
                        as SchemaQualifiednameNonTypeNode,
                    Expressions = (context.vex() != null)? new List<ExpressionNode>() : null
                };
                if ((context.vex() != null))
                    foreach (var expr in context.vex())
                        result.Expressions.Add(Visit(expr) as ExpressionNode);
                return result;
            }
        }

        public override ASTNode VisitRaise_param([NotNull] PlPgSqlParser.Raise_paramContext context)
        {
            return base.VisitRaise_param(context);
        }

        public override ASTNode VisitRaise_using([NotNull] PlPgSqlParser.Raise_usingContext context)
        {
            return base.VisitRaise_using(context);
        }

        //ok
        public override ASTNode VisitReturn_stmt([NotNull] PlPgSqlParser.Return_stmtContext context)
        {
            var result = new ReturnStmtNode(context.Start.Line, context.Start.Column);
            if (context.vex() != null)
                result.Expression = Visit(context.vex()) as ExpressionNode;
            else if (context.execute_stmt() != null)
                result.Stmt = Visit(context.execute_stmt()) as ExecuteStmtNode;
            else if (context.select_stmt() != null)
                result.Stmt = Visit(context.select_stmt()) as SelectStatementNode;
            else if (context.perform_stmt() != null)
                result.Stmt = Visit(context.perform_stmt()) as PerformStmtNode;
            else if (context.show_statement() != null)
                result.Stmt = Visit(context.show_statement()) as ShowStmtNode;
            else if (context.explain_statement() != null)
                result.Stmt = Visit(context.explain_statement()) as ExplainStmtNode;
            return result;
        }

        //ok
        public override ASTNode VisitSchema_qualified_name([NotNull] PlPgSqlParser.Schema_qualified_nameContext context)
        {
            var result = new SchemaQualifieldNode(context.Start.Line, context.Start.Column)
            {
                Identifiers = new List<IdNode>()
            };
            foreach (var id in context.identifier())
                result.Identifiers.Add(Visit(id) as IdNode);
            return result;
        }

        //ok
        public override ASTNode VisitSchema_qualified_name_nontype([NotNull] PlPgSqlParser.Schema_qualified_name_nontypeContext context)
        {
            var id = Visit(context.identifier_nontype());
            var identifierNonType = (id is IdentifierNonTypeNode)? id as IdentifierNonTypeNode :
                new IdentifierNonTypeNode(id.Line, id.Column, (id as IdNode).Text);
            return new SchemaQualifiednameNonTypeNode(context.Start.Line, context.Start.Column)
            {
                Schema = (context.schema != null)? Visit(context.schema) as IdNode :  null,
                IdentifierNonType = identifierNonType
            };
        }
        //ok
        public override ASTNode VisitSelect_list([NotNull] PlPgSqlParser.Select_listContext context)
        {
            var result = new SelectListNode(context.Start.Line, context.Start.Column)
            {
                SelectSubLists = new List<SelectSubListNode>()
            };
            foreach (var item in context.select_sublist())
            {
                result.SelectSubLists.Add(Visit(item) as SelectSubListNode);
            }
            return result;
        }
        
        //ok
        public override ASTNode VisitSelect_ops([NotNull] PlPgSqlParser.Select_opsContext context)
        {
            var result = new SelectOpsNode(context.Start.Line, context.Start.Column);
            if (context.select_stmt() != null)
                result.SelectStmt = Visit(context.select_stmt()) as SelectStatementNode;
            else if (context.select_ops() != null && context.select_ops().Length > 0)
            {
                result.SelectOps = new List<SelectOpsNode>();
                foreach (var sops in context.select_ops())
                    result.SelectOps.Add(Visit(sops) as SelectOpsNode);
                if (context.set_qualifier() != null)
                    result.SetQualifier = context.set_qualifier().GetText();
                result.Intersect = context.INTERSECT() != null;
                result.Union = context.UNION() != null;
                result.Except = context.EXCEPT() != null;
            }
            else if (context.select_primary() != null)
                result.SelectPrimary = Visit(context.select_primary()) as SelectPrimaryNode;
            return result;
        }

        //ok
        public override ASTNode VisitSelect_ops_no_parens([NotNull] PlPgSqlParser.Select_ops_no_parensContext context)
        {
            return new SelectOpsNoParensNode(context.Start.Line, context.Start.Column)
            {
                SelectOps = (context.select_ops() != null)? Visit(context.select_ops()) as SelectOpsNode :  null,
                Qualifier = (context.set_qualifier() != null)? context.set_qualifier().GetText() : null,
                Intersect = context.INTERSECT() != null,
                Union = context.UNION() != null,
                Except = context.EXCEPT() != null,
                SelectPrimary = (context.select_primary() != null)?
                    Visit(context.select_primary()) as SelectPrimaryNode : null,
                SelectStmt = (context.select_stmt() != null)? Visit(context.select_stmt()) as SelectStatementNode : null
            };
        }

        //ok
        public override ASTNode VisitSelect_primary([NotNull] PlPgSqlParser.Select_primaryContext context)
        {
            var result = new SelectPrimaryNode(context.Start.Line, context.Start.Column);
            if (context.values_stmt() != null)
                result = Visit(context.values_stmt()) as ValuesStmtNode;
            else if (context.schema_qualified_name() != null)
                result.SchemaQualifield = Visit(context.schema_qualified_name()) as SchemaQualifieldNode;
            else if (context.SELECT() != null)
            {
                if (context.set_qualifier() != null)
                {
                    result.SetQualifier = context.set_qualifier().GetText();
                    if(context.vex() != null)
                    {
                        result.Expressions = new List<ExpressionNode>();
                        foreach (var expr in context.vex())
                            result.Expressions.Add(Visit(expr) as ExpressionNode);
                    }
                }
                if (context.select_list() != null)
                {
                    result.SelectList = Visit(context.select_list()) as SelectListNode;
                }
                if (context.into_table() != null)
                {
                    result.IntoTable = new List<SchemaQualifieldNode>();
                    foreach (var item in context.into_table().schema_qualified_name())
                    {
                        result.IntoTable.Add(Visit(item) as SchemaQualifieldNode);
                    }
                }
                if (context.from_item() != null && context.from_item().Length > 0)
                {
                    result.FromItems = new List<FromItemNode>();
                    foreach (var fi in context.from_item())
                        result.FromItems.Add(Visit(fi) as FromItemNode);
                }
                if (context.WHERE() != null)
                {
                    result.Where = true;
                    if (result.Expressions == null)
                    {
                        result.Expressions = new List<ExpressionNode>();
                        foreach (var expr in context.vex())
                            result.Expressions.Add(Visit(expr) as ExpressionNode);
                    }
                }
                if (context.groupby_clause() != null)
                    result.GroupByClause = Visit(context.groupby_clause()) as GroupByClauseNode;
                if (context.HAVING() != null)
                {
                    result.Having = true;
                    if (result.Expressions == null)
                    {
                        result.Expressions = new List<ExpressionNode>();
                        foreach (var expr in context.vex())
                            result.Expressions.Add(Visit(expr) as ExpressionNode);
                    }
                }
                if (context.identifier() != null)
                {
                    result.Identifiers = new List<IdNode>();
                    foreach (var id in context.identifier())
                        result.Identifiers.Add(Visit(id) as IdNode);
                    result.WindowsDefinitions = new List<WindowsDefinitionNode>();
                    foreach (var wd in context.window_definition())
                        result.WindowsDefinitions.Add(Visit(wd) as WindowsDefinitionNode);
                }
                
            }
            return result;
        }

        //ok
        public override ASTNode VisitSelect_stmt([NotNull] PlPgSqlParser.Select_stmtContext context)
        {
            var result = new SelectStatementNode(context.Start.Line, context.Start.Column)
            {
                WithClause = (context.with_clause() != null) ? Visit(context.with_clause()) as WithClauseNode : null,
                SelectOps = Visit(context.select_ops()) as SelectOpsNode,
                AfterOps = new List<AfterOpsNode>()
            };
            if(context.after_ops() != null)
                foreach (var ao in context.after_ops())
                    result.AfterOps.Add(Visit(ao) as AfterOpsNode);
            return result;
        }

        //ok
        public override ASTNode VisitSelect_stmt_no_parens([NotNull] PlPgSqlParser.Select_stmt_no_parensContext context)
        {
            var result = new SelectStmtNonParensNode(context.Start.Line, context.Start.Column)
            {
                WithClause = (context.with_clause() != null)? 
                    Visit(context.with_clause()) as WithClauseNode : null,
                SelectOps = Visit(context.select_ops_no_parens()) as SelectOpsNoParensNode,
                AfterOps = new List<AfterOpsNode>()
            };

            if (context.after_ops() != null)
                foreach (var ops in context.after_ops())
                    result.AfterOps.Add(Visit(ops) as AfterOpsNode);
            return result;
        }

        //ok
        public override ASTNode VisitSelect_sublist([NotNull] PlPgSqlParser.Select_sublistContext context)
        {
            var result = new SelectSubListNode(context.Start.Line, context.Start.Column)
            {
                Expression = Visit(context.vex()) as ExpressionNode
            };
            if (context.col_label() != null)
                result.Identifier = Visit(context.col_label()) as IdNode;
            else if(context.id_token() != null)
                result.Identifier = Visit(context.id_token()) as IdNode;
            return result;
        }

        //ok
        public override ASTNode VisitSet_qualifier([NotNull] PlPgSqlParser.Set_qualifierContext context)
        {
            return base.VisitSet_qualifier(context);
        }

        //ok
        public override ASTNode VisitShow_statement([NotNull] PlPgSqlParser.Show_statementContext context)
        {
            var result = new ShowStmtNode(context.Start.Line, context.Start.Column)
            {
                All = context.ALL() != null,
                TimeZone = context.TIME() != null, 
                TransactionIsilationLevel = context.TRANSACTION() != null, 
                SessionAuthorization = context.SESSION() != null
            };
            if (context.identifier() != null)
            {
                if (context.identifier().Length == 2)
                {
                    result.PrimaryIdentifier = Visit(context.identifier(0)) as IdNode;
                    result.SecundaryIdentifier = Visit(context.identifier(1)) as IdNode;
                }
                else
                    result.SecundaryIdentifier = Visit(context.identifier(0)) as IdNode;
            }
            return result;
        }

        public override ASTNode VisitSign([NotNull] PlPgSqlParser.SignContext context)
        {
            return base.VisitSign(context);
        }

        public override ASTNode VisitSigned_number_literal([NotNull] PlPgSqlParser.Signed_number_literalContext context)
        {
            return base.VisitSigned_number_literal(context);
        }

        //ok
        public override ASTNode VisitSort_specifier([NotNull] PlPgSqlParser.Sort_specifierContext context)
        {
            return new SortSpecifierNode(context.Start.Line, context.Start.Column)
            {
                Key = Visit(context.key) as ExpressionNode,
                OpClass = (context.opclass != null)? Visit(context.opclass) as SchemaQualifieldNode : null,
                OrderSpecification = (context.order != null)? Visit(context.order) as OrderSpecificationNode : null,
                NullOrdering = (context.null_order != null)? Visit(context.null_order) as NullOrderingNode : null
            };
        }

        public override ASTNode VisitSort_specifier_list([NotNull] PlPgSqlParser.Sort_specifier_listContext context)
        {
            return base.VisitSort_specifier_list(context);
        }
       
        //ok
        public override ASTNode VisitStart_label([NotNull] PlPgSqlParser.Start_labelContext context)
        {
            return base.VisitStart_label(context);
        }
       
        //ok
        public override ASTNode VisitStorage_parameter([NotNull] PlPgSqlParser.Storage_parameterContext context)
        {
            return base.VisitStorage_parameter(context);
        }
       
        //ok
        public override ASTNode VisitStorage_parameter_oid([NotNull] PlPgSqlParser.Storage_parameter_oidContext context)
        {
            return base.VisitStorage_parameter_oid(context);
        }

        public override ASTNode VisitStorage_parameter_option([NotNull] PlPgSqlParser.Storage_parameter_optionContext context)
        {
            return base.VisitStorage_parameter_option(context);
        }
        
        //ok
        public override ASTNode VisitString_value_function([NotNull] PlPgSqlParser.String_value_functionContext context)
        {
            if (context.TRIM() != null)
            {
                return new TrimStringValueFunctionNode(context.Start.Line, context.Start.Column)
                {
                    Chars = Visit(context.chars) as ExpressionNode,
                    Str = Visit(context.str) as ExpressionNode
                };
            }
            else if (context.SUBSTRING() != null)
            {
                var result = new SubstringStringValueFunctionNode(context.Start.Line, context.Start.Column)
                {
                    Expressions = new List<ExpressionNode>(),
                    For = context.FOR() != null,
                    From = context.FROM() != null
                };
                foreach (var expr in context.vex())
                    result.Expressions.Add(Visit(expr) as ExpressionNode);
                return result;
            }
            else if (context.POSITION() != null)
            {
                return new PositionStringValueFunctionNode(context.Start.Line, context.Start.Column)
                {
                    ExpressionB = Visit(context.vex_b()) as ExpressionNode,
                    Expression = Visit(context.vex(0)) as ExpressionNode,
                };
            }
            else if (context.OVERLAY() != null)
            {
                var result = new OverlayStringValueFunctionNode(context.Start.Line, context.Start.Column)
                {
                    Expressions = new List<ExpressionNode>(),
                    For = context.FOR() != null
                };
                foreach (var expr in context.vex())
                    result.Expressions.Add(Visit(expr) as ExpressionNode);
                return result;
            }
            else
            {
                return new CollationStringValueFunctionNode(context.Start.Line, context.Start.Column)
                {
                    Expression = Visit(context.vex(0)) as ExpressionNode
                };
            }
        }
        
        //ok
        public override ASTNode VisitSystem_function([NotNull] PlPgSqlParser.System_functionContext context)
        {
            if (context.CURRENT_CATALOG() != null)
                return new CurrentCatalogSystemFunctionNode(context.Start.Line, context.Start.Column);
            else if (context.CURRENT_SCHEMA() != null)
                return new CurrentSchemaSystemFunctionNode(context.Start.Line, context.Start.Column);
            else if (context.CURRENT_USER() != null)
                return new CurrentUserSystemFunctionNode(context.Start.Line, context.Start.Column);
            else if (context.USER() != null)
                return new UserSystemFunctionNode(context.Start.Line, context.Start.Column);
            else if (context.SESSION_USER() != null)
                return new SessionUserSystemFunctionNode(context.Start.Line, context.Start.Column);
            else
            {
                return new CastSpesificationSystemFunction(context.Start.Line, context.Start.Column)
                {
                    Expression = Visit(context.cast_specification().vex()) as ExpressionNode,
                    DataType = Visit(context.cast_specification().data_type()) as DataTypeNode
                };
            }
        }

        //ok
        public override ASTNode VisitTable_cols([NotNull] PlPgSqlParser.Table_colsContext context)
        {
            var result = new TableColsNode(context.Start.Line, context.Start.Column)
            {
                SchemaQualifield = Visit(context.schema_qualified_name()) as SchemaQualifieldNode,
                Identifiers = new List<IdNode>()
            };
            foreach (var id in context.identifier())
            {
                result.Identifiers.Add(Visit(id) as IdNode);
            }
            return result;
        }

        public override ASTNode VisitTable_cols_list([NotNull] PlPgSqlParser.Table_cols_listContext context)
        {
            return base.VisitTable_cols_list(context);
        }

        public override ASTNode VisitTable_space([NotNull] PlPgSqlParser.Table_spaceContext context)
        {
            return base.VisitTable_space(context);
        }

        public override ASTNode VisitTable_subquery([NotNull] PlPgSqlParser.Table_subqueryContext context)
        {
            return base.VisitTable_subquery(context);
        }

        public override ASTNode VisitTokens_nonkeyword([NotNull] PlPgSqlParser.Tokens_nonkeywordContext context)
        {
            return base.VisitTokens_nonkeyword(context);
        }

        public override ASTNode VisitTokens_nonreserved([NotNull] PlPgSqlParser.Tokens_nonreservedContext context)
        {
            return base.VisitTokens_nonreserved(context);
        }

        public override ASTNode VisitTokens_nonreserved_except_function_type([NotNull] PlPgSqlParser.Tokens_nonreserved_except_function_typeContext context)
        {
            return base.VisitTokens_nonreserved_except_function_type(context);
        }

        public override ASTNode VisitTokens_reserved([NotNull] PlPgSqlParser.Tokens_reservedContext context)
        {
            return base.VisitTokens_reserved(context);
        }

        public override ASTNode VisitTokens_reserved_except_function_type([NotNull] PlPgSqlParser.Tokens_reserved_except_function_typeContext context)
        {
            return base.VisitTokens_reserved_except_function_type(context);
        }

        //ok
        public override ASTNode VisitTransaction_statement([NotNull] PlPgSqlParser.Transaction_statementContext context)
        {
            var result = new TransactionStatementNode(context.Start.Line, context.Start.Column)
            {
                MultiplyTables = new List<SchemaQualifieldNode>()
            };
            if (context.lock_table() != null)
                foreach (var name in context.lock_table().only_table_multiply())
                    result.MultiplyTables.Add(Visit(name.schema_qualified_name()) as SchemaQualifieldNode);
            return result;
        }

        //ok
        public override ASTNode VisitTruncate_stmt([NotNull] PlPgSqlParser.Truncate_stmtContext context)
        {
            var result = new TruncateStmtNode(context.Start.Line, context.Start.Column)
            {
                SchemaQualifields = new List<SchemaQualifieldNode>(),
                Cascade = (context.cascade_restrict() != null)? context.cascade_restrict().CASCADE() != null : false,
                Restrict = (context.cascade_restrict() != null) ? context.cascade_restrict().RESTRICT() != null : false,
            };
            foreach (var item in context.only_table_multiply())
                result.SchemaQualifields.Add(Visit(item.schema_qualified_name()) as SchemaQualifieldNode);
            return result;
        }

        public override ASTNode VisitTruth_value([NotNull] PlPgSqlParser.Truth_valueContext context)
        {
            return new BoolNode(context.Start.Line, context.Start.Column, context.TRUE() != null);

        }
        //ok
        public override ASTNode VisitType_coercion([NotNull] PlPgSqlParser.Type_coercionContext context)
        {
            if (context.data_type() != null)
                return new BaseTypeCoercionNode(context.Start.Line, context.Start.Column)
                {
                    DataType = Visit(context.data_type()) as DataTypeNode,
                    Id = context.character_string().GetText()
                };
            else
            {
                return new IntervalTypeCoercionNode(context.Start.Line, context.Start.Column)
                {
                    Id = context.character_string().GetText(),
                    IntervalField = Visit(context.interval_field()) as IntervalFieldNode,
                    Length = (context.type_length() != null)? int.Parse(context.type_length().NUMBER_LITERAL().GetText()) : 0,
                };
            }
        }

        //ok
        public override ASTNode VisitType_declaration([NotNull] PlPgSqlParser.Type_declarationContext context)
        {
            if (context.data_type_dec() != null)
                return new OrdinalTypeDeclarationNode(context.Start.Line, context.Start.Column)
                {
                    DataType = Visit(context.data_type_dec()) as DataTypeDeclarationNode,
                    CollateIdentifier = (context.collate_identifier() != null) ?
                        Visit(context.collate_identifier().collation) as SchemaQualifieldNode : null,
                    Expression = (context.vex() != null) ? Visit(context.vex()) as ExpressionNode : null
                };
            else if (context.ALIAS() != null)
                return new AliasDeclarationNode(context.Start.Line, context.Start.Column)
                {
                    Identifier = (context.identifier() != null) ? Visit(context.identifier()) as IdNode :
                        new DollarNumberNode(context.DOLLAR_NUMBER().Symbol.Line,
                            context.DOLLAR_NUMBER().Symbol.Column, context.DOLLAR_NUMBER().GetText())
                };
            else
            {
                var cursorDeclaration = new CursorDeclarationNode(context.Start.Line, context.Start.Column)
                {
                    ArgumentList = new List<ArgumentNode>(),
                    SelectStmt = Visit(context.select_stmt()) as SelectStatementNode
                };
                if (context.arguments_list() != null)
                {
                    var identifiers = (from id in context.arguments_list().identifier() select Visit(id) as IdNode).ToList();
                    var types = (from type in context.arguments_list().data_type() select Visit(type) as DataTypeNode).ToList();
                    for (int i = 0; i < identifiers.Count; i++)
                    {
                        cursorDeclaration.ArgumentList.Add(
                            new ArgumentNode(identifiers[i].Line, identifiers[i].Column)
                            {
                                Identifier = identifiers[i],
                                DataType = types[i]
                            });
                    }
                }
                return cursorDeclaration;
            }
        }
        
        //ok
        public override ASTNode VisitType_length([NotNull] PlPgSqlParser.Type_lengthContext context)
        {
            return base.VisitType_length(context);
        }

        //ok
        public override ASTNode VisitType_list([NotNull] PlPgSqlParser.Type_listContext context)
        {
            return base.VisitType_list(context);
        }
        
        //ok
        public override ASTNode VisitUnsigned_numeric_literal([NotNull] PlPgSqlParser.Unsigned_numeric_literalContext context)
        {
            if (context.NUMBER_LITERAL() != null)
            {
                var stringValue = context.NUMBER_LITERAL().GetText();
                int intValue;
                long longValue;
                if (int.TryParse(stringValue, out intValue))
                {
                    return new IntNode(context.Start.Line, context.Start.Column, intValue);
                }
                else if (long.TryParse(stringValue, out longValue))
                {
                    return new Int8Node(context.Start.Line, context.Start.Column, longValue);
                }
            }
            else
            {
                var stringValue = context.REAL_NUMBER().GetText();
                float floatValue;
                double doubleValue;
                if (float.TryParse(stringValue, out floatValue))
                    return new Float4Node(context.Start.Line, context.Start.Column, floatValue);
                else if(double.TryParse(stringValue, out doubleValue))
                    return new Float8Node(context.Start.Line, context.Start.Column, doubleValue);
            }
            return null;
        }

        //ok
        public override ASTNode VisitUnsigned_value_specification([NotNull] PlPgSqlParser.Unsigned_value_specificationContext context)
        {
            if (context.unsigned_numeric_literal() != null)
                return Visit(context.unsigned_numeric_literal());
            else if (context.character_string() != null)
                return Visit(context.character_string());
            else
                return Visit(context.truth_value());
        }

        //ok
        public override ASTNode VisitUpdate_set([NotNull] PlPgSqlParser.Update_setContext context)
        {
            var result = new UpdateSetNode(context.Start.Line, context.Start.Column)
            {
                TableSubquery = (context.table_subquery() != null)? Visit(context.table_subquery().select_stmt()) 
                        as SelectStatementNode : null,
                Columns = (context._column != null)? new List<IndirectionIdentifierNode>() : null,
                Values = (context._value != null)? new List<ExpressionNode>() : null
            };
            if (context._column != null)
                foreach (var ii in context._column)
                    result.Columns.Add(Visit(ii) as IndirectionIdentifierNode);

            if (context._value != null)
                foreach (var expr in context._value)
                    result.Values.Add(Visit(expr) as ExpressionNode);

            return result;
        }

        //ok
        public override ASTNode VisitUpdate_stmt_for_psql([NotNull] PlPgSqlParser.Update_stmt_for_psqlContext context)
        {
            var result = new UpdateStmtPSqlNode(context.Start.Line, context.Start.Column)
            {
                WithClause = (context.with_clause() != null)? Visit(context.with_clause())  as WithClauseNode : null,
                UpdateTableName = Visit(context.update_table_name) as SchemaQualifieldNode,
                Alias = (context.alias != null)? Visit(context.alias) as IdNode : null,
                UpdateSets = new List<UpdateSetNode>(),
                FromItems = (context.from_item() != null)? new List<FromItemNode>() : null,
                Expression = (context.vex() != null)? Visit(context.vex()) as ExpressionNode: null,
                Cursor = (context.cursor != null)? Visit(context.cursor) as IdNode :  null,
                SelectList = (context.select_list() != null)? Visit(context.select_list()) as SelectListNode :  null
            };
            if (context.update_set() != null)
                foreach (var us in context.update_set())
                    result.UpdateSets.Add(Visit(us) as UpdateSetNode);
            if (context.from_item() != null)
                foreach (var fi in context.from_item())
                    result.FromItems.Add(Visit(fi) as FromItemNode);
            return result;
        }

        public override ASTNode VisitUser_name([NotNull] PlPgSqlParser.User_nameContext context)
        {
            return base.VisitUser_name(context);
        }
        
        //ok
        public override ASTNode VisitUsing_vex([NotNull] PlPgSqlParser.Using_vexContext context)
        {
            return base.VisitUsing_vex(context);
        }

        //ok
        public override ASTNode VisitValues_stmt([NotNull] PlPgSqlParser.Values_stmtContext context)
        {
            var result = new ValuesStmtNode(context.Start.Line, context.Start.Column)
            {
                Values = new List<ValuesValuesNode>()
            };
            foreach (var vv in context.values_values())
            {
                var _vv = Visit(vv) as ValuesValuesNode;
                result.Values.Add(_vv);
            }

            return result;
        }
        //ok
        public override ASTNode VisitValues_values([NotNull] PlPgSqlParser.Values_valuesContext context)
        {
            var result = new ValuesValuesNode(context.Start.Line, context.Start.Column)
            {
                Expressions = new List<ExpressionNode>()
            };
            foreach (var expr in context.vex())
            {
                result.Expressions.Add(Visit(expr) as ExpressionNode);
            }
            foreach (var d in context.DEFAULT())
            {
                result.Expressions.Add(new DefaultNode(d.Symbol.Line, d.Symbol.Column));
            }
            return result;
        }

        //ok
        public override ASTNode VisitValue_expression_primary([NotNull] PlPgSqlParser.Value_expression_primaryContext context)
        {
            if (context.unsigned_value_specification() != null)
                return Visit(context.unsigned_value_specification());
            else if (context.case_expression() != null)
                return Visit(context.case_expression());
            else if (context.function_call() != null)
                return Visit(context.function_call());
            else if (context.comparison_mod() != null)
                return Visit(context.comparison_mod());
            else if (context.indirection_var() != null)
                return Visit(context.indirection_var());
            else if (context.type_coercion() != null)
                return Visit(context.type_coercion());
            else if (context.datetime_overlaps() != null)
                return Visit(context.datetime_overlaps());
            else if (context.array_expression() != null)
                return Visit(context.array_expression());
            else if (context.EXISTS() != null)
                return new ExistsNode(context.Start.Line, context.Start.Column)
                {
                    SelectStmt = Visit(context.table_subquery().select_stmt()) as SelectStatementNode
                };
            else if (context.select_stmt_no_parens() != null)
            {
                var result = new ValueExpressionPrimaryNode(context.Start.Line, context.Start.Column)
                {
                    SelectStmtNonParens = Visit(context.select_stmt_no_parens()) as SelectStmtNonParensNode,
                    Indirections = (context.indirection_list() != null) ? new List<IndirectionNode>() : null
                };
                if (context.indirection_list() != null)
                    foreach (var item in context.indirection_list().indirection())
                    {
                        result.Indirections.Add(Visit(item) as IndirectionNode);
                    }
                return result;
            }
            else if (context.NULL() != null)
            {
                return new NullNode(context.Start.Line, context.Start.Column);
            }
            else if (context.MULTIPLY() != null)
                return new MultiplyNode(context.Start.Line, context.Start.Column);
            else return null;
        }
        //ok
        public override ASTNode VisitVar([NotNull] PlPgSqlParser.VarContext context)
        {
            var result = new VarNode(context.Start.Line, context.Start.Column)
            {
                Expressions = (context.vex() != null) ? new List<ExpressionNode>() : null,
                Id = (context.DOLLAR_NUMBER() != null) ? 
                    new DollarNumberNode (context.DOLLAR_NUMBER().Symbol.Line, context.DOLLAR_NUMBER().Symbol.Column, context.DOLLAR_NUMBER().GetText()) : null,
                SchemaQualifield = (context.schema_qualified_name() != null)? 
                    Visit(context.schema_qualified_name()) as SchemaQualifieldNode : null
            };
            if (context.vex() != null)
            {
                foreach (var exp in context.vex())
                {
                    result.Expressions.Add(Visit(exp) as ExpressionNode);
                }
            }
            return result;
        }

        //ok
        public override ASTNode VisitVex([NotNull] PlPgSqlParser.VexContext context)
        {
            if (context.CAST_EXPRESSION() != null)
                return new CastExpressionNode(context.Start.Line, context.Start.Column)
                {
                    Expression = Visit(context.vex(0)) as ExpressionNode,
                    DataType = Visit(context.data_type()) as DataTypeNode
                };
            else if (context.AND() != null && context.BETWEEN() == null)
            {
                return new AndNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex(1)) as ExpressionNode
                };
            }
            else if (context.OR() != null)
            {
                return new OrNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex(1)) as ExpressionNode
                };
            }
            else if (context.ISNULL() != null)
            {
                return new IsNullNode(context.Start.Line, context.Start.Column)
                {
                    Operand = Visit(context.vex(0)) as ExpressionNode
                };
            }
            else if (context.NOTNULL() != null)
            {
                return new IsNotNullNode(context.Start.Line, context.Start.Column)
                {
                    Operand = Visit(context.vex(0)) as ExpressionNode
                };
            }
            else if (context.LTH() != null)
            {
                return new LessNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex(1)) as ExpressionNode
                };
            }
            else if (context.GTH() != null)
            {
                return new GreaterNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex(1)) as ExpressionNode
                };
            }
            else if (context.LEQ() != null)
            {
                return new LessEqualNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex(1)) as ExpressionNode
                };
            }
            else if (context.GEQ() != null)
            {
                return new GreaterEqualNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex(1)) as ExpressionNode
                };
            }
            else if (context.EQUAL() != null)
            {
                return new EqualNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex(1)) as ExpressionNode
                };
            }
            else if (context.NOT_EQUAL() != null)
            {
                return new NotEqualNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex(1)) as ExpressionNode
                };
            }
            else if (context.BETWEEN() != null)
            {
                return new BetweenNode(context.Start.Line, context.Start.Column)
                {
                    FirstOperand = Visit(context.vex(0)) as ExpressionNode,
                    SecondOperand = Visit(context.vex_b()) as ExpressionNode,
                    ThirdOperand = Visit(context.vex(1)) as ExpressionNode,
                    Not = context.NOT() != null,
                    Asymmetric = context.ASYMMETRIC() != null,
                    Symmetric = context.SYMMETRIC() != null
                };
            }
            else if (context.MULTIPLY() != null)
            {
                return new MultiplicationNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex(1)) as ExpressionNode
                };
            }
            else if (context.DIVIDE() != null)
            {
                return new DivisionNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex(1)) as ExpressionNode
                };
            }
            else if (context.MODULAR() != null)
            {
                return new ModuloNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex(1)) as ExpressionNode
                };
            }
            else if (context.PLUS() != null && context.vex().Length == 1)
            {
                return new AdditionNode(context.Start.Line, context.Start.Column)
                {
                    RightOperand = Visit(context.vex(0)) as ExpressionNode
                };
            }
            else if (context.MINUS() != null && context.vex().Length == 1)
            {
                return new SubtractionNode(context.Start.Line, context.Start.Column)
                {
                    RightOperand = Visit(context.vex(0)) as ExpressionNode
                };
            }
            else if (context.PLUS() != null)
            {
                return new AdditionNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex(1)) as ExpressionNode
                };
            }
            else if (context.MINUS() != null)
            {
                return new SubtractionNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex(1)) as ExpressionNode
                };
            }
            else if (context.EXP() != null)
            {
                return new ExponentiationNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex(1)) as ExpressionNode
                };
            }
            else if (context.DISTINCT() != null)
            {
                if (context.NOT() != null)
                    return new IsDistinctFromNode(context.Start.Line, context.Start.Column)
                    {
                        LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                        RightOperand = Visit(context.vex(1)) as ExpressionNode
                    };
                else
                    return new IsNotDistinctFromNode(context.Start.Line, context.Start.Column)
                    {
                        LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                        RightOperand = Visit(context.vex(1)) as ExpressionNode
                    };
            }
            else if (context.IN() != null)
            {
                var result = new InNode(context.Start.Line, context.Start.Column)
                {
                    Expressions = new List<ExpressionNode>(),
                    Not = context.NOT() != null,
                    SelectStmtNonParens = (context.select_stmt_no_parens() != null) ?
                    Visit(context.select_stmt_no_parens()) as SelectStmtNonParensNode : null
                };
                foreach (var exp in context.vex())
                    result.Expressions.Add(Visit(exp) as ExpressionNode);
                return result;
            }
            else if (context.OF() != null)
            {
                var result = new OfNode(context.Start.Line, context.Start.Column)
                {
                    Expression = Visit(context.vex(0)) as ExpressionNode,
                    Not = context.NOT() != null,
                    DataTypes = new List<DataTypeNode>()
                };
                foreach (var dt in context.type_list().data_type())
                    result.DataTypes.Add(Visit(dt) as DataTypeNode);
                return result;
            }
            else if (context.op() != null)
            {
                if (context.vex().Length == 2)
                    return new OtherOpBinaryNode(context.Start.Line, context.Start.Column)
                    {
                        Op = Visit(context.op()) as OpNode,
                        LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                        RightOperand = Visit(context.vex(1)) as ExpressionNode
                    };
                else
                    return new OtherOpUnaryNode(context.Start.Line, context.Start.Column)
                    {
                        Op = Visit(context.op()) as OpNode,
                        Operand = Visit(context.vex(0)) as ExpressionNode,
                        FirstOp = context.op().Start.Column < context.vex(0).Start.Column
                    };
            }
            else if (context.UNKNOWN() != null)
            {
                if (context.NOT() != null)
                    return new IsNotUnknownNode(context.Start.Line, context.Start.Column)
                    {
                        Operand = Visit(context.vex(0)) as ExpressionNode
                    };
                else
                    return new IsUnknownNode(context.Start.Line, context.Start.Column)
                    {
                        Operand = Visit(context.vex(0)) as ExpressionNode
                    };

            }
            else if (context.DOCUMENT() != null)
            {
                if (context.NOT() != null)
                    return new IsNotDocumentNode(context.Start.Line, context.Start.Column)
                    {
                        Operand = Visit(context.vex(0)) as ExpressionNode
                    };
                else
                    return new IsDocumentNode(context.Start.Line, context.Start.Column)
                    {
                        Operand = Visit(context.vex(0)) as ExpressionNode
                    };
            }
            else if (context.TIME() != null)
                return new AtTimeZoneNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex(1)) as ExpressionNode
                };
            else if (context.IS() != null && (context.truth_value() != null || context.NULL() != null))
            {
                if (context.NULL() != null)
                {
                    if (context.NOT() != null)
                        return new IsNotNullNode(context.Start.Line, context.Start.Column)
                        {
                            Operand = Visit(context.vex(0)) as ExpressionNode
                        };
                    else
                        return new IsNullNode(context.Start.Line, context.Start.Column)
                        {
                            Operand = Visit(context.vex(0)) as ExpressionNode
                        };
                }
                else
                {
                    if (context.NOT() != null)
                    {
                        if (context.truth_value().TRUE() != null)
                            return new IsNotTrueNode(context.Start.Line, context.Start.Column)
                            {
                                Operand = Visit(context.vex(0)) as ExpressionNode
                            };
                        else
                            return new IsNotFalseNode(context.Start.Line, context.Start.Column)
                            {
                                Operand = Visit(context.vex(0)) as ExpressionNode
                            };
                    }
                    else
                    {
                        if (context.truth_value().TRUE() != null)
                            return new IsTrueNode(context.Start.Line, context.Start.Column)
                            {
                                Operand = Visit(context.vex(0)) as ExpressionNode
                            };
                        else
                            return new IsFalseNode(context.Start.Line, context.Start.Column)
                            {
                                Operand = Visit(context.vex(0)) as ExpressionNode
                            };
                    }
                }
            }
            else if (context.collate_identifier() != null)
            {
                return new CollateNode(context.Start.Line, context.Start.Column)
                {
                    Expression = Visit(context.vex(0)) as ExpressionNode,
                    Collate = Visit(context.collate_identifier().collation) as SchemaQualifieldNode
                };
            }
            else if (context.LIKE() != null)
            {
                if (context.NOT() != null)
                {
                    return new NotLikeBinaryNode(context.Start.Line, context.Start.Column)
                    {
                        Escape = (context.ESCAPE() != null) ? Visit(context.vex(2)) as ExpressionNode : null,
                        LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                        RightOperand = Visit(context.vex(1)) as ExpressionNode
                    };
                }
                else
                {
                    return new LikeBinaryNode(context.Start.Line, context.Start.Column)
                    {
                        Escape = (context.ESCAPE() != null) ? Visit(context.vex(2)) as ExpressionNode : null,
                        LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                        RightOperand = Visit(context.vex(1)) as ExpressionNode
                    };
                }
            }
            else if (context.ILIKE() != null)
            {
                if (context.NOT() != null)
                {
                    return new NotILikeBinaryNode(context.Start.Line, context.Start.Column)
                    {
                        Escape = (context.ESCAPE() != null) ? Visit(context.vex(2)) as ExpressionNode : null,
                        LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                        RightOperand = Visit(context.vex(1)) as ExpressionNode
                    };
                }
                else
                    return new ILikeBinaryNode(context.Start.Line, context.Start.Column)
                    {
                        Escape = (context.ESCAPE() != null) ? Visit(context.vex(2)) as ExpressionNode : null,
                        LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                        RightOperand = Visit(context.vex(1)) as ExpressionNode
                    };
            }
            else if (context.SIMILAR() != null)
            {
                if (context.NOT() != null)
                {
                    return new NotSimilarToBinaryNode(context.Start.Line, context.Start.Column)
                    {
                        Escape = (context.ESCAPE() != null) ? Visit(context.vex(2)) as ExpressionNode : null,
                        LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                        RightOperand = Visit(context.vex(1)) as ExpressionNode
                    };
                }
                else
                    return new SimilarToBinaryNode(context.Start.Line, context.Start.Column)
                    {
                        Escape = (context.ESCAPE() != null) ? Visit(context.vex(2)) as ExpressionNode : null,
                        LeftOperand = Visit(context.vex(0)) as ExpressionNode,
                        RightOperand = Visit(context.vex(1)) as ExpressionNode
                    };
            }
            else if (context.vex() != null && context.vex().Length == 1 && context.NOT() != null)
            {
                return new NotNode(context.Start.Line, context.Start.Column)
                {
                    Operand = Visit(context.vex(0)) as ExpressionNode
                };
            }
            else if (context.vex() != null && context.vex().Length == 1)
            {
                var result = new ExpInDirectionNode(context.Start.Line, context.Start.Column)
                {
                    Expression = Visit(context.vex(0)) as ExpressionNode,
                    Indirections = (context.indirection_list() != null) ? new List<IndirectionNode>() : null
                };
                if (context.indirection_list() != null)
                    foreach (var item in context.indirection_list().indirection())
                    {
                        result.Indirections.Add(Visit(item) as IndirectionNode);
                    }
                return result;
            }
            else if (context.value_expression_primary() != null)
            {
                return Visit(context.value_expression_primary());
            }
            else
            {
                var result = new ExpressionListNode(context.Start.Line, context.Start.Column)
                {
                    Expressions = new List<ExpressionNode>()
                };
                foreach (var exp in context.vex())
                {
                    result.Expressions.Add(Visit(exp) as ExpressionNode);
                }
                return result;
            }
        }

        //ok
        public override ASTNode VisitVex_b([NotNull] PlPgSqlParser.Vex_bContext context)
        {
            if (context.CAST_EXPRESSION() != null)
                return new CastExpressionNode(context.Start.Line, context.Start.Column)
                {
                    Expression = Visit(context.vex_b(0)) as ExpressionNode,
                    DataType = Visit(context.data_type()) as DataTypeNode
                };
            else if (context.LTH() != null)
            {
                return new LessNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex_b(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex_b(1)) as ExpressionNode
                };
            }
            else if (context.GTH() != null)
            {
                return new GreaterNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex_b(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex_b(1)) as ExpressionNode
                };
            }
            else if (context.LEQ() != null)
            {
                return new LessEqualNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex_b(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex_b(1)) as ExpressionNode
                };
            }
            else if (context.GEQ() != null)
            {
                return new GreaterEqualNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex_b(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex_b(1)) as ExpressionNode
                };
            }
            else if (context.EQUAL() != null)
            {
                return new EqualNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex_b(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex_b(1)) as ExpressionNode
                };
            }
            else if (context.NOT_EQUAL() != null)
            {
                return new NotEqualNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex_b(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex_b(1)) as ExpressionNode
                };
            }
            else if (context.MULTIPLY() != null)
            {
                return new MultiplicationNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex_b(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex_b(1)) as ExpressionNode
                };
            }
            else if (context.DIVIDE() != null)
            {
                return new DivisionNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex_b(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex_b(1)) as ExpressionNode
                };
            }
            else if (context.MODULAR() != null)
            {
                return new ModuloNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex_b(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex_b(1)) as ExpressionNode
                };
            }
            else if (context.PLUS() != null && context.vex_b().Length == 1)
            {
                return new AdditionNode(context.Start.Line, context.Start.Column)
                {
                    RightOperand = Visit(context.vex_b(0)) as ExpressionNode
                };
            }
            else if (context.MINUS() != null && context.vex_b().Length == 1)
            {
                return new SubtractionNode(context.Start.Line, context.Start.Column)
                {
                    RightOperand = Visit(context.vex_b(0)) as ExpressionNode
                };
            }
            else if (context.PLUS() != null)
            {
                return new AdditionNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex_b(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex_b(1)) as ExpressionNode
                };
            }
            else if (context.MINUS() != null)
            {
                return new SubtractionNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex_b(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex_b(1)) as ExpressionNode
                };
            }
            else if (context.EXP() != null)
            {
                return new ExponentiationNode(context.Start.Line, context.Start.Column)
                {
                    LeftOperand = Visit(context.vex_b(0)) as ExpressionNode,
                    RightOperand = Visit(context.vex_b(1)) as ExpressionNode
                };
            }
            else if (context.DISTINCT() != null)
            {
                if (context.NOT() != null)
                    return new IsDistinctFromNode(context.Start.Line, context.Start.Column)
                    {
                        LeftOperand = Visit(context.vex_b(0)) as ExpressionNode,
                        RightOperand = Visit(context.vex_b(1)) as ExpressionNode
                    };
                else
                    return new IsNotDistinctFromNode(context.Start.Line, context.Start.Column)
                    {
                        LeftOperand = Visit(context.vex_b(0)) as ExpressionNode,
                        RightOperand = Visit(context.vex_b(1)) as ExpressionNode
                    };
            }
            else if (context.OF() != null)
            {
                var result = new OfNode(context.Start.Line, context.Start.Column)
                {
                    Expression = Visit(context.vex_b(0)) as ExpressionNode,
                    Not = context.NOT() != null,
                    DataTypes = new List<DataTypeNode>()
                };
                foreach (var dt in context.type_list().data_type())
                {
                    result.DataTypes.Add(Visit(dt) as DataTypeNode);
                }
                return result;
            }
            else if (context.op() != null)
            {
                if (context.vex_b().Length == 2)
                    return new OtherOpBinaryNode(context.Start.Line, context.Start.Column)
                    {
                        Op = Visit(context.op()) as OpNode,
                        LeftOperand = Visit(context.vex_b(0)) as ExpressionNode,
                        RightOperand = Visit(context.vex_b(1)) as ExpressionNode
                    };
                else
                    return new OtherOpUnaryNode(context.Start.Line, context.Start.Column)
                    {
                        Op = Visit(context.op()) as OpNode,
                        Operand = Visit(context.vex_b(0)) as ExpressionNode,
                        FirstOp = context.op().Start.Column < context.vex_b(0).Start.Column
                    };
            }
            else if (context.UNKNOWN() != null)
            {
                if (context.NOT() != null)
                    return new IsNotUnknownNode(context.Start.Line, context.Start.Column)
                    {
                        Operand = Visit(context.vex_b(0)) as ExpressionNode
                    };
                else
                    return new IsUnknownNode(context.Start.Line, context.Start.Column)
                    {
                        Operand = Visit(context.vex_b(0)) as ExpressionNode
                    };

            }
            else if (context.DOCUMENT() != null)
            {
                if (context.NOT() != null)
                    return new IsNotDocumentNode(context.Start.Line, context.Start.Column)
                    {
                        Operand = Visit(context.vex_b(0)) as ExpressionNode
                    };
                else
                    return new IsDocumentNode(context.Start.Line, context.Start.Column)
                    {
                        Operand = Visit(context.vex_b(0)) as ExpressionNode
                    };
            }
            else if (context.value_expression_primary() != null)
            {
                return Visit(context.value_expression_primary());
            }
            else if (context.vex_b() != null && context.vex_b().Length == 1)
            {
                var result = new ExpInDirectionNode(context.Start.Line, context.Start.Column)
                {
                    Expression = Visit(context.vex_b(0)) as ExpressionNode,
                    Indirections = (context.indirection_list() != null) ? new List<IndirectionNode>() : null
                };
                if (context.indirection_list() != null)
                    foreach (var item in context.indirection_list().indirection())
                    {
                        result.Indirections.Add(Visit(item) as IndirectionNode);
                    }
                return result;
            }
            else
            {
                var result = new ExpressionListNode(context.Start.Line, context.Start.Column)
                {
                    Expressions = new List<ExpressionNode>()
                };
                foreach (var exp in context.vex_b())
                {
                    result.Expressions.Add(Visit(exp) as ExpressionNode);
                }
                return result;
            }
        }

        public override ASTNode VisitVex_or_named_notation([NotNull] PlPgSqlParser.Vex_or_named_notationContext context)
        {
            return new VexOrNamedNotationNode(context.Start.Line, context.Start.Column)
            {
                ArgName = (context.argname != null)? Visit(context.argname) as IdNode : null,
                Pointer = (context.pointer() != null)? context.pointer().GetText() : null,
                Expression = Visit(context.vex()) as ExpressionNode
            };
        }

        public override ASTNode VisitView_columns([NotNull] PlPgSqlParser.View_columnsContext context)
        {
            return base.VisitView_columns(context);
        }

        //ok
        public override ASTNode VisitWindow_definition([NotNull] PlPgSqlParser.Window_definitionContext context)
        {
            var result = new WindowsDefinitionNode(context.Start.Line, context.Start.Column)
            {
                 Identifier = (context.identifier() != null)? Visit(context.identifier()) as IdNode : null,
                 PartitionByColumns = (context.partition_by_columns() != null)? new List<ExpressionNode>() :  null,
                 OrderByClause = (context.orderby_clause() != null)? Visit(context.orderby_clause())  as OrderByClauseNode : null,
                 FrameClause = (context.frame_clause() != null)? Visit(context.frame_clause()) as FrameClauseNode : null
            };
            if (context.partition_by_columns() != null)
                foreach (var expr in context.partition_by_columns().vex())
                    result.PartitionByColumns.Add(Visit(expr) as ExpressionNode);
            return result;
        }

        public override ASTNode VisitWith_check_option([NotNull] PlPgSqlParser.With_check_optionContext context)
        {
            return base.VisitWith_check_option(context);
        }

        public override ASTNode VisitWith_clause([NotNull] PlPgSqlParser.With_clauseContext context)
        {
            var withClause = new WithClauseNode(context.Start.Line, context.Start.Column)
            {
                WithQuerys = new List<WithQueryNode>()
            };
            foreach (var q in context.with_query())
            {
                withClause.WithQuerys.Add(Visit(q) as WithQueryNode);
            }
            return withClause;
        }

        public override ASTNode VisitWith_query([NotNull] PlPgSqlParser.With_queryContext context)
        {
            var withQuery = new WithQueryNode(context.Start.Line, context.Start.Column)
            {
                Identifier = Visit(context.query_name) as IdNode,
                Columns = (context._column_name != null)? new List<IdNode>(): null
            };

            if (context._column_name != null)
                foreach (var columnName in context._column_name)
                    withQuery.Columns.Add(Visit(columnName) as IdNode);

            if (context.select_stmt() != null)
                withQuery.Statement = Visit(context.select_stmt()) as SelectStatementNode;
            else if (context.insert_stmt_for_psql() != null)
                withQuery.Statement = Visit(context.insert_stmt_for_psql()) as InsertStmtPSqlNode;
            else if (context.update_stmt_for_psql() != null)
                withQuery.Statement = Visit(context.update_stmt_for_psql()) as UpdateStmtPSqlNode;
            else if (context.delete_stmt_for_psql() != null)
                withQuery.Statement = Visit(context.delete_stmt_for_psql()) as DeleteStmtPSqlNode;
            return withQuery;

        }

        public override ASTNode VisitWith_storage_parameter([NotNull] PlPgSqlParser.With_storage_parameterContext context)
        {
            return base.VisitWith_storage_parameter(context);
        }
        
        //ok
        public override ASTNode VisitXml_function([NotNull] PlPgSqlParser.Xml_functionContext context)
        {
            if (context.XMLELEMENT() != null)
            {
                var result = new XmlElementFunctionNode(context.Start.Line, context.Start.Column)
                {
                    Name = Visit(context.name) as IdNode,
                    AttNames = (context.attname != null) ?  new List<IdNode>() : null,
                    Expressions = (context.vex() != null) ? new List<ExpressionNode>() : null
                };
                if (context.attname != null)
                    foreach (var an in context.identifier())
                        if (an != context.name)
                            result.AttNames.Add(Visit(context.attname) as IdNode);
                if (context.vex() != null)
                    foreach (var expr in context.vex())
                        result.Expressions.Add(Visit(expr) as ExpressionNode);
                return result;
            }
            else if (context.XMLFOREST() != null)
            {
                var result = new XmlForestFunctionNode(context.Start.Line, context.Start.Column)
                {
                    Expressions = new List<ExpressionNode>(),
                    Names = (context.name != null) ? new List<IdNode>() : null
                };
                if (context.name != null)
                    foreach (var name in context.identifier())
                        result.Names.Add(Visit(name) as IdNode);
                
                foreach (var expr in context.vex())
                    result.Expressions.Add(Visit(expr) as ExpressionNode);
                return result;
            }
            else if (context.XMLPI() != null)
            {
                return new XmlPiFunctionNode(context.Start.Line, context.Start.Column)
                {
                    Expression = (context.vex() != null) ? Visit(context.vex(0)) as ExpressionNode : null,
                    Name = Visit(context.name) as IdNode
                };
            }
            else if (context.XMLROOT() != null)
            {
                var result = new XmlRootFunctionNode(context.Start.Line, context.Start.Column)
                {
                    Expressions = new List<ExpressionNode>(),
                };
                foreach (var expr in context.vex())
                    result.Expressions.Add(Visit(expr) as ExpressionNode);
                return result;
            }
            else if (context.XMLEXISTS() != null)
            {
                var result = new XmlExistsFunctionNode(context.Start.Line, context.Start.Column)
                {
                    Expressions = new List<ExpressionNode>(),
                };
                foreach (var expr in context.vex())
                    result.Expressions.Add(Visit(expr) as ExpressionNode);
                return result;
            }
            else if (context.XMLPARSE() != null)
            {
                return new XmlParseFunctionNode(context.Start.Line, context.Start.Column)
                {
                    Expression = Visit(context.vex(0)) as ExpressionNode,
                };
            }
            else if (context.XMLSERIALIZE() != null)
            {
                return new XmlSerializeFunctionNode(context.Start.Line, context.Start.Column)
                {
                    Expression = Visit(context.vex(0)) as ExpressionNode,
                    DataType = Visit(context.data_type()) as DataTypeNode
                };
            }
            else
            {
                var result = new XmlTabletFunctionNode(context.Start.Line, context.Start.Column)
                {
                    Expressions = new List<ExpressionNode>(),
                    Names = new List<IdNode>(),
                    XmlTableColumns = new List<XmlTableColumnNode>()
                };
                foreach (var expr in context.vex())
                    result.Expressions.Add(Visit(expr) as ExpressionNode);
                foreach (var name in context.identifier())
                    result.Names.Add(Visit(name) as IdNode);
                foreach (var tc in context.xml_table_column())
                    result.XmlTableColumns.Add(Visit(tc) as XmlTableColumnNode);
                return result;
            }
        }

        public override ASTNode VisitXml_table_column([NotNull] PlPgSqlParser.Xml_table_columnContext context)
        {
            var result = new XmlTableColumnNode(context.Start.Line, context.Start.Column)
            {
                Name = Visit(context.name) as IdNode,
                DataType = (context.data_type() != null)? Visit(context.data_type()) as DataTypeNode : null,
                Expressions = (context.vex() != null)? new List<ExpressionNode>() : null
            };
            if (context.vex() != null)
                foreach (var expr in context.vex())
                    result.Expressions.Add(Visit(expr) as ExpressionNode);
            return result;
        }
    }
}
