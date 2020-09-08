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
using DB2BM.Abstractions.AST.Expressions.Operators.Unarys.Arithmetics;
using DB2BM.Abstractions.AST.Expressions.Operators.Unarys.Comparison;
using DB2BM.Abstractions.AST.Expressions.Operators.Unarys.Logicals;
using DB2BM.Abstractions.AST.Statements;
using DB2BM.Abstractions.AST.Statements.Additional;
using DB2BM.Abstractions.AST.Statements.Base;
using DB2BM.Abstractions.AST.Statements.Control;
using DB2BM.Abstractions.AST.Statements.Cursor;
using DB2BM.Abstractions.AST.Statements.Data;
using DB2BM.Abstractions.AST.Types;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Visitors;
using DB2BM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace DB2BM.Extensions.EFCore.Visitors
{
    public class EFCoreCodeGenVisitor : ASTVisitor<CodeContext>
    {
        DatabaseCatalog Catalog;
        StoredProcedure Sp;
        bool UseFoundVariable;
        Dictionary<string, string> TablesAlias = new Dictionary<string, string>();
        List<StoredProcedure> InternalFunctionUse = new List<StoredProcedure>();
        int countVariablesGen = 0;
        int identation;
        bool InQuery;

        public static readonly Dictionary<string, string> TypesMapper = new Dictionary<string, string>
        {
            { "object" , "dynamic"},
            { "dynamic" , "dynamic"},
            { "bool", "bool?" },
            { "short", "short?"},
            { "int", "int?" },
            { "long", "long?"},
            { "decimal", "decimal?"},
            { "float", "float?" },
            { "double", "double?" },
            { "string", "string" },
            { "DateTime", "DateTime?" },
            { "TimeSpan", "TimeSpan?" },
            { "dynamic[]" , "dynamic[]"},
            { "uint", "uint?"},
            { "char", "char"},
            { "Guid", "Guid"},
            { "int[]", "int?[]" },
            { "long[]", "long?[]"},
            { "bool[]", "bool?[]"},
            { "byte[]", "byte?[]"},
            { "uint[]", "uint?[]"},
            { "PhysicalAddress", "PhysicalAddress" },
            { "NpgsqlTsQuery","NpgsqlTsQuery" },
            { "NpgsqlTsVector","NpgsqlTsVector" },
            { "NpgsqlBox", "NpgsqlBox"},
            { "NpgsqlCircle", "NpgsqlCircle" },
            { "NpgsqlLine", "NpgsqlLine" },
            { "NpgsqlPolygon", "NpgsqlPolygon"},
            { "NpgsqlPath", "NpgsqlPath"},
            { "NpgsqlLSeg", "NpgsqlLSeg"},
            { "NpgsqlPoint", "NpgsqlPoint"},
            {  "(IPAddress,int)", "(IPAddress,int)"},
            { "Dictionary<string,string>", "Dictionary<string,string>"}
        };

        string GenVariable()
        {
            var varibleIndex = (++countVariablesGen).ToString();
            while (varibleIndex.Length < 3)
                varibleIndex = "0" + varibleIndex;
            return "_" + varibleIndex;
        }
        void SetType(StoredProcedure sp)
        {
            if (TypesMapper.ContainsKey(sp.ReturnType))
                sp.ReturnType = TypesMapper[sp.ReturnType];
            else if (Catalog.Tables.Values.Any(t => t.Name == sp.ReturnType))
            {
                if (sp.ReturnClause.Contains("setof"))
                    sp.ReturnType = $"IEnumerable<{sp.ReturnType.ToPascal()}>";
                else
                    sp.ReturnType = sp.ReturnType.ToPascal();
            }
            else if (Catalog.UserDefinedTypes.Values.Any(u => u.TypeName == sp.ReturnType))
            {
                if (sp.ReturnClause.Contains("setof"))
                    sp.ReturnType = $"IEnumerable<{sp.ReturnType}>";
            }
        }
        string GetIdentation
        {
            get
            {
                var result = "";
                int i = 0;
                while (i++ < identation)
                    result += "\t";
                return result;
            }
        }

        public EFCoreCodeGenVisitor(DatabaseCatalog catalog, StoredProcedure sp)
        {
            Catalog = catalog;
            Sp = sp;
            identation = 0;
            SetType(Sp);
        }

        public override CodeContext Visit(FunctionBlockNode node)
        {
            var result = "";
            var foundDec = "var FOUND = false;\n";
            if (node.Declarations != null)
                foreach (var dec in node.Declarations)
                    result += VisitNode(dec).Code + "\n";
            if (node.Statements != null)
                foreach (var stmt in node.Statements)
                    result += VisitNode(stmt).Code + "\n";

            var codeContext = new CodeContext();

            var declarationOutParams = "";
            var outParameters = Sp.Params.Where(p => p.OutMode).ToList();
            var parameters = "";

            var code = (UseFoundVariable) ? foundDec + result : result;
            foreach (var p in outParameters)
            {
                if (Sp.ReturnClause.Contains("setof"))
                    declarationOutParams += $"IEnumerable<{TypesMapper[p.DestinyType]}> {p.Name.ToCamel()};\n";
                else
                    declarationOutParams += $"{TypesMapper[p.DestinyType]} {p.Name.ToCamel()} = default({TypesMapper[p.DestinyType]});\n";
                parameters += (parameters == "") ? p.Name.ToCamel() : ", " + p.Name.ToCamel();
            }
            if (Sp.LanguageDefinition.ToLower() == "sql")
            {
                codeContext.Code = "return " + result;
                if (Sp.ReturnClause.ToLower().Contains("setof"))
                    codeContext.Code = codeContext.Code.Replace(".FirstOrDefault()", "");
            }
            else
            {
                codeContext.Code = code;
                if (outParameters.Count > 0)
                {
                    codeContext.Code = declarationOutParams + codeContext.Code + "return (" + parameters + ");";
                }
            }
            codeContext.InternalFunctionUse = InternalFunctionUse;


            if (node.ExceptionStatement != null)
            {
                var exceptionCode = VisitNode(node.ExceptionStatement).Code;
                var existCode = codeContext.Code;
                codeContext.Code = "try \n" +
                                   "{\n" +
                                        existCode + "\n" +
                                   "}\n" +
                                   "catch (Exception)\n" +
                                   "{\n" +
                                        exceptionCode + "\n" +
                                   "}";
            }

            return codeContext;
        }
        List<string> replaceList = new List<string>();
        public override CodeContext Visit(DeclarationNode node)
        {
            if (node.TypeDeclaration is AliasDeclarationNode)
            {
                var typeDeclaration = node.TypeDeclaration as AliasDeclarationNode;
                return new CodeContext()
                {
                    Code = $"var {VisitNode(node.Identifier).Code.ToCamel()} = {VisitNode(typeDeclaration.Identifier).Code.ToCamel()};"
                };
            }
            else if (node.TypeDeclaration is OrdinalTypeDeclarationNode)
            {
                var typeDeclaration = node.TypeDeclaration as OrdinalTypeDeclarationNode;
                var dataTypeCode = VisitNode(typeDeclaration.DataType).Code;
                if (typeDeclaration.Expression == null)
                {
                    return new CodeContext()
                    {
                        Code = $"var {VisitNode(node.Identifier).Code.ToCamel()} = default({dataTypeCode});"
                    };
                }
                else
                    return new CodeContext()
                    {
                        Code = $"{dataTypeCode} {VisitNode(node.Identifier).Code.ToCamel()} = {VisitNode(typeDeclaration.Expression).Code};"
                    };
            }
            else if (node.TypeDeclaration is CursorDeclarationNode)
            {
                var typeDeclaration = node.TypeDeclaration as CursorDeclarationNode;
                var arguments = "";
                UseFoundVariable = true;
                replaceList = new List<string>();
                foreach (var arg in typeDeclaration.ArgumentList)
                {
                    replaceList.Add(arg.Identifier.Text);
                    var acode = VisitNode(arg).Code;

                    if (arguments == "")
                        arguments += acode;
                    else
                        arguments += acode + ",";
                }
                var code = $"{GetIdentation}IEnumerator<dynamic> {VisitNode(node.Identifier).Code.ToCamel()};\n";
                code +=    $"{GetIdentation}IEnumerable<dynamic> {VisitNode(node.Identifier).Code.ToPascal()} ({arguments})\n" +
                              GetIdentation + "{\n" +
                              GetIdentation + "\t" + "return " + VisitNode(typeDeclaration.SelectStmt).Code.Replace(".FirstOrDefault()", "") + "\n" +
                              GetIdentation + "}";

                replaceList = new List<string>();
                return new CodeContext()
                {
                    Code = code
                };
            }
            return null;
        }


        public override CodeContext Visit(AliasDeclarationNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(CursorDeclarationNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(OrdinalTypeDeclarationNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(IsNotDistinctFromNode node)
        {
            var rOpCodeContext = VisitNode(node.LeftOperand);
            var lOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = rOpCodeContext.UserFunctionCall || lOpCodeContext.UserFunctionCall,
                Code = $"{lOpCodeContext.Code} == {rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(AllSimpleOpNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(OtherTypeNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(OtherOpBinaryNode node)
        {
            var codeCodeContext = new CodeContext();
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            var opCodeContext = VisitNode(node.Op);
            codeCodeContext.Code = $"{lOpCodeContext.Code} {opCodeContext.Code} {rOpCodeContext.Code}";
            codeCodeContext.UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall;
            return codeCodeContext;
        }

        public override CodeContext Visit(DataTypeNode node)
        {
            var codeContext = new CodeContext();
            if (TypesMapper.ContainsKey(node.TypeReturn))
                codeContext.Code = TypesMapper[node.TypeReturn];
            else if (Catalog.Tables.Values.Any(t => t.Name == node.TypeReturn))
                codeContext.Code = node.TypeReturn.ToPascal();
            else codeContext.Code = node.TypeReturn;
            return codeContext;
        }

        public override CodeContext Visit(NCharTypeNode node)
        {
            return new CodeContext() { Code = "string" };
        }

        public override CodeContext Visit(NCharVaryingTypeNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(NumericTypeNode node)
        {
            return new CodeContext() { Code = "decimal?" };
        }

        public override CodeContext Visit(RealTypeNode node)
        {
            return new CodeContext() { Code = "float?" };
        }

        public override CodeContext Visit(BigintTypeNode node)
        {
            return new CodeContext() { Code = "long?" };
        }

        public override CodeContext Visit(SmallintTypeNode node)
        {
            return new CodeContext() { Code = "short?" };
        }

        public override CodeContext Visit(BitTypeNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(TimeTypeNode node)
        {
            return new CodeContext() { Code = "DateTime?" };
        }

        public override CodeContext Visit(BitVaryingTypeNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(VarcharTypeNode node)
        {
            return new CodeContext() { Code = "string" };
        }

        public override CodeContext Visit(BooleanTypeNode node)
        {
            return new CodeContext() { Code = "bool?" };
        }

        public override CodeContext Visit(DecTypeNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(DecimalTypeNode node)
        {
            return new CodeContext() { Code = "decimal?" };
        }

        public override CodeContext Visit(DollarNumberNode node)
        {
            int index;
            var codeContext = new CodeContext();
            if (int.TryParse(new string(node.Text.Skip(1).ToArray()), out index))
                codeContext.Code = Sp.Params[index - 1].Name.ToCamel();
            else codeContext.Code = "";
            return codeContext;
        }

        public override CodeContext Visit(DoublePrecisionTypeNode node)
        {
            return new CodeContext() { Code = "double?" };
        }

        public override CodeContext Visit(AtTimeZoneNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(FloatTypeNode node)
        {
            return new CodeContext() { Code = "float?" };
        }

        public override CodeContext Visit(IntTypeNode node)
        {
            return new CodeContext() { Code = "int?" };
        }

        public override CodeContext Visit(IntegerTypeNode node)
        {
            return new CodeContext() { Code = "int?" };
        }

        public override CodeContext Visit(IntervalTypeNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(CharTypeNode node)
        {
            return new CodeContext() { Code = "string" };
        }

        public override CodeContext Visit(CharVaryingTypeNode node)
        {
            return new CodeContext() { Code = "string" };
        }

        public override CodeContext Visit(AddAnalizeNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(AddClusterNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(AddDeallocatteNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(AddListenNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(AddPrepareNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(AddReassignNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(AddRefreshNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(AddReindexNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(AddResetNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(AddUnlistenNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(CopyStmtNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(ExplainStmtNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(ShowStmtNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(CallFunctionCallNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(WithClauseNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(WithQueryNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(CaseStmtNode node)
        {
            var codeContext = new CodeContext();
            if (node.HeaderExpression != null)
            {
                var headCodeContext = VisitNode(node.HeaderExpression);
                codeContext.Code = $"{GetIdentation}switch ({headCodeContext.Code})\n" +
                                    GetIdentation + "{\n";
                identation++;
                foreach (var item in node.Cases)
                {
                    var exp = VisitNode(item.Expressions[0]);
                    if (node.HeaderExpression.TypeReturn != item.Expressions[0].TypeReturn)
                        codeContext.Code += $"{GetIdentation}case ({node.HeaderExpression.TypeReturn}){exp.Code}:\n" +
                                            GetIdentation + "{\n";
                    else codeContext.Code += $"{GetIdentation}case {exp.Code}:\n" +
                                            GetIdentation + "{\n";
                    foreach (var stmt in item.Stmts)
                        codeContext.Code += $"{GetIdentation}{VisitNode(stmt).Code}\n " + $"{GetIdentation}break;\n";
                    codeContext.Code += GetIdentation + "}\n";
                }
                if (node.ElseStmts != null)
                {
                    codeContext.Code += $"{GetIdentation}default:\n" +
                                        GetIdentation + "{\n";
                    foreach (var stmt in node.ElseStmts)
                        codeContext.Code += $"{GetIdentation}{VisitNode(stmt).Code}\n" + $"{GetIdentation}break;\n";
                    codeContext.Code += GetIdentation + "}\n";
                }
                identation--;
                codeContext.Code += GetIdentation + "}";
            }
            else
            {
                foreach (var _case in node.Cases)
                {
                    var exp = VisitNode(_case.Expressions[0]);
                    if (codeContext.Code == null || codeContext.Code == "")
                        codeContext.Code = $"{GetIdentation}if ({exp.Code})\n";
                    else codeContext.Code = $"{GetIdentation}else if ({exp.Code})\n";
                    codeContext.Code += GetIdentation + "{\n";
                    identation++;
                    foreach (var stmt in _case.Stmts)
                        codeContext.Code += VisitNode(stmt).Code + "\n";
                    identation--;
                    codeContext.Code += GetIdentation + "}\n";
                }
                if (node.ElseStmts != null)
                {
                    codeContext.Code += GetIdentation + "else \n" +
                            GetIdentation + "{\n";
                    identation++;
                    foreach (var stmt in node.ElseStmts)
                        codeContext.Code += VisitNode(stmt).Code + "\n";
                    identation--;
                    codeContext.Code += GetIdentation + "}\n";
                }
            }
            return codeContext;
        }

        public override CodeContext Visit(WindowsDefinitionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(NullOrderingNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(CastExpressionNode node)
        {
            var expCodeContext = VisitNode(node.Expression);
            return new CodeContext()
            {
                UserFunctionCall = expCodeContext.UserFunctionCall,
                Code = $"({VisitNode(node.DataType).Code})({expCodeContext.Code})"
            };
        }

        public override CodeContext Visit(IsNullNode node)
        {
            var opCodeContext = VisitNode(node.Operand);
            return new CodeContext()
            {
                Code = $"{opCodeContext.Code} == null",
                UserFunctionCall = opCodeContext.UserFunctionCall
            };
        }

        public override CodeContext Visit(IsNotNullNode node)
        {
            var opCodeContext = VisitNode(node.Operand);
            return new CodeContext()
            {
                Code = $"{opCodeContext.Code} != null",
                UserFunctionCall = opCodeContext.UserFunctionCall
            };
        }

        public override CodeContext Visit(BetweenNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(InNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(OfNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(SelectStatementNode node)
        {
            InQuery = true;
            var codeContext = new CodeContext();
            if (node.SelectOps != null)
            {
                var selectOpsCodeContext = VisitNode(node.SelectOps);
                codeContext.Code = selectOpsCodeContext.Code;
                codeContext.UserFunctionCall = selectOpsCodeContext.UserFunctionCall;
            }
            if (node.AfterOps != null && node.AfterOps.Count > 0)
            {
                codeContext.Code = codeContext.Code.Replace(".FirstOrDefault()", "");
                foreach (var afterOp in node.AfterOps)
                {
                    var afterOpCodeContext = VisitNode(afterOp);
                    codeContext.Code = $"({codeContext.Code}).{afterOpCodeContext.Code}";
                    codeContext.UserFunctionCall |= afterOpCodeContext.UserFunctionCall;
                }
            }
            codeContext.Code += ";";
            InQuery = false;
            return codeContext;
        }

        public override CodeContext Visit(SelectStmtNonParensNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(TruncateStmtNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(IsTrueNode node)
        {
            var opCodeContext = VisitNode(node.Operand);
            return new CodeContext()
            {
                Code = opCodeContext.Code,
                UserFunctionCall = opCodeContext.UserFunctionCall
            };
        }

        public override CodeContext Visit(IsNotTrueNode node)
        {
            var opCodeContext = VisitNode(node.Operand);
            return new CodeContext()
            {
                Code = "!(" + opCodeContext.Code + ")",
                UserFunctionCall = opCodeContext.UserFunctionCall
            };
        }

        public override CodeContext Visit(IsFalseNode node)
        {
            var opCodeContext = VisitNode(node.Operand);
            return new CodeContext()
            {
                Code = "!(" + opCodeContext.Code + ")",
                UserFunctionCall = opCodeContext.UserFunctionCall
            };
        }

        public override CodeContext Visit(IsNotFalseNode node)
        {
            var opCodeContext = VisitNode(node.Operand);
            return new CodeContext()
            {
                Code = opCodeContext.Code,
                UserFunctionCall = opCodeContext.UserFunctionCall
            };
        }

        public override CodeContext Visit(CollateNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(LikeBinaryNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var lCode = (node.LeftOperand.TypeReturn != "string") ?
                $"(string){lOpCodeContext.Code}" : lOpCodeContext.Code;
            var rOpCodeContext = VisitNode(node.RightOperand);
            var rCode = (node.RightOperand.TypeReturn != "string") ?
                $"(string){rOpCodeContext.Code}" : rOpCodeContext.Code;
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = (InQuery) ? $"EF.Functions.Like({lCode}, {rCode})" :
                    $"DbContext.Like({ lCode },{ rCode })"
            };

        }

        public override CodeContext Visit(ILikeBinaryNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var lCode = (node.LeftOperand.TypeReturn != "string") ?
                $"(string){lOpCodeContext.Code}" : lOpCodeContext.Code;
            var rOpCodeContext = VisitNode(node.RightOperand);
            var rCode = (node.RightOperand.TypeReturn != "string") ?
                $"(string){rOpCodeContext.Code}" : rOpCodeContext.Code;
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = (InQuery) ? $"EF.Functions.Like(({lCode}).ToLower(), ({rCode}).ToLower())" :
                    $"DbContext.Like(({lCode}).ToLower(), ({rCode}).ToLower())"
            };
        }

        public override CodeContext Visit(NotLikeBinaryNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var lCode = (node.LeftOperand.TypeReturn != "string") ?
                $"(string){lOpCodeContext.Code}" : lOpCodeContext.Code;
            var rOpCodeContext = VisitNode(node.RightOperand);
            var rCode = (node.RightOperand.TypeReturn != "string") ?
                $"(string){rOpCodeContext.Code}" : rOpCodeContext.Code;
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = (InQuery) ? $"!EF.Functions.Like({lCode}, {rCode})" :
                    "!" + $"DbContext.Like({ lCode },{ rCode })"
            };
        }

        public override CodeContext Visit(NotILikeBinaryNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var lCode = (node.LeftOperand.TypeReturn != "string") ?
                $"(string){lOpCodeContext.Code}" : lOpCodeContext.Code;
            var rOpCodeContext = VisitNode(node.RightOperand);
            var rCode = (node.RightOperand.TypeReturn != "string") ?
                $"(string){rOpCodeContext.Code}" : rOpCodeContext.Code;
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = (InQuery) ? $"!EF.Functions.Like(({lCode}).ToLower(), ({rCode}).ToLower())" :
                    "!" + $"DbContext.Like(({lCode}).ToLower(), ({rCode}).ToLower())"
            };
        }

        public override CodeContext Visit(SimilarToBinaryNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            InternalFunctionUse.Add(Catalog.InternalFunctions.Values.FirstOrDefault(f => f.Name == "similar_escape"));
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"DbContext.SimilarEscape({lOpCodeContext.Code}, {rOpCodeContext.Code})"
            };
        }

        public override CodeContext Visit(NotSimilarToBinaryNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"!DbContext.SimilarEscape({lOpCodeContext.Code}, {rOpCodeContext.Code})",
                InternalFunctionUse = new List<StoredProcedure>()
                {
                    Catalog.InternalFunctions.Values.FirstOrDefault(f => f.Name == "similar_escope")
                }
            };
        }

        public override CodeContext Visit(NotNode node)
        {
            var opCodeContext = VisitNode(node.Operand);
            return new CodeContext()
            {
                Code = $"!({opCodeContext.Code})",
                UserFunctionCall = opCodeContext.UserFunctionCall
            };
        }

        public override CodeContext Visit(AbsoluteValueNode node)
        {
            var opCodeContext = VisitNode(node.Operand);
            return new CodeContext()
            {
                Code = $"Math.Abs({VisitNode(node.Operand)})",
                UserFunctionCall = opCodeContext.UserFunctionCall
            };
        }

        public override CodeContext Visit(BitwiseNotNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(FactorialNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(OtherOpUnaryNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(IsDocumentNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(IsNotDocumentNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(IsUnknownNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(IsNotUnknownNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(BaseTypeCoercionNode node)
        {
            var codeContext = new CodeContext();
            if (node.TypeReturn == "interval")
            {
                var id = node.Id.Replace("'", "");
                codeContext.Code = "DbContext.Interval(\"" + id + "\")";
            }
            else
                codeContext.Code = $"({VisitNode(node.DataType).Code}){node.Id}";
            return codeContext;
        }

        public override CodeContext Visit(IntervalTypeCoercionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(EqualNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"{lOpCodeContext.Code} == {rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(GreaterEqualNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"{lOpCodeContext.Code} >= {rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(GreaterNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"{lOpCodeContext.Code} > {rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(IsDistinctFromNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"{lOpCodeContext.Code} != {rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(LessEqualNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"{lOpCodeContext.Code} <= {rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(LessNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"{lOpCodeContext.Code} < {rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(NotEqualNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"{lOpCodeContext.Code} != {rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(ExponentiationNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"Math.Pow({lOpCodeContext.Code},{rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(ModuloNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"{lOpCodeContext.Code} % {rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(MultiplicationNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"{lOpCodeContext.Code} * {rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(StringConcatNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"{lOpCodeContext.Code} + {rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(SubtractionNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"{lOpCodeContext.Code} - {rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(AdditionNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"{lOpCodeContext.Code} + {rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(BitwiseAndNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"{lOpCodeContext.Code} & {rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(BitwiseOrNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"{lOpCodeContext.Code} | {rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(BitwiseShiftLeftNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"{lOpCodeContext.Code} << {rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(BitwiseShiftRightNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"{lOpCodeContext.Code} >> {rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(BitwiseXorNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"{lOpCodeContext.Code} ^ {rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(DivisionNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"{lOpCodeContext.Code} / {rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(AndNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"{lOpCodeContext.Code} && {rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(OrNode node)
        {
            var lOpCodeContext = VisitNode(node.LeftOperand);
            var rOpCodeContext = VisitNode(node.RightOperand);
            return new CodeContext()
            {
                UserFunctionCall = lOpCodeContext.UserFunctionCall || rOpCodeContext.UserFunctionCall,
                Code = $"{lOpCodeContext.Code} || {rOpCodeContext.Code}"
            };
        }

        public override CodeContext Visit(ExpInDirectionNode node)
        {
            var codeContext = new CodeContext();
            var expCodeContext = VisitNode(node.Expression);
            codeContext.Code = $"({expCodeContext.Code})";
            codeContext.UserFunctionCall |= expCodeContext.UserFunctionCall;
            if (node.Indirections != null)
                foreach (var indirection in node.Indirections)
                {
                    var indirectionCodeContext = VisitNode(indirection);
                    codeContext.Code += indirectionCodeContext.Code;
                    codeContext.UserFunctionCall |= indirectionCodeContext.UserFunctionCall;
                }
            return codeContext;
        }

        public override CodeContext Visit(ExpressionListNode node)
        {
            var codeContext = new CodeContext();
            var expressionsCode = "";
            foreach (var item in node.Expressions)
                expressionsCode += (expressionsCode == "") ? VisitNode(item).Code : ", " + VisitNode(item).Code;
            codeContext.Code = $"({expressionsCode})";
            return codeContext;
        }

        public override CodeContext Visit(NullNode node)
        {
            return new CodeContext() { Code = "null" };
        }

        public override CodeContext Visit(MultiplyNode node)
        {
            var codeContext = new CodeContext();
            if (multiplySustitution != null || multiplySustitution != "")
                codeContext.Code = multiplySustitution;
            else if (TablesAlias.Count > 0)
                codeContext.Code = TablesAlias.First().Key;
            else codeContext.Code = "";
            return codeContext;
        }

        public override CodeContext Visit(CaseExpressionNode node)
        {
            var start = 0;
            var end = node.Expressions.Count - 1;

            var headCodeContext = (node.ContainsRootExpression) ? VisitNode(node.Expressions[start++]) : null;
            var endCodeContext = (node.Else) ? VisitNode(node.Expressions[end--]) : null;

            var conditionsCodeContext = new List<CodeContext>();
            var expsCodeContext = new List<CodeContext>();
            for (int i = start; i <= end; i += 2)
            {
                conditionsCodeContext.Add(VisitNode(node.Expressions[i]));
                expsCodeContext.Add(VisitNode(node.Expressions[i + 1]));
            }
            var codeContext = new CodeContext();
            for (int i = 0; i < conditionsCodeContext.Count; i++)
            {
                if (node.ContainsRootExpression)
                    codeContext.Code += $"({headCodeContext.Code} == {conditionsCodeContext[i].Code})? {expsCodeContext[i].Code} : ";
                else
                    codeContext.Code += $"({conditionsCodeContext[i].Code})? {expsCodeContext[i].Code} : ";
            }
            if (node.Else)
                codeContext.Code += $"{endCodeContext.Code}";
            return codeContext;
        }

        public override CodeContext Visit(ExistsNode node)
        {
            var stmtCodeContext = VisitNode(node.SelectStmt);
            return new CodeContext()
            {
                Code = $"({stmtCodeContext.Code}).Any()",
                UserFunctionCall = stmtCodeContext.UserFunctionCall
            };
        }

        public override CodeContext Visit(IndirectionVarNode node)
        {
            var identifierCodeContext = VisitNode(node.Identifiers);
            var codeContext = new CodeContext()
            {
                Code = identifierCodeContext.Code,
                UserFunctionCall = identifierCodeContext.UserFunctionCall
            };
            if (node.Indirections != null)
                foreach (var item in node.Indirections)
                {
                    var indirectionCodeContext = VisitNode(item);
                    codeContext.Code += indirectionCodeContext.Code;
                    codeContext.UserFunctionCall |= indirectionCodeContext.UserFunctionCall;
                }
            return codeContext;
        }

        public override CodeContext Visit(ValueExpressionPrimaryNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(IntervalFieldNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(DatetimeOverlapsNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(TableColsNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(UserNameNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(GroupByClauseNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(ArrayTypeNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(ArrayToSelectNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(ArrayElementsNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(ComparisonModNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(BasicFunctionCallNode node)
        {
            var codeContext = new CodeContext();
            var functionName = node.SchemaQualifiednameNonType.IdentifierNonType.Text;
            if (InQuery && functionName.ToLower() == "count")
            {
                var arg = VisitNode(node.VexOrNamedNotations[0]);
                codeContext.Code = $"Count()";
            }
            else if (InQuery && functionName.ToLower() == "Sum")
            {
                var arg = VisitNode(node.VexOrNamedNotations[0]);
                codeContext.Code = $"Sum({arg.Code})";
            }
            else
            {
                var userDefinedFunctions = Catalog.StoredProcedures.Values.Where(f => f.Name == functionName).ToList();
                var internalFunctions = Catalog.InternalFunctions.Values.Where(f => f.Name == functionName).ToList();
                if (userDefinedFunctions != null && userDefinedFunctions.Count > 0)
                    foreach (var function in userDefinedFunctions)
                    {
                        if ((from p in function.Params where !p.OutMode select p).ToList().Count == node.VexOrNamedNotations.Count)
                        {
                            codeContext.UserFunctionCall = true;

                            codeContext.Code = functionName.ToPascal() + "(";
                            var first = true;
                            foreach (var arg in node.VexOrNamedNotations)
                            {
                                var argCodeContext = Visit(arg);
                                if (first)
                                {
                                    codeContext.Code += argCodeContext.Code;
                                    first = false;
                                }
                                else
                                    codeContext.Code += ", " + argCodeContext.Code;
                            }
                            codeContext.Code += ")";
                        }
                    }
                if (internalFunctions != null && internalFunctions.Count > 0)
                    foreach (var function in internalFunctions)
                    {
                        if (function.Params.Where(p => !p.OutMode).Count() == node.VexOrNamedNotations.Count)
                        {
                            codeContext.Code = "DbContext." + functionName.ToPascal() + "(";
                            InternalFunctionUse.Add(function);
                            var first = true;
                            foreach (var arg in node.VexOrNamedNotations)
                            {
                                var argCodeContext = Visit(arg);
                                if (first)
                                    codeContext.Code += argCodeContext.Code;
                                else
                                    codeContext.Code += ", " + argCodeContext.Code;
                                codeContext.UserFunctionCall |= argCodeContext.UserFunctionCall;
                            }
                            codeContext.Code += ")";
                        }
                    }
            }
            return codeContext;
        }

        public override CodeContext Visit(FunctionConstructNode node)
        {
            var codeContext = new CodeContext();

            if (node.Coalesce)
            {
                var lExpCodeContext = VisitNode(node.Expressions[0]);
                var rExpCodeContext = VisitNode(node.Expressions[1]);
                codeContext.Code = (node.Expressions[0].TypeReturn == node.Expressions[1].TypeReturn) ?
                    $"{lExpCodeContext.Code} ?? {rExpCodeContext.Code}" :
                    $"{lExpCodeContext.Code} ?? ({node.Expressions[0].TypeReturn}){rExpCodeContext.Code}";
                codeContext.UserFunctionCall = lExpCodeContext.UserFunctionCall || rExpCodeContext.UserFunctionCall;
            }
            else if (node.Nullif)
            {
                var lExpCodeContext = VisitNode(node.Expressions[0]);
                var rExpCodeContext = VisitNode(node.Expressions[1]);
                codeContext.Code = $"({lExpCodeContext.Code} == {rExpCodeContext.Code}) ? null : {lExpCodeContext.Code}";
            }
            else if (node.Greatest || node.Least)
            {
                var expsCodeContext = new List<CodeContext>();
                foreach (var item in node.Expressions)
                    expsCodeContext.Add(VisitNode(item));

                var parametersCode = "";
                foreach (var expCode in expsCodeContext)
                    parametersCode = (parametersCode == "") ? expCode.Code : ", " + expCode.Code;
                codeContext.Code += $"({parametersCode})";
                if (node.Greatest)
                    codeContext.Code += $".Max()";
                else
                    codeContext.Code += $".Min()";
            }
            return codeContext;
        }

        public override CodeContext Visit(ExtractFunctionNode node)
        {
            var codeContext = new CodeContext();
            var expCodeContext = VisitNode(node.Expression);
            codeContext.UserFunctionCall |= expCodeContext.UserFunctionCall;
            CodeContext idCodeContext = null;
            if (node.Identifier != null)
            {
                idCodeContext = VisitNode(node.Identifier);
                codeContext.UserFunctionCall |= idCodeContext.UserFunctionCall;
            }
            codeContext.Code = (idCodeContext != null) ?
                $"({expCodeContext.Code}).{idCodeContext.Code.ToPascal()}" :
                expCodeContext.Code;
            return codeContext;
        }

        public override CodeContext Visit(TrimStringValueFunctionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(SubstringStringValueFunctionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(PositionStringValueFunctionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(OverlayStringValueFunctionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(CollationStringValueFunctionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(CastSpesificationSystemFunction node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(XmlElementFunctionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(XmlForestFunctionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(XmlPiFunctionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(XmlRootFunctionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(XmlExistsFunctionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(XmlParseFunctionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(XmlSerializeFunctionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(XmlTabletFunctionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(XmlTableColumnNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(CurrentCatalogSystemFunctionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(CurrentSchemaSystemFunctionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(CurrentUserSystemFunctionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(UserSystemFunctionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(SessionUserSystemFunctionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(CurrentDateFunctionNode node)
        {
            return new CodeContext() { Code = "DateTime.Now()" };
        }

        public override CodeContext Visit(CurrentTimeFunctionNode node)
        {
            return new CodeContext() { Code = "DateTime.Now()" };
        }

        public override CodeContext Visit(LocalTimeFunctionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(CurrentTimestampFunctionNode node)
        {
            return new CodeContext() { Code = "DateTime.Now()" };
        }

        public override CodeContext Visit(LocalTimestampFunctionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(BoolNode node)
        {
            return new CodeContext() { Code = node.Value.ToString().ToLower() };
        }

        public override CodeContext Visit(DefaultNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(Float4Node node)
        {
            return new CodeContext() { Code = node.Value.ToString() };
        }

        public override CodeContext Visit(Float8Node node)
        {
            return new CodeContext() { Code = node.Value.ToString() };
        }

        public override CodeContext Visit(Int8Node node)
        {
            return new CodeContext() { Code = node.Value.ToString() };
        }

        public override CodeContext Visit(IntNode node)
        {
            return new CodeContext() { Code = node.Value.ToString() };
        }

        public override CodeContext Visit(VarcharNode node)
        {
            var value = node.Value.Replace("'", "");
            return new CodeContext() { Code = "\"" + value + "\"" };
        }

        public override CodeContext Visit(SelectOpsNoParensNode node)
        {
            if (node.SelectOps != null)
            {
                var selectOpsCode = VisitNode(node.SelectOps);
                CodeContext statementCode = null;
                if (node.SelectStmt != null)
                    statementCode = VisitNode(node.SelectStmt);
                else if (node.SelectPrimary != null)
                    statementCode = VisitNode(node.SelectPrimary);
                if (node.Intersect)
                    return new CodeContext()
                    {
                        Code = $"({selectOpsCode.Code}).Intersect({statementCode.Code})",
                        UserFunctionCall = selectOpsCode.UserFunctionCall || statementCode.UserFunctionCall
                    };
                else if (node.Except)
                    return new CodeContext()
                    {
                        Code = $"({selectOpsCode.Code}).Except({statementCode.Code})",
                        UserFunctionCall = selectOpsCode.UserFunctionCall || statementCode.UserFunctionCall
                    };
                else
                {
                    if (node.Union && node.Qualifier.ToLower() == "distinct")
                        return new CodeContext()
                        {
                            Code = $"({selectOpsCode.Code}).Union({statementCode.Code})",
                            UserFunctionCall = selectOpsCode.UserFunctionCall || statementCode.UserFunctionCall
                        };
                    else if (node.Union && node.Qualifier.ToLower() == "all")
                        return new CodeContext()
                        {
                            Code = $"({selectOpsCode.Code}).Concat({statementCode.Code})",
                            UserFunctionCall = selectOpsCode.UserFunctionCall || statementCode.UserFunctionCall
                        };
                }
            }
            else if (node.SelectPrimary != null)
                return VisitNode(node.SelectPrimary);
            return null;
        }

        public override CodeContext Visit(SelectOpsNode node)
        {
            if (node.SelectStmt != null)
            {
                var selectCodeContext = VisitNode(node.SelectStmt);
                return new CodeContext()
                {
                    Code = "(" + selectCodeContext.Code + ")",
                    UserFunctionCall = selectCodeContext.UserFunctionCall
                };
            }
            else if (node.SelectPrimary != null)
                return VisitNode(node.SelectPrimary);
            else
            {
                var selectOpCodeContext1 = VisitNode(node.SelectOps[0]);
                var selectOpCodeContext2 = VisitNode(node.SelectOps[1]);
                var codeContext = new CodeContext()
                {
                    UserFunctionCall = selectOpCodeContext1.UserFunctionCall || selectOpCodeContext2.UserFunctionCall
                };
                if (node.Intersect)
                    codeContext.Code = $"({selectOpCodeContext1.Code}).Intersect({selectOpCodeContext2.Code})";
                else if (node.Union && node.SetQualifier.ToLower() == "distinct")
                    codeContext.Code = $"({selectOpCodeContext1.Code}).Intersect({selectOpCodeContext2.Code})";
                else if (node.Union && node.SetQualifier.ToLower() == "all")
                    codeContext.Code = $"({VisitNode(node.SelectOps[0]).Code}).Concat({VisitNode(node.SelectOps[1]).Code})";
                else
                    codeContext.Code = $"({VisitNode(node.SelectOps[0]).Code}).Except({VisitNode(node.SelectOps[1]).Code})";
                return codeContext;
            }
        }
        bool IfPredefinedFunction(BasicFunctionCallNode functionCall)
        {
            var predifined = new[] { "count", "sum", "max", "min", "avg" };
            return predifined.Contains(functionCall.SchemaQualifiednameNonType.IdentifierNonType.Text.ToLower());
        }
        string multiplySustitution;
        private List<(string, string)> FindJoinColumns(List<FromItemNode> nodes)
        {
            var result = new List<(string, string)>();
            foreach (var fromItem in nodes)
            {
                if (fromItem is FromPrimary1Node)
                {
                    var tableName = (fromItem as FromPrimary1Node).SchemaQualifield.Identifiers[0].Text;
                    var table = Catalog.Tables.Values.FirstOrDefault(t => t.Name == tableName);
                    if (table != null)
                    {
                        var field = table.Fields.FirstOrDefault(f => f.IsPrimaryKey);
                        if (field != null)
                            result.Add((fromItem.Alias, field.GenName));
                    }
                }
                else if (fromItem is FromPrimary2Node)
                {

                }
                else if (fromItem is FromPrimary3Node)
                {

                }
                else if (fromItem is FromPrimary4Node)
                {

                }
            }
            return result;
        }

        private CodeContext VisitQuery(SelectBodyNode node)
        {
            InQuery = true;
            var codeContext = new CodeContext();
            int expressionsIndex = 0;
            var result = "";
            var tmp = TablesAlias;
            var firstorDefault = false;
            TablesAlias = new Dictionary<string, string>();
            if (node.FromItems != null && node.FromItems.Count > 0)
            {
                firstorDefault = true;
                var fromItemsCode = "";
                var addWhere = "";
                var bnd = true;
                var generalAlias = "";
                if (node.FromItems.Count == 1)
                {
                    var fromItemsCC = VisitNode(node.FromItems[0]) as CodeContextFromItemResult;
                    multiplySustitution = node.FromItems[0].Alias;
                    generalAlias = node.FromItems[0].Alias;
                    if (fromItemsCC.Count == 1)
                    {
                        fromItemsCode = fromItemsCC.Code;
                        bnd = false;
                    }
                }
                if (bnd)
                {
                    var alias = GenVariable();
                    multiplySustitution = alias;
                    generalAlias = alias;
                    var joinExpressions = new List<ExpressionNode>();
                    var tablesName = new List<string>();
                    foreach (var fromItem in node.FromItems)
                    {
                        var fiCodeContext = VisitNode(fromItem) as CodeContextFromItemResult;
                        joinExpressions.AddRange(fiCodeContext.JionExpressions);
                        tablesName.AddRange(fiCodeContext.TablesName);
                        fromItemsCode += fiCodeContext.Code + " ";
                    }
                    var selectCode = "";
                    foreach (var tn in tablesName)
                    {
                        selectCode += (selectCode == "") ? TablesAlias[tn] : ", " + TablesAlias[tn];
                        if (!TablesAlias.ContainsKey(TablesAlias[tn]))
                            TablesAlias.Add(TablesAlias[tn], alias + "." + TablesAlias[tn]);
                        TablesAlias[tn] = alias + "." + TablesAlias[tn];

                    }

                    fromItemsCode = "from " + alias + " in (" + fromItemsCode + " select new {" + selectCode + "}) ";
                    foreach (var exp in joinExpressions)
                    {
                        var expCode = VisitNode(exp).Code;
                        addWhere += (addWhere == "") ? $"({expCode})" : " && " + $"({expCode})";
                    }
                }

                codeContext.Code = fromItemsCode + ((addWhere == "") ? "" : "where " + $"({addWhere})");
                if (node.Where)
                {
                    var expCodeContext = VisitNode(node.Expressions[expressionsIndex]);
                    if (!expCodeContext.UserFunctionCall)
                        codeContext.Code = codeContext.Code.Replace(".AsEnumerable()", "");
                    codeContext.Code += (addWhere == "") ? $" where {expCodeContext.Code}" : $" && {expCodeContext.Code}";
                }
                else
                {
                    codeContext.Code = codeContext.Code.Replace(".AsEnumerable()", "");
                }

                if (node.GroupByClause != null)
                {
                    var expCode = "";
                    var oneKey = node.GroupByClause.Expressions.Count() == 1;
                    var intoVariable = "_" + generalAlias;
                    foreach (var exp in node.GroupByClause.Expressions)
                    {
                        expCode = (expCode == "") ? VisitNode(exp).Code : ", " + VisitNode(exp).Code;

                        var indirection = exp as IndirectionVarNode;
                        if (indirection.Indirections.Count > 0 &&
                            indirection.Indirections[indirection.Indirections.Count - 1].ColLabel != null)
                            TablesAlias.Add(indirection.Indirections[indirection.Indirections.Count - 1]
                                .ColLabel.Text, "@.Key");
                    }
                    codeContext.Code += $" group {generalAlias} by {expCode} into {intoVariable}";
                    var newTA = new Dictionary<string, string>();
                    newTA.Add(generalAlias, "@" + intoVariable);
                    foreach (var ta in TablesAlias)
                    {
                        if (ta.Value.StartsWith(generalAlias))
                        {
                            if (expCode.Contains(ta.Value + "."))
                            {
                                if (oneKey)
                                    newTA.Add(ta.Key, "@" + intoVariable + ".Key");
                                else
                                    newTA.Add(ta.Key, ta.Value.Replace(generalAlias, intoVariable + ".Key"));
                            }
                            else
                                newTA.Add(ta.Key, ta.Value.Replace(generalAlias, intoVariable));
                        }
                        else
                            newTA.Add(ta.Key, ta.Value);
                    }
                    TablesAlias = newTA;
                }
            }
            var flag = true;
            CodeContext selectListCodeContext = null;
            if (node.SelectList.SelectSubLists.Count == 1)
            {
                if ((node.FromItems != null && node.FromItems.Count > 0) &&
                    node.SelectList.SelectSubLists[0].Expression is BasicFunctionCallNode &&
                    IfPredefinedFunction(node.SelectList.SelectSubLists[0].Expression as BasicFunctionCallNode))
                {
                    flag = false;
                    selectListCodeContext = VisitNode(node.SelectList);
                    if (selectListCodeContext.Code.Contains("Count"))
                    {
                        firstorDefault = false;
                        var argCC = VisitNode((node.SelectList.SelectSubLists[0].Expression as BasicFunctionCallNode)
                            .VexOrNamedNotations[0]).Code;
                        codeContext.Code = $"({codeContext.Code} select {argCC}).{selectListCodeContext.Code}";
                    }
                    else codeContext.Code = $"({codeContext.Code}).{selectListCodeContext.Code}";
                }
                else selectListCodeContext = VisitNode(node.SelectList);
            }
            else
            {
                var predF = false;

                foreach (var item in node.SelectList.SelectSubLists)
                {
                    if ((node.FromItems != null || node.FromItems.Count > 0) &&
                        item.Expression is BasicFunctionCallNode &&
                        IfPredefinedFunction(item.Expression as BasicFunctionCallNode))
                    {
                        predF = true;
                    }
                }
                if (!predF)
                {
                    selectListCodeContext = VisitNode(node.SelectList);
                }
                else
                {
                    var selection = "";
                    if (node.IntoTable?.Count <= 1)
                    {
                        foreach (var item in node.SelectList.SelectSubLists)
                            selection += (selection == "") ? VisitNode(item).Code : ", " + VisitNode(item).Code;
                    }
                    else
                    {
                        var index = 0;
                        foreach (var item in node.SelectList.SelectSubLists)
                        {
                            if (selection == "")
                                selection += (index < node.IntoTable.Count)?
                                    $"{ VisitNode(node.IntoTable[index++]).Code} = { VisitNode(item).Code}" : VisitNode(item).Code;
                            else
                                selection += (index < node.IntoTable.Count)?
                                    $",  {VisitNode(node.IntoTable[index++]).Code} = {VisitNode(item).Code}" : VisitNode(item).Code;
                        }
                    }
                    selectListCodeContext = new CodeContext() { Code = "x => new {" + selectListCodeContext + "}" };
                }
            }
            codeContext.UserFunctionCall |= selectListCodeContext.UserFunctionCall;
            if (codeContext.Code == null)
                codeContext.Code = selectListCodeContext.Code;
            if (flag && node.FromItems != null && node.FromItems.Count > 0)
                codeContext.Code = codeContext.Code + " select " + selectListCodeContext.Code;
            if (firstorDefault)
                codeContext.Code = $"({codeContext.Code}).FirstOrDefault()";
            TablesAlias = tmp;

            return codeContext;
        }
        public override CodeContext Visit(SelectPrimaryNode node)
        {
            var codeContext = VisitQuery(node);
            if (node.IntoTable != null)
            {
                if (node.IntoTable.Count == 1)
                {
                    var variableCodeContext = VisitNode(node.IntoTable[0]);
                    if (node.IntoTable[0].TypeReturn == node.TypeReturn)
                        codeContext.Code = $"{variableCodeContext.Code} = {codeContext.Code}";
                    else codeContext.Code = $"{variableCodeContext.Code} = ({node.IntoTable[0].TypeReturn}){codeContext.Code}";
                }
                else
                {
                    var variableName = GenVariable();
                    var variablesList = new List<CodeContext>();
                    foreach (var identifier in node.IntoTable)
                    {
                        var identifierCodeContext = VisitNode(identifier);
                        variablesList.Add(identifierCodeContext);
                    }
                    codeContext.Code = $"{variableName} = {codeContext.Code};\n";
                    foreach (var item in variablesList)
                    {
                        codeContext.Code += $"{item} = {variableName}.{item};\n"; 
                    }
                }
            }
            multiplySustitution = null;
            InQuery = false;
            return codeContext;
        }

        public override CodeContext Visit(SelectListNode node)
        {
            if (node.SelectSubLists.Count > 1)
            {
                var codeContext = new CodeContext();
                foreach (var item in node.SelectSubLists)
                {
                    var subListCodeContext = VisitNode(item);
                    if (codeContext.Code == null || codeContext.Code == "")
                        codeContext.Code = subListCodeContext.Code;
                    else
                        codeContext.Code += ", " + subListCodeContext.Code;
                }
                codeContext.Code = "new { " + codeContext.Code + " }";
                return codeContext;
            }
            else
                return VisitNode(node.SelectSubLists[0]);
        }

        public override CodeContext Visit(SelectSubListNode node)
        {
            return VisitNode(node.Expression);
        }

        public override CodeContext Visit(IdNode node)
        {
            var codeContext = new CodeContext();

            if (node.Text == "FOUND")
            {
                UseFoundVariable = true;
                codeContext.Code = node.Text;
            }
            else if (replaceList.Contains(node.Text))
            {
                codeContext.Code = "_" + node.Text.ToCamel();
            }
            else
            {
                if (node.Type == IdentifierType.Table)
                    codeContext.Code = node.Table.Name.ToPascal();
                else if (TablesAlias.ContainsKey(node.Text))
                {
                    if (TablesAlias[node.Text].StartsWith("@"))
                        codeContext.Code = new String(TablesAlias[node.Text].Skip(1).ToArray());
                    else
                        codeContext.Code = TablesAlias[node.Text];
                }
                else if (node.Type == IdentifierType.TableField && TablesAlias.ContainsKey(node.TableField.Table.Name))
                {
                    if (TablesAlias[node.TableField.Table.Name].StartsWith("@"))
                        codeContext.Code = new String(TablesAlias[node.Text].Skip(1).ToArray());
                    else
                        codeContext.Code = TablesAlias[node.TableField.Table.Name] + "." + node.Text.ToPascal();
                }
                else if (node.Type == IdentifierType.UdtField)
                    codeContext.Code = node.Text;
                else codeContext.Code = node.Text.ToCamel();
            }
            return codeContext;
        }

        public override CodeContext Visit(SchemaQualifieldNode node)
        {
            var codeContext = new CodeContext();
            foreach (var identifier in node.Identifiers)
            {
                var idCodeContext = VisitNode(identifier);
                if (codeContext.Code == null || codeContext.Code == "")
                {
                    if (identifier.Type == IdentifierType.Table)
                        codeContext.Code = "DbContext." + idCodeContext.Code.ToPascal() + "s.AsEnumerable()";
                    else codeContext.Code = idCodeContext.Code;
                }
                else codeContext.Code += "." + idCodeContext.Code;
                codeContext.UserFunctionCall |= idCodeContext.UserFunctionCall;
            }
            return codeContext;
        }

        public override CodeContext Visit(SchemaQualifiednameNonTypeNode node)
        {
            var codeContext = new CodeContext();
            if (node.Schema != null)
            {
                var schemaCodeContext = VisitNode(node.Schema);
                codeContext.Code = schemaCodeContext.Code.ToPascal();
                codeContext.UserFunctionCall |= schemaCodeContext.UserFunctionCall;
            }
            var identifierCodeContext = VisitNode(node.IdentifierNonType);
            if (codeContext.Code == null)
                codeContext.Code = identifierCodeContext.Code.ToPascal();
            else
                codeContext.Code = "." + identifierCodeContext.Code.ToPascal();
            codeContext.UserFunctionCall |= identifierCodeContext.UserFunctionCall;
            return codeContext;
        }

        public override CodeContext Visit(FrameBoundNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(FrameClauseNode node)
        {
            return new CodeContext();
        }

        string GenFromItem(string item, string alias)
        {
            if (item.StartsWith("from"))
            {
                if (item.Contains(" in "))
                    return item;
                else return "form " + alias + " in " + item.Split(new char[] { ' ' }, 2)[1];
            }
            else
            {
                if (item.Contains(" in "))
                    return "from" + item;
                else return "form " + alias + " in " + item;
            }
        }

        public override CodeContext Visit(FromItemSimpleNode node)
        {
            var itemCodeContext = VisitNode(node.Item) as CodeContextFromItemResult;
            return new CodeContextFromItemResult()
            {
                Code = itemCodeContext.Code,
                JionExpressions = itemCodeContext.JionExpressions,
                TablesName = itemCodeContext.TablesName,
                Count = itemCodeContext.Count
            };
        }

        public override CodeContext Visit(FromItemCrossJoinNode node)
        {
            var item1CodeContext = VisitNode(node.Item1) as CodeContextFromItemResult;
            var item2CodeContext = VisitNode(node.Item2) as CodeContextFromItemResult;
            return new CodeContextFromItemResult()
            {
                Code = item1CodeContext.Code + " " + item2CodeContext.Code,
                JionExpressions = item1CodeContext.JionExpressions
                    .Union(item2CodeContext.JionExpressions).ToList(),
                TablesName = item1CodeContext.TablesName
                    .Union(item2CodeContext.TablesName).ToList(),
                Count = item1CodeContext.Count + item2CodeContext.Count
            };
        }

        public override CodeContext Visit(FromItemOnExpressionNode node)
        {
            var item1CodeContext = VisitNode(node.Item1) as CodeContextFromItemResult;
            var item2CodeContext = VisitNode(node.Item2) as CodeContextFromItemResult;
            item1CodeContext.JionExpressions.Union(item2CodeContext.JionExpressions).ToList();
            item1CodeContext.JionExpressions.Add(node.Expression);
            return new CodeContextFromItemResult()
            {
                Code = item1CodeContext.Code + " " + item2CodeContext.Code,
                JionExpressions = item1CodeContext.JionExpressions,
                TablesName = item1CodeContext.TablesName
                    .Union(item2CodeContext.TablesName)
                    .ToList(),
                Count = item1CodeContext.Count + item2CodeContext.Count
            };
        }

        List<List<string>> FindTableNameByFieldName(List<string> fields)
        {
            var result = new List<List<string>>();
            foreach (var field in fields)
            {
                if (Catalog.Tables.Values.Any(t => t.Fields.Any(f => f.Name == field)))
                    result.Add(new List<string>(from t in Catalog.Tables.Values where t.Fields.Any(f => f.Name == field) select t.Name));
                else result.Add(null);
            }
            return result;
        }
        public override CodeContext Visit(FromItemUsingNode node)
        {
            var item1CodeContext = VisitNode(node.Item1) as CodeContextFromItemResult;
            var item2CodeContext = VisitNode(node.Item2) as CodeContextFromItemResult;

            return new CodeContextFromItemResult()
            {
                Code = item1CodeContext.Code + " " + item2CodeContext.Code,
                JionExpressions = item1CodeContext.JionExpressions
                    .Union(item2CodeContext.JionExpressions).ToList(),
                TablesName = item1CodeContext.TablesName
                    .Union(item2CodeContext.TablesName).ToList(),
                Count = item1CodeContext.Count + item2CodeContext.Count
            };
        }


        public override CodeContext Visit(FromItemNaturalNode node)
        {
            var item1CodeContext = VisitNode(node.Item1) as CodeContextFromItemResult;
            var item2CodeContext = VisitNode(node.Item2) as CodeContextFromItemResult;
            return new CodeContextFromItemResult()
            {
                Code = item1CodeContext.Code + " " + item2CodeContext.Code,
                JionExpressions = item1CodeContext.JionExpressions
                    .Union(item2CodeContext.JionExpressions).ToList(),
                TablesName = item1CodeContext.TablesName
                    .Union(item2CodeContext.TablesName).ToList(),
                Count = item1CodeContext.Count + item2CodeContext.Count
            };
        }

        public override CodeContext Visit(ExceptionStatementNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(ExecuteStatementNode node)
        {
            return new CodeContext()
            {
                Code = @"//Instruction not supported SP:" + Sp.Name + " Line:" + node.Line + " Column:" + node.Column
            };
        }

        public override CodeContext Visit(TransactionStatementNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(FromPrimary1Node node)
        {
            if (node.AliasClause != null)
            {
                node.Alias = node.AliasClause.Alias.Text;
            }
            else
            {
                node.Alias = (String.Concat(from x in node.SchemaQualifield.Identifiers select x.Text)).ToCamel();
            }
            var code = $"from {node.Alias} in {VisitNode(node.SchemaQualifield).Code}";
            if (!TablesAlias.ContainsKey(node.SchemaQualifield.TypeReturn))
                TablesAlias.Add(node.SchemaQualifield.TypeReturn, node.Alias);
            return new CodeContextFromItemResult()
            {
                Code = code,
                TablesName = new List<string>() { node.SchemaQualifield.TypeReturn },
                JionExpressions = new List<ExpressionNode>(),
                Count = 1
            };
        }

        public override CodeContext Visit(FromPrimary2Node node)
        {
            if (!TablesAlias.ContainsKey(node.AliasClause.Alias.Text.ToCamel()))
                TablesAlias.Add(node.AliasClause.Alias.Text.ToCamel(), node.TableSubquery.TypeReturn);
            return new CodeContextFromItemResult()
            {
                Code = $"from {node.AliasClause.Alias.Text.ToCamel()} in ({VisitNode(node.TableSubquery).Code})",
                TablesName = new List<string>() { node.AliasClause.Alias.Text.ToCamel() },
                JionExpressions = new List<ExpressionNode>(),
                Count = 1
            };
        }

        public override CodeContext Visit(FromPrimary3Node node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(FromPrimary4Node node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(FromFunctionColumnDefNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(AliasClauseNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(VexOrNamedNotationNode node)
        {
            return VisitNode(node.Expression);
        }

        public override CodeContext Visit(ArgumentNode node)
        {
            return new CodeContext()
            {
                Code = node.DataType.TypeReturn + " " + VisitNode(node.Identifier).Code
            };
        }

        public override CodeContext Visit(IndirectionIdentifierNode node)
        {
            var identifier = VisitNode(node.Identifier).Code.ToCamel();
            foreach (var item in node.Indirections)
            {
                identifier += VisitNode(item).Code;
            }
            return new CodeContext() { Code = identifier };
        }

        public override CodeContext Visit(IndirectionNode node)
        {
            if (node.ColLabel != null)
                return new CodeContext()
                {
                    Code = "." + VisitNode(node.ColLabel).Code.ToPascal()
                };
            else
            {
                if (node.Expressions != null && node.Expressions.Count > 0)
                {
                    var codeContext = new CodeContext();
                    foreach (var exp in node.Expressions)
                    {
                        var expCodeContext = VisitNode(exp);
                        if (codeContext.Code == null)
                            codeContext.Code = expCodeContext.Code;
                        else codeContext.Code += "," + expCodeContext.Code;
                        codeContext.UserFunctionCall |= expCodeContext.UserFunctionCall;
                    }
                    codeContext.Code = "[" + codeContext.Code + "]";
                    return codeContext;
                }
                else
                    return new CodeContext()
                    {
                        Code = "[]"
                    };
            }
        }

        public override CodeContext Visit(ReturnStmtNode node)
        {
            if (node.Stmt is PerformStmtNode)
            {
                var stmtCodeContext = VisitNode(node.Stmt);
                return new CodeContext()
                {
                    Code = $"{GetIdentation}return {stmtCodeContext.Code};",
                    UserFunctionCall = stmtCodeContext.UserFunctionCall
                };
            }
            else if (node.Expression != null)
            {
                UseFoundVariable = true;
                var expCodeContext = VisitNode(node.Expression);
                return new CodeContext()
                {
                    Code = $"{GetIdentation}FOUND = true;\n{GetIdentation}yield return {expCodeContext.Code};",
                    UserFunctionCall = expCodeContext.UserFunctionCall
                };
            }
            else if (node.Stmt != null)
            {
                UseFoundVariable = true;
                var stmtCodeContext = VisitNode(node.Stmt);
                stmtCodeContext.Code = stmtCodeContext.Code.Replace(".FirstOrDefault();", "");
                return new CodeContext()
                {
                    Code = $"{GetIdentation}FOUND = false;\n" +
                        $"{GetIdentation}foreach (var item in {stmtCodeContext.Code})\n" +
                        GetIdentation + "{\n" +
                        $"{GetIdentation}\tFOUND = true;\n" +
                        $"{GetIdentation}\tyield return item;\n" +
                        GetIdentation + "}",
                    UserFunctionCall = stmtCodeContext.UserFunctionCall
                };
            }
            else return new CodeContext() { Code = GetIdentation + "yield break;" };
        }

        public override CodeContext Visit(PerformStmtNode node)
        {
            var codeContext = VisitQuery(node);
            multiplySustitution = null;
            InQuery = false;
            if (node.SelectOps != null)
            {
                var selectOpsCode = VisitNode(node.SelectOps);
                if (node.Intersect)
                    return new CodeContext()
                    {
                        Code = $"({codeContext.Code}).Intersect({selectOpsCode.Code})",
                        UserFunctionCall = selectOpsCode.UserFunctionCall || selectOpsCode.UserFunctionCall
                    };
                else if (node.Except)
                    return new CodeContext()
                    {
                        Code = $"({codeContext.Code}).Except({selectOpsCode.Code})",
                        UserFunctionCall = selectOpsCode.UserFunctionCall || selectOpsCode.UserFunctionCall
                    };
                else if (node.SetQualifier1 != null && node.Union && node.SetQualifier1.ToLower() == "distinct")
                    return new CodeContext()
                    {
                        Code = $"({codeContext.Code}).Union({selectOpsCode.Code})",
                        UserFunctionCall = selectOpsCode.UserFunctionCall || selectOpsCode.UserFunctionCall
                    };
                else if (node.SetQualifier1 != null && node.Union && node.SetQualifier1.ToLower() == "all")
                    return new CodeContext()
                    {
                        Code = $"({codeContext.Code}).Concat({selectOpsCode.Code})",
                        UserFunctionCall = selectOpsCode.UserFunctionCall || selectOpsCode.UserFunctionCall
                    };
            }

            return codeContext;
        }

        public override CodeContext Visit(IfStmtNode node)
        {
            var codeContext = new CodeContext();
            for (int i = 0; i < node.Expressions.Count; i++)
            {
                var expCodeContext = VisitNode(node.Expressions[i]);
                if (codeContext.Code == null)
                    codeContext.Code = GetIdentation + $"if ({expCodeContext.Code})\n";
                else
                    codeContext.Code = $"else if ({expCodeContext.Code})\n";
                codeContext.UserFunctionCall |= expCodeContext.UserFunctionCall;

                codeContext.Code += GetIdentation + "{\n";
                identation++;
                foreach (var item in node.Statements[i])
                {
                    var itemCodeContext = VisitNode(item);
                    codeContext.Code += itemCodeContext.Code + "\n";
                    codeContext.UserFunctionCall |= itemCodeContext.UserFunctionCall;
                }
                identation--;
                codeContext.Code += GetIdentation + "}\n";
            }
            if (node.ElseStatements != null && node.ElseStatements.Count > 0)
            {
                codeContext.Code += "else\n" + "{\n";
                identation++;
                foreach (var item in node.ElseStatements)
                {
                    var elseCodeContext = VisitNode(item);
                    codeContext.Code += elseCodeContext.Code + "\n";
                    codeContext.UserFunctionCall |= elseCodeContext.UserFunctionCall;
                }
                identation--;
                codeContext.Code += "}";
            }
            return codeContext;
        }

        public override CodeContext Visit(AssingStmtNode node)
        {
            var varCodeContext = VisitNode(node.Var);
            CodeContext stmtCodeContext = null;
            var typeReturn = "";
            if (node.Stmt is PerformStmtNode)
            {
                var stmt = node.Stmt as PerformStmtNode;
                stmtCodeContext = VisitNode(stmt);
                typeReturn = stmt.TypeReturn;
            }
            else
            {
                var stmt = node.Stmt as SelectStmtNonParensNode;
                stmtCodeContext = VisitNode(stmt);
                typeReturn = stmt.TypeReturn;
            }
            var codeContext = new CodeContext()
            {
                UserFunctionCall = varCodeContext.UserFunctionCall || stmtCodeContext.UserFunctionCall
            };
            if (node.Var.TypeReturn == typeReturn)
                codeContext.Code = $"{GetIdentation}{varCodeContext.Code} = {stmtCodeContext.Code};";
            else
                codeContext.Code = $"{GetIdentation}{varCodeContext.Code} = ({node.Var.TypeReturn}){stmtCodeContext.Code};";
            return codeContext;

        }

        public override CodeContext Visit(VarNode node)
        {
            var nameOfVariable = (node.SchemaQualifield == null) ? VisitNode(node.Id).Code.ToCamel() :
                VisitNode(node.SchemaQualifield).Code;
            foreach (var expression in node.Expressions)
                nameOfVariable += $"[{VisitNode(expression).Code}]";
            return new CodeContext() { Code = nameOfVariable };
        }

        public override CodeContext Visit(RaiseMessageStatementNode node)
        {
            var msg = node.Message.Replace("'", "\"");
            return new CodeContext()
            {
                Code = $"{GetIdentation}throw new Exception({msg});"
            };
        }

        public override CodeContext Visit(AssertMessageStatementNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(IdentifierNonTypeNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(DeleteStmtPSqlNode node)
        {
            var tableName = VisitNode(node.DeleteTableName).Code;
            var alias = "";
            var tmp = TablesAlias;
            TablesAlias = new Dictionary<string, string>();
            if (node.Alias != null)
                alias = node.Alias.Text;

            else alias = GenVariable();

            TablesAlias.Add(tableName, "x");
            var conditionCode = "";
            if (node.Expression != null)
            {
                conditionCode = VisitNode(node.Expression).Code;
            }
            var codeContext = new CodeContext();
            codeContext.Code = $"{GetIdentation}foreach {alias} in {tableName}.Where(x => {conditionCode}).ToList();\n" +
                                 GetIdentation + "{\n" +
                                 GetIdentation + "\t" + $"DbContext.Remove({alias});\n" +
                                 GetIdentation + "}\n" +
                                 GetIdentation + "DbContext.SaveChanges();";
            return codeContext;
        }

        public override CodeContext Visit(OpCharsNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(LoopStmtNode node)
        {
            var codeContext = new CodeContext();
            if (node.Exit)
            {
                if (node.Expression != null)
                    codeContext.Code = $"{GetIdentation}if({VisitNode(node.Expression).Code}) break;";
                else codeContext.Code = $"{GetIdentation}break;";
            }
            else if (node.Continue)
            {
                if (node.Expression != null)
                    codeContext.Code = $"{GetIdentation}if({VisitNode(node.Expression).Code}) continue;";
                else codeContext.Code = $"{GetIdentation}continue;";
            }
            else
            {
                if (node.LoopStart != null)
                {
                    var startLoopCodeContext = VisitNode(node.LoopStart);
                    codeContext.Code = startLoopCodeContext.Code + "\n";
                }
                else
                    codeContext.Code = $"while (true)\n";

                codeContext.Code += GetIdentation + "{";
                identation++;
                if (node.LoopStart is ForAliasLoopNode)
                {
                    var id = (node.LoopStart as ForAliasLoopNode);
                    codeContext.Code +=
                        "\n" + GetIdentation + id.Identifier.Text + " = " + "for_" + id.Identifier.Text + ";";
                }
                else if (node.LoopStart is ForCursorLoopNode)
                {
                    var id = (node.LoopStart as ForCursorLoopNode);
                    codeContext.Code +=
                        "\n" + GetIdentation + id.Cursor.Text + " = " + "for_" + id.Cursor.Text + ";";
                }
                else if (node.LoopStart is ForIdListLoopNode)
                {
                    var id = (node.LoopStart as ForIdListLoopNode);
                    codeContext.Code +=
                        "\n" + GetIdentation + id.Identifiers[0].Text + " = " + "for_" + id.Identifiers[0].Text + ";";
                }
                else if (node.LoopStart is ForeachLoopNode)
                {
                    var id = (node.LoopStart as ForeachLoopNode);
                    codeContext.Code +=
                        "\n" + GetIdentation + id.Identifiers[0].Text + " = " + "for_" + id.Identifiers[0].Text + ";";
                }
                foreach (var stmt in node.Statemets)
                {
                    codeContext.Code += $"\n{VisitNode(stmt).Code}";
                }
                identation--;
                codeContext.Code += "\n" + GetIdentation + "}";
            }
            return codeContext;
        }

        public override CodeContext Visit(OpNode node)
        {
            return new CodeContext() { Code = node.Operator };
        }

        public override CodeContext Visit(ConflictObjectNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(ConflictActionNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(DeclareStatementNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(ModularTypeDeclarationNode node)
        {
            return new CodeContext() { Code = node.TypeReturn };
        }

        public override CodeContext Visit(ModularRowTypeDeclarationNode node)
        {
            return new CodeContext() { Code = node.TypeReturn.ToPascal() };
        }

        public override CodeContext Visit(AnalizeModeNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(WhileLoopNode node)
        {
            return new CodeContext()
            {
                Code = $"while ({VisitNode(node.Expressions[0]).Code})"
            };
        }

        public override CodeContext Visit(ForAliasLoopNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(ForIdListLoopNode node)
        {
            UseFoundVariable = true;
            var identifiersCodeContexts = new List<CodeContext>();
            foreach (var identifier in node.Identifiers)
                identifiersCodeContexts.Add(VisitNode(identifier));
            var stmtCodeContext = VisitNode(node.Stmt);
            var codeContext = new CodeContext()
            {
                Code = $"foreach (var for_{identifiersCodeContexts[0].Code} in {stmtCodeContext.Code.Replace(".FirstOrDefault();", "")})"
            };
            return codeContext;
        }

        public override CodeContext Visit(ForCursorLoopNode node)
        {
            UseFoundVariable = true;
            var cursorCodeContext = VisitNode(node.Cursor);
            var identifierCodeContext = VisitNode(node.Identifier);
            var codeContext = new CodeContext();
            var options = "";
            if (node.Options != null)
                foreach (var op in node.Options)
                    options = (options == "") ? VisitNode(op).Code : ", " + VisitNode(op).Code;
            codeContext.Code = $"foreach (var for_{cursorCodeContext.Code} in {identifierCodeContext.Code}{options})";
            return codeContext;
        }

        public override CodeContext Visit(ForeachLoopNode node)
        {
            UseFoundVariable = true;
            var codeContext = new CodeContext();
            var idsCodeContext = new List<CodeContext>();
            foreach (var id in node.Identifiers)
                idsCodeContext.Add(VisitNode(id));
            var expCodeContext = VisitNode(node.Expression);
            codeContext.Code = $"foreach (var for_{idsCodeContext[0].Code} in {expCodeContext.Code})";
            return codeContext;
        }

        public override CodeContext Visit(InsertStmtPSqlNode node)
        {
            try
            {
                var codeContext = new CodeContext();
                var table = node.InsertTableName.Identifiers[0].Text.ToPascal();
                var tableName = GenVariable();
                ValuesStmtNode valuesStmt = null;
                if (node.SelectStmt?.SelectOps?.SelectPrimary != null)
                    valuesStmt = node.SelectStmt.SelectOps.SelectPrimary as ValuesStmtNode;
                var valuesValues = new List<CodeContext>();
                var columns = new List<CodeContext>();
                if(node.InsertColumns != null)
                    foreach (var c in node.InsertColumns)
                       columns.Add(VisitNode(c));
                foreach (var v in valuesStmt?.Values)
                    valuesValues.Add(VisitNode(v));
                codeContext.Code = $"{GetIdentation}var {tableName} = new {table}()\n" +
                                    GetIdentation + "{\n";
                if (valuesValues.Count >= columns.Count)
                    for (int i = 0; i < columns.Count; i++)
                        codeContext.Code += $"{GetIdentation}\t{columns[i].Code.ToPascal()} = {valuesValues[i].Code}\n";

                codeContext.Code += GetIdentation + "}\n";
                codeContext.Code += $"{GetIdentation}DbContext.Add({tableName});\n";
                codeContext.Code += $"{GetIdentation}DbContext.SaveChanges();\n";
                return codeContext;
            }
            catch (Exception)
            {
                return new CodeContext();
            }
             
        }

        public override CodeContext Visit(AfterOpsFetchNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(AfterOpsForNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(AfterOpsLimitNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(AfterOpsOffsetNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(OrderByClauseNode node)
        {
            var codeContext = new CodeContext();
            var alias = "";
            var expsCode = new List<CodeContext>();
            foreach (var orderBy in node.SortSpecifiers)
            {
                var indirectionVar = VisitNode(orderBy);
                expsCode.Add(indirectionVar);
                if (alias == "")
                {
                    var alias_name = indirectionVar.Code.Split(new char[] { '.' }, 2);
                    if (alias_name[1] != null && alias_name[1] != "")
                    {
                        indirectionVar.Code = alias_name[1];
                        alias = alias_name[0];
                    }
                }
            }

            if (alias == "") alias = "x";

            if (expsCode.Count > 1)
            {
                codeContext.Code = "OrderBy({" + alias + "} => new {";
                var first = true;
                foreach (var exp in expsCode)
                {
                    if (first)
                    {
                        codeContext.Code += alias + "." + exp.Code;
                        first = false;
                    }
                    else
                        codeContext.Code += ", " + alias + "." + exp.Code;
                }
                codeContext.Code += "})";
            }
            else
            {
                codeContext.Code = "OrderBy(" + alias + " => ";
                codeContext.Code += alias + "." + expsCode[0].Code + ")";
            }
            return codeContext;
        }

        public override CodeContext Visit(SortSpecifierNode node)
        {
            var codeContext = new CodeContext();
            if (node.Key != null)
                codeContext.Code = VisitNode(node.Key).Code;
            return codeContext;
        }

        public override CodeContext Visit(OrderSpecificationNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(AllOpRefNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(ExecuteStmtNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(UpdateStmtPSqlNode node)
        {
            try
            {
                var codeContext = new CodeContext();
                var generalAlias = "";
                var table = VisitNode(node.UpdateTableName).Code;
                var alias = "";
                if (node.Alias != null)
                    alias = VisitNode(node.Alias).Code;
                else
                    alias = node.UpdateTableName.Identifiers[0].Text.ToCamel();
                InQuery = true;
                var query = "";

                var tmp = TablesAlias;
                TablesAlias = new Dictionary<string, string>();

                var fromItemsCodeContext = new CodeContextFromItemResult();
                foreach (var item in node.FromItems)
                {
                    var itemCodeContext = VisitNode(item) as CodeContextFromItemResult;
                    fromItemsCodeContext.Code += $" {itemCodeContext.Code}";
                    fromItemsCodeContext.JionExpressions.AddRange(itemCodeContext.JionExpressions);
                    fromItemsCodeContext.Count += itemCodeContext.Count;
                    fromItemsCodeContext.TablesName.AddRange(itemCodeContext.TablesName);
                }
                CodeContext expCodeContext = null;
                if(node.Expression != null)
                    expCodeContext = VisitNode(node.Expression);

                if (expCodeContext != null && !expCodeContext.UserFunctionCall)
                {
                    table = table.Replace(".AsEnumerable()", "");
                    fromItemsCodeContext.Code = fromItemsCodeContext.Code.Replace(".AsEnumerable()", "");
                }
                
                if (fromItemsCodeContext.Count == 0)
                {
                    generalAlias = alias;
                    TablesAlias.Add(alias, alias);
                    if (expCodeContext == null)
                        query = $"from {generalAlias} in {table}";
                    else
                        query = $"from {generalAlias} in {table} where {expCodeContext} select {generalAlias}";
                }
                else
                {
                    generalAlias = GenVariable();

                    var fromCode = $"from {alias} in {table} {fromItemsCodeContext.Code}";
                    var firtSelect = "";
                    foreach (var tablename in fromItemsCodeContext.TablesName)
                    {
                        firtSelect = (firtSelect == "") ? firtSelect : ", " + firtSelect;
                        if (!TablesAlias.ContainsKey(TablesAlias[tablename]))
                            TablesAlias.Add(TablesAlias[tablename], generalAlias + "." + TablesAlias[tablename]);
                        TablesAlias[tablename] = generalAlias + "." + TablesAlias[tablename];
                    }

                    query = $"from {generalAlias} in ({fromCode}"+" select new {"+ firtSelect +"})";

                    var whereCode = "";
                    var joinExpCode = "";
                    if (fromItemsCodeContext.JionExpressions?.Count > 0)
                    {
                        foreach (var exp in fromItemsCodeContext.JionExpressions)
                            joinExpCode = (joinExpCode != "") ? $"{VisitNode(exp).Code}" : $" &&{VisitNode(exp).Code}";
                    }
                    if (joinExpCode != "")
                    {
                        whereCode = $" where ({joinExpCode})";
                        if (expCodeContext.Code != null && expCodeContext.Code != "")
                            whereCode += $" && {expCodeContext.Code}";
                    }
                    else
                    {
                        if (expCodeContext.Code != null && expCodeContext.Code != "")
                            whereCode += $" where {expCodeContext.Code}";
                    }
                    query += whereCode + $" select {generalAlias}";
                    var newTA = new Dictionary<string, string>();
                    var newAlias = GenVariable();
                    foreach (var item in TablesAlias)
                    {
                        if (item.Value.StartsWith(generalAlias))
                            newTA.Add(item.Key, item.Value.Replace(generalAlias, newAlias));
                        else newTA.Add(item.Key, item.Value);
                    }
                    TablesAlias = newTA;
                    var statemant = $"{GetIdentation}foreach(var {newAlias} in {query})\n" +
                                       GetIdentation + "{\n";
                    foreach (var set in node.UpdateSets)
                    {
                        statemant += VisitNode(set) + "\n";
                    }
                    statemant += GetIdentation + "}\n";
                    statemant += GetIdentation + "SaveChanges()";
                }
                InQuery = false;
                TablesAlias = tmp;
                return codeContext;
            }
            catch (Exception)
            {
                return new CodeContext();
            }
        }

        public override CodeContext Visit(ValuesStmtNode node)
        {
            return new CodeContext();
        }

        public override CodeContext Visit(UpdateSetNode node)
        {
            var codeContext = new CodeContext();
            if(node.Columns.Count <= node.Values.Count)
                for (int i = 0; i < node.Columns.Count; i++)
                {
                    codeContext.Code += $"{VisitNode(node.Columns[i]).Code} = {VisitNode(node.Values[i]).Code},\n";
                }
            return codeContext;
        }

        public override CodeContext Visit(ValuesValuesNode node)
        {
            var expsCode = "";
            foreach (var item in node.Expressions)
                expsCode = (expsCode == "") ? VisitNode(item).Code : " ," + VisitNode(item).Code;
            return new CodeContext() { Code = $"({expsCode})" };
        }

        public override CodeContext Visit(CursorStatementNode node)
        {
            var codeContext = new CodeContext();
            if (node.Open)
            {
                var varCodeContext = VisitNode(node.Var);
                codeContext.Code = $"{GetIdentation}{varCodeContext.Code.ToCamel()} = ";
                var optionsCode = "";
                foreach (var option in node.Options)
                    optionsCode = (optionsCode == "") ? VisitNode(option).Code : ", " + VisitNode(option).Code;
                codeContext.Code += $"{varCodeContext.Code.ToPascal()}({optionsCode}).GetEnumerator();";
            }
            else if (node.Move)
            {
                var cursorInstance = VisitNode(node.Var).Code.ToCamel();
                codeContext.Code = $"{GetIdentation}FOUND = {cursorInstance}.MoveNext();\n";
            }
            else if (node.Fetch)
            {
                var cursorInstance = VisitNode(node.Var).Code.ToCamel();
                codeContext.Code = $"{GetIdentation}FOUND = {cursorInstance}.MoveNext();\n";
                var variables = new List<string>();
                foreach (var item in node.IntoTable)
                    variables.Add(VisitNode(item).Code);
                if (variables.Count == 1)
                    codeContext.Code += $"{GetIdentation}{variables[0]} = {cursorInstance}.Current;";
                else
                    foreach (var item in variables)
                        codeContext.Code += $"{GetIdentation}{item} = {cursorInstance}.Current;";
            }
            else if (node.Close)
            {
                var varCodeContext = VisitNode(node.Var);
                codeContext.Code = $"{varCodeContext.Code.ToCamel()}.Dispose();";
            }
            return codeContext;
        }

        public override CodeContext Visit(OptionNode node)
        {
            return VisitNode(node.Expression);
        }
    }
}
