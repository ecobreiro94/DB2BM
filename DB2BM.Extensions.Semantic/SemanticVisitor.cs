using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DB2BM.Abstractions;
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
using DB2BM.Abstractions.Entities.UserDefined;
using DB2BM.Abstractions.Interfaces;
using DB2BM.Abstractions.Visitors;
using Newtonsoft.Json;

namespace DB2BM.Extensions.Semantic
{
    public class SemanticVisitor : ASTVisitor<List<SemanticResult>>
    {
        StoreProcedure Sp;
        Dictionary<string, (string, IdentifierType, object) > VariablesTypes;
        Dictionary<string, (List<string>, string)> CursorTypes;
        List<(string, List<string>, string)> PredefinedFunctions = new List<(string, List<string>, string)>()
        {
            { ("SUM", new List<string>(){"int"}, "int") },
            { ("SUM", new List<string>(){"short"}, "short") },
            { ("SUM", new List<string>(){"long"}, "long") },
            { ("SUM", new List<string>(){"decimal"}, "decimal") },
            { ("SUM", new List<string>(){"float"}, "float") },
            { ("SUM", new List<string>(){"double"}, "double") },
            { ("SUM", new List<string>(){"TimeSpan"}, "TimeSpan") }
        };

        private static Dictionary<string, string> typeMapper;
        public static Dictionary<string, string> TypesMapper
        {
            get
            {
                if (typeMapper == null)
                    typeMapper = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("typesMapper.json"));
                return typeMapper;
            }
        }

        DatabaseCatalog Catalog;

        public SemanticVisitor(DatabaseCatalog catalog, StoreProcedure sp)
        {
            Catalog = catalog;
            Sp = sp;
            VariablesTypes = new Dictionary<string, (string, IdentifierType, object)>()
            {
                { "FOUND", ("bool", IdentifierType.Variable, null) }
            };
            foreach (var param in sp.Params)
                VariablesTypes.Add(param.Name, (param.DestinyType, IdentifierType.Variable, null));
            CursorTypes = new Dictionary<string, (List<string>, string)>();
        }
        SemanticVisitor(DatabaseCatalog catalog, StoreProcedure sp,
            Dictionary<string, (string, IdentifierType, object)> variablesTypes,
            Dictionary<string, (List<string>, string)> cursorInfo)
        {
            Catalog = catalog;
            Sp = sp;
            VariablesTypes = variablesTypes;
            CursorTypes = new Dictionary<string, (List<string>, string)>(cursorInfo);
        }



        public override List<SemanticResult> Visit(FunctionBlockNode node)
        {
            var errors = new List<SemanticResult>();
            if (node.Declarations != null)
                foreach (var dec in node.Declarations)
                    errors.AddRange(VisitNode(dec));
            if (node.Statements != null)
                foreach (var stmt in node.Statements)
                    errors.AddRange(VisitNode(stmt));
            if (node.ExceptionStatement != null)
                errors.AddRange(VisitNode(node.ExceptionStatement));
            return errors;
        }

        public override List<SemanticResult> Visit(DeclarationNode node)
        {
            var errors = new List<SemanticResult>();
            if (VariablesTypes.ContainsKey(node.Identifier.Text))
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column)));
            if (node.TypeDeclaration is AliasDeclarationNode)
            {
                var instanceIdentifier = (node.TypeDeclaration as AliasDeclarationNode).Identifier;
                var instanceName = "";
                if (instanceIdentifier is DollarNumberNode)
                {
                    int number = int.Parse(new String(instanceIdentifier.Text.Skip(1).ToArray())) - 1;
                    instanceName = Sp.Params[number].Name;
                }
                else
                    instanceName = instanceIdentifier.Text;
                if (!VariablesTypes.ContainsKey(instanceName))
                    errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name,
                        instanceIdentifier.Line, instanceIdentifier.Column)));
                else
                    VariablesTypes.Add(node.Identifier.Text, VariablesTypes[instanceName]);
            }
            else if (node.TypeDeclaration is OrdinalTypeDeclarationNode)
            {
                var typeDeclaration = node.TypeDeclaration as OrdinalTypeDeclarationNode;
                errors.AddRange(VisitNode(typeDeclaration.DataType));
                if (typeDeclaration.Expression != null)
                {
                    errors.AddRange(VisitNode(typeDeclaration.Expression));
                    if (!(typeDeclaration.DataType is ModularRowTypeDeclarationNode))
                    {
                        if(!(TypeInfo.TypesInfo.ContainsKey(typeDeclaration.DataType.TypeReturn) &&
                           TypeInfo.TypesInfo.ContainsKey(typeDeclaration.Expression.TypeReturn) &&
                           TypeInfo.TypesInfo[typeDeclaration.DataType.TypeReturn].GeneralType == TypeInfo.TypesInfo[typeDeclaration.Expression.TypeReturn].GeneralType) &&
                           (typeDeclaration.Expression.TypeReturn != typeDeclaration.DataType.TypeReturn))
                            errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                                Sp.Name, typeDeclaration.Expression.Line, typeDeclaration.Expression.Column)));
                    }
                }
                VariablesTypes.Add(node.Identifier.Text, (typeDeclaration.DataType.TypeReturn, IdentifierType.Variable, null));
            }
            else if (node.TypeDeclaration is CursorDeclarationNode)
            {
                var cursorDeclaration = node.TypeDeclaration as CursorDeclarationNode;
                var newVariablesTypes = new Dictionary<string, (string, IdentifierType, object)>(VariablesTypes);
                if (!VariablesTypes.ContainsKey("FOUND"))
                    VariablesTypes.Add("FOUND", ("bool", IdentifierType.Variable, null));
                List<string> cursorParamsTypes = new List<string>();
                foreach (var arg in cursorDeclaration.ArgumentList)
                {
                    errors.AddRange(VisitNode(arg.DataType));
                    if (!newVariablesTypes.ContainsKey(arg.Identifier.Text))
                    {
                        newVariablesTypes.Add(arg.Identifier.Text, (arg.DataType.TypeReturn, IdentifierType.Variable, null));
                        cursorParamsTypes.Add(arg.DataType.TypeReturn);
                    }
                }
                var newSemanticVisitor = new SemanticVisitor(Catalog, Sp,
                    new Dictionary<string, (string, IdentifierType, object)>(newVariablesTypes),
                    CursorTypes);
                errors.AddRange(newSemanticVisitor.VisitNode(cursorDeclaration.SelectStmt));
                CursorTypes.Add(node.Identifier.Text, (cursorParamsTypes, cursorDeclaration.SelectStmt.TypeReturn));
            }
            return errors;
        }

        public override List<SemanticResult> Visit(AliasDeclarationNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(CursorDeclarationNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(OrdinalTypeDeclarationNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(OtherTypeNode node)
        {
            var typeName = node.SchemaQualifiednameNonType.IdentifierNonType.Text.ToLower();
            if (TypesMapper.ContainsKey(typeName))
                node.TypeReturn = TypesMapper[typeName];
            else
                node.TypeReturn = typeName;
            var errors = new List<SemanticResult>();
            foreach (var exp in node.Expressions)
                errors.AddRange(VisitNode(exp));
            return errors;
        }

        public override List<SemanticResult> Visit(OtherOpBinaryNode node)
        {
            var errors = (node.LeftOperand != null) ?
                VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList() :
                VisitNode(node.RightOperand);
            node.TypeReturn = (node.LeftOperand != null) ?
                SelectType(node.LeftOperand.TypeReturn, node.RightOperand.TypeReturn) :
                node.RightOperand.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(DataTypeNode node)
        {
            var errors = VisitNode(node.Type);
            node.TypeReturn = node.Type.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(NCharTypeNode node)
        {
            node.TypeReturn = "string";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(NCharVaryingTypeNode node)
        {
            node.TypeReturn = "string";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(NumericTypeNode node)
        {
            node.TypeReturn = "decimal";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(RealTypeNode node)
        {
            node.TypeReturn = "float";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(BigintTypeNode node)
        {
            node.TypeReturn = "long";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(SmallintTypeNode node)
        {
            node.TypeReturn = "short";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(BitTypeNode node)
        {
            node.TypeReturn = "short";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(TimeTypeNode node)
        {
            node.TypeReturn = "TimeSpan";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(BitVaryingTypeNode node)
        {
            node.TypeReturn = "string";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(VarcharTypeNode node)
        {
            node.TypeReturn = "string";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(BooleanTypeNode node)
        {
            node.TypeReturn = "bool";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(DecTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(DecimalTypeNode node)
        {
            node.TypeReturn = "decimal";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(DollarNumberNode node)
        {
            var errors = new List<SemanticResult>();
            int index;
            if (!int.TryParse(new string(node.Text.Skip(1).ToArray()), out index))
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column)));
            node.TypeReturn = Sp.Params[--index].DestinyType;
            return errors;
        }

        public override List<SemanticResult> Visit(DoublePrecisionTypeNode node)
        {
            node.TypeReturn = "double";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(AtTimeZoneNode node)
        {
            node.TypeReturn = "TimeSpan";
            return VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
        }

        public override List<SemanticResult> Visit(FloatTypeNode node)
        {
            node.TypeReturn = "float";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(IntTypeNode node)
        {
            node.TypeReturn = "int";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(IntegerTypeNode node)
        {
            node.TypeReturn = "int";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(IntervalTypeNode node)
        {
            node.TypeReturn = "interval";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(CharTypeNode node)
        {
            node.TypeReturn = "string";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(CharVaryingTypeNode node)
        {
            node.TypeReturn = "string";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(AddAnalizeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(AddClusterNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(AddDeallocatteNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(AddListenNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(AddPrepareNode node)
        {
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(AddReassignNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(AddRefreshNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(AddReindexNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(AddResetNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(AddUnlistenNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(CopyStmtNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(ExplainStmtNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(ShowStmtNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(CallFunctionCallNode node)
        {
            return VisitNode(node.FunctionCall);
        }

        public override List<SemanticResult> Visit(WithClauseNode node)
        {
            var errors = new List<SemanticResult>();
            foreach (var withQuery in node.WithQuerys)
            {
                errors.AddRange(VisitNode(withQuery));  
            }
            return errors;
        }

        public override List<SemanticResult> Visit(WithQueryNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(CaseStmtNode node)
        {
            var errors = new List<SemanticResult>();
            if (node.HeaderExpression != null)
                errors.AddRange(VisitNode(node.HeaderExpression));
            foreach (var c in node.Cases)
            {
                foreach (var exp in c.Expressions)
                    errors.AddRange(VisitNode(exp));
                foreach (var statement in c.Stmts)
                    errors.AddRange(VisitNode(statement));
            }
            if (node.ElseStmts != null)
                foreach (var statement in node.ElseStmts)
                    errors.AddRange(VisitNode(statement));
            return errors;
        }

        public override List<SemanticResult> Visit(WindowsDefinitionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(NullOrderingNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(CastExpressionNode node)
        {
            var errors = VisitNode(node.DataType).Concat(VisitNode(node.Expression)).ToList();
            node.TypeReturn = node.DataType.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(IsNullNode node)
        {
            node.TypeReturn = "bool";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(IsNotNullNode node)
        {
            node.TypeReturn = "bool";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(BetweenNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(InNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(OfNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(SelectStatementNode node)
        {
            var errors = new List<SemanticResult>();

            if (node.WithClause != null)
                errors.Add(new ErrorResult(String.Format("No Suport SP:{0} Line:{1} Column:{3}", Sp.Name, node.WithClause.Line, node.WithClause.Column)));
                //    errors.AddRange(VisitNode(node.WithClause));
            errors.AddRange(VisitNode(node.SelectOps));
            if (node.SelectOps.SelectPrimary != null)
                node.TypeReturn = node.SelectOps.SelectPrimary.TypeReturn;
            else if (node.SelectOps.SelectStmt != null)
                node.TypeReturn = node.SelectOps.SelectStmt.TypeReturn;
            foreach (var afterOps in node.AfterOps)
            {
                errors.AddRange(VisitNode(afterOps));
            }
            return errors;
        }

        public override List<SemanticResult> Visit(SelectStmtNonParensNode node)
        {
            var errors = new List<SemanticResult>();

            if (node.WithClause != null)
                errors.AddRange(VisitNode(node.WithClause));
            errors.AddRange(VisitNode(node.SelectOps));
            if (node.AfterOps != null)
                foreach (var afterOp in node.AfterOps)
                    errors.AddRange(VisitNode(afterOp));
            node.TypeReturn = node.SelectOps.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(TruncateStmtNode node)
        {
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(IsTrueNode node)
        {
            node.TypeReturn = "bool";
            return VisitNode(node.Operand);
        }

        public override List<SemanticResult> Visit(IsNotTrueNode node)
        {
            node.TypeReturn = "bool";
            return VisitNode(node.Operand);
        }

        public override List<SemanticResult> Visit(IsFalseNode node)
        {
            node.TypeReturn = "bool";
            return VisitNode(node.Operand);
        }

        public override List<SemanticResult> Visit(IsNotFalseNode node)
        {
            node.TypeReturn = "bool";
            return VisitNode(node.Operand);
        }

        public override List<SemanticResult> Visit(CollateNode node)
        {
            throw new NotImplementedException();
        }
        private List<SemanticResult> SemanticCheckLikeNodes(LikeNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            node.TypeReturn = "bool";
            if (node.LeftOperand.TypeReturn != "string" && node.LeftOperand.TypeReturn != "object")
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column{2}", Sp.Name, node.LeftOperand.Line, node.LeftOperand.Column)));
            if (node.RightOperand.TypeReturn != "string" && node.RightOperand.TypeReturn != "object")
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column{2}", Sp.Name, node.RightOperand.Line, node.RightOperand.Column)));
            return errors;
        }
        public override List<SemanticResult> Visit(LikeBinaryNode node)
        {
            return SemanticCheckLikeNodes(node);
        }

        public override List<SemanticResult> Visit(ILikeBinaryNode node)
        {
            return SemanticCheckLikeNodes(node);
        }

        public override List<SemanticResult> Visit(NotLikeBinaryNode node)
        {
            return SemanticCheckLikeNodes(node);
        }

        public override List<SemanticResult> Visit(NotILikeBinaryNode node)
        {
            return SemanticCheckLikeNodes(node);
        }

        public override List<SemanticResult> Visit(SimilarToBinaryNode node)
        {
            return SemanticCheckLikeNodes(node);
        }

        public override List<SemanticResult> Visit(NotSimilarToBinaryNode node)
        {
            return SemanticCheckLikeNodes(node);
        }

        public override List<SemanticResult> Visit(NotNode node)
        {
            node.TypeReturn = "bool";
            return VisitNode(node.Operand);
        }

        public override List<SemanticResult> Visit(AbsoluteValueNode node)
        {
            node.TypeReturn = "int";
            return VisitNode(node.Operand);
        }

        public override List<SemanticResult> Visit(BitwiseNotNode node)
        {
            var errors = VisitNode(node.Operand);
            node.TypeReturn = node.Operand.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(FactorialNode node)
        {
            var errors = VisitNode(node.Operand);
            node.TypeReturn = node.Operand.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(OtherOpUnaryNode node)
        {
            var errors = VisitNode(node.Operand);
            node.TypeReturn = node.Operand.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(IsDocumentNode node)
        {
            node.TypeReturn = "bool";
            return VisitNode(node.Operand);
        }

        public override List<SemanticResult> Visit(IsNotDocumentNode node)
        {
            node.TypeReturn = "bool";
            return VisitNode(node.Operand);
        }

        public override List<SemanticResult> Visit(IsUnknownNode node)
        {
            node.TypeReturn = "bool";
            return VisitNode(node.Operand);
        }

        public override List<SemanticResult> Visit(IsNotUnknownNode node)
        {
            node.TypeReturn = "bool";
            return VisitNode(node.Operand);
        }

        public override List<SemanticResult> Visit(BaseTypeCoercionNode node)
        {
            var errors = new List<SemanticResult>();
            errors.AddRange(VisitNode(node.DataType));
            node.TypeReturn = node.DataType.TypeReturn;
            return errors;

        }

        public override List<SemanticResult> Visit(IntervalTypeCoercionNode node)
        {
            node.TypeReturn = "interval";
            return new List<SemanticResult>();
        }


        private List<SemanticResult> SemanticCheckComparisonBynaryOperators(ComparisonBinaryExpressionNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            node.TypeReturn = "bool";
            if (TypeInfo.TypesInfo.ContainsKey(node.RightOperand.TypeReturn) &&
                TypeInfo.TypesInfo.ContainsKey(node.LeftOperand.TypeReturn) &&
                TypeInfo.TypesInfo[node.RightOperand.TypeReturn].GeneralType != TypeInfo.TypesInfo[node.LeftOperand.TypeReturn].GeneralType)
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line{1} Column{2}", Sp.Name, node.Line, node.Column)));
            else if ((Catalog.Tables.Values.Any(t => t.Name == node.RightOperand.TypeReturn) ||
                Catalog.Tables.Values.Any(t => t.Name == node.LeftOperand.TypeReturn)) &&
                node.LeftOperand.TypeReturn != node.RightOperand.TypeReturn)
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line{1} Column{2}", Sp.Name, node.Line, node.Column)));
            else if ((Catalog.UserDefinedTypes.Values.Any(t => t.TypeName == node.RightOperand.TypeReturn) ||
                Catalog.UserDefinedTypes.Values.Any(t => t.TypeName == node.LeftOperand.TypeReturn)) &&
                node.LeftOperand.TypeReturn != node.RightOperand.TypeReturn)
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line{1} Column{2}", Sp.Name, node.Line, node.Column)));
            return errors;
        }
        public override List<SemanticResult> Visit(EqualNode node)
        {
            return SemanticCheckComparisonBynaryOperators(node);
        }

        public override List<SemanticResult> Visit(GreaterEqualNode node)
        {
            return SemanticCheckComparisonBynaryOperators(node);
        }

        public override List<SemanticResult> Visit(GreaterNode node)
        {
            return SemanticCheckComparisonBynaryOperators(node);
        }

        public override List<SemanticResult> Visit(IsDistinctFromNode node)
        {
            return SemanticCheckComparisonBynaryOperators(node);
        }

        public override List<SemanticResult> Visit(LessEqualNode node)
        {
            return SemanticCheckComparisonBynaryOperators(node);
        }

        public override List<SemanticResult> Visit(LessNode node)
        {
            return SemanticCheckComparisonBynaryOperators(node);
        }

        public override List<SemanticResult> Visit(NotEqualNode node)
        {
            return SemanticCheckComparisonBynaryOperators(node);
        }

        public override List<SemanticResult> Visit(ExponentiationNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            node.TypeReturn = node.LeftOperand.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(ModuloNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            node.TypeReturn = "int";
            return errors;
        }
        private string SelectType(string typeA, string typeB)
        {
            if (TypeInfo.TypesInfo.ContainsKey(typeA) && TypeInfo.TypesInfo.ContainsKey(typeB))
                if (TypeInfo.TypesInfo[typeA].GeneralType == TypeInfo.TypesInfo[typeB].GeneralType)
                    return (TypeInfo.TypesInfo[typeA].Index <= TypeInfo.TypesInfo[typeB].Index) ? typeA : typeB;
                else return (TypeInfo.TypesInfo[typeA].Index < TypeInfo.TypesInfo[typeB].Index) ? typeB : typeA;
            return typeA;
        }
        private List<SemanticResult> SemanticCheckBinaryArithmeticNodes(ArithmeticsBinaryExpressionNode node)
        {
            var errors = (node.LeftOperand != null) ? VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList() :
            VisitNode(node.RightOperand);
            if (node.LeftOperand == null)
                node.TypeReturn = node.RightOperand.TypeReturn;
            else
            {
                node.TypeReturn = SelectType(node.LeftOperand.TypeReturn, node.RightOperand.TypeReturn);
                if (node is SubtractionNode && node.TypeReturn == "DateTime")
                    node.TypeReturn = "TimeSpan";
            }
            return errors;
        }
        public override List<SemanticResult> Visit(MultiplicationNode node)
        {
            return SemanticCheckBinaryArithmeticNodes(node);
        }

        public override List<SemanticResult> Visit(StringConcatNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            node.TypeReturn = "string";
            if (node.LeftOperand.TypeReturn != "string")
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{0}",
                    Sp.Name, node.LeftOperand.Line, node.LeftOperand.Column)));
            if (node.RightOperand.TypeReturn != "varchar")
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{0}",
                    Sp.Name, node.RightOperand.Line, node.RightOperand.Column)));
            return errors;
        }

        public override List<SemanticResult> Visit(SubtractionNode node)
        {
            return SemanticCheckBinaryArithmeticNodes(node);
        }

        public override List<SemanticResult> Visit(AdditionNode node)
        {
            return SemanticCheckBinaryArithmeticNodes(node);
        }

        public override List<SemanticResult> Visit(BitwiseAndNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            if (node.LeftOperand.TypeReturn != node.RightOperand.TypeReturn)
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column)));
            node.TypeReturn = node.LeftOperand.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(BitwiseOrNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            if (node.LeftOperand.TypeReturn != node.RightOperand.TypeReturn)
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column)));
            node.TypeReturn = node.LeftOperand.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(BitwiseShiftLeftNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            if ((node.LeftOperand.TypeReturn != "int" || node.LeftOperand.TypeReturn != "long") &&
                node.LeftOperand.TypeReturn != node.RightOperand.TypeReturn)
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column)));
            node.TypeReturn = node.LeftOperand.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(BitwiseShiftRightNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            if ((node.LeftOperand.TypeReturn != "int" || node.LeftOperand.TypeReturn != "long") &&
                node.LeftOperand.TypeReturn != node.RightOperand.TypeReturn)
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column)));
            node.TypeReturn = node.LeftOperand.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(BitwiseXorNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            if (node.LeftOperand.TypeReturn != node.RightOperand.TypeReturn)
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column)));
            node.TypeReturn = node.LeftOperand.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(DivisionNode node)
        {
            return SemanticCheckBinaryArithmeticNodes(node);
        }

        public override List<SemanticResult> Visit(AndNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            if (node.LeftOperand.TypeReturn != "bool" && node.LeftOperand.TypeReturn != node.RightOperand.TypeReturn)
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column)));
            node.TypeReturn = node.LeftOperand.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(OrNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            if (node.LeftOperand.TypeReturn != "bool" && node.LeftOperand.TypeReturn != node.RightOperand.TypeReturn)
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column)));
            node.TypeReturn = node.LeftOperand.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(ExpInDirectionNode node)
        {
            var errors = VisitNode(node.Expression);
            if (node.Indirections != null)
                foreach (var indirection in node.Indirections)
                    errors.AddRange(VisitNode(indirection));
            node.TypeReturn = node.Expression.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(ExpressionListNode node)
        {
            var errors = new List<SemanticResult>();

            foreach (var exp in node.Expressions)
            {
                errors.AddRange(VisitNode(exp));
                if (node.TypeReturn == null)
                    node.TypeReturn = exp.TypeReturn;
                else
                    node.TypeReturn += "," + exp.TypeReturn;
            }
            node.TypeReturn = "(" + node.TypeReturn + ")";
            return errors;
        }

        public override List<SemanticResult> Visit(NullNode node)
        {
            node.TypeReturn = "null";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(MultiplyNode node)
        {
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(CaseExpressionNode node)
        {
            var errors = new List<SemanticResult>();
            foreach (var exp in node.Expressions)
            {
                errors.AddRange(VisitNode(exp));
            }
            var start = 0;
            var end = node.Expressions.Count - 1;
            string elseExpressionType = null;
            string headCaseType = null;
            if (node.Else)
                elseExpressionType = node.Expressions[end--].TypeReturn;
            if (node.ContainsRootExpression)
                headCaseType = node.Expressions[start++].TypeReturn;
            for (int i = start; i <= end; i += 2)
            {
                if (headCaseType != null)
                {
                    if (headCaseType != node.Expressions[i].TypeReturn)
                        errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                            Sp.Name, node.Expressions[i].Line, node.Expressions[i].Column)));
                }
                else
                    headCaseType = node.Expressions[i].TypeReturn;
                if (elseExpressionType != null)
                {
                    if (elseExpressionType != node.Expressions[i + 1].TypeReturn)
                        errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                            Sp.Name, node.Expressions[i + 1].Line, node.Expressions[i + 1].Column)));
                }
                else
                    elseExpressionType = node.Expressions[i + 1].TypeReturn;
            }
            return errors;
        }

        public override List<SemanticResult> Visit(ExistsNode node)
        {
            node.TypeReturn = "bool";
            return VisitNode(node.SelectStmt);
        }

        public override List<SemanticResult> Visit(IndirectionVarNode node)
        {
            var errors = new List<SemanticResult>();
            errors.AddRange(VisitNode(node.Identifiers));
            node.TypeReturn = node.Identifiers.TypeReturn;
            
            if (node.Indirections != null && node.Indirections.Count > 0)
            {
                foreach (var indirection in node.Indirections)
                {
                    if (indirection.ColLabel != null)
                    {
                        var table = Catalog.Tables.Values.FirstOrDefault(t => t.Name == node.TypeReturn);
                        if (table != null)
                        {
                            var field = table.Fields.FirstOrDefault(f => f.Name == indirection.ColLabel.Text);
                            if (field != null)
                                node.TypeReturn = field.DestinyType;
                            else
                                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                                    Sp.Name, indirection.Line, indirection.Column)));
                        }
                        else
                        {
                            var udt = Catalog.UserDefinedTypes.Values.FirstOrDefault(t => t.TypeName == node.TypeReturn) as UserDefinedType;
                            if (table != null)
                            {
                                var field = udt.Fields.FirstOrDefault(f => f.Name == indirection.ColLabel.Text);
                                if (field != null)
                                    node.TypeReturn = field.DestinyType;
                                else
                                    errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                                        Sp.Name, indirection.Line, indirection.Column)));
                            }
                        }
                    }
                }
            }
            return errors;
        }

        public override List<SemanticResult> Visit(ValueExpressionPrimaryNode node)
        {
            var errors = VisitNode(node.SelectStmtNonParens);
            node.TypeReturn = node.SelectStmtNonParens.TypeReturn;
            if (node.Indirections != null && node.Indirections.Count > 0)
                foreach (var indirection in node.Indirections)
                {
                    errors.AddRange(VisitNode(indirection));
                }
            return errors;
        }

        public override List<SemanticResult> Visit(IntervalFieldNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(DatetimeOverlapsNode node)
        {
            var errors = VisitNode(node.Expression1)
                .Concat(VisitNode(node.Expression2))
                .Concat(VisitNode(node.Expression3))
                .Concat(VisitNode(node.Expression4)).ToList();
            node.TypeReturn = "bool";
            return errors;
        }

        public override List<SemanticResult> Visit(TableColsNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(UserNameNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(GroupByClauseNode node)
        {
            var errors = new List<SemanticResult>();
            foreach (var exp in node.Expressions)
                errors.AddRange(VisitNode(exp));
            return errors;
        }

        public override List<SemanticResult> Visit(ArrayTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(ArrayToSelectNode node)
        {
            var errors = VisitNode(node.SelectStmt);
            node.TypeReturn = "object[]";
            return errors;
        }

        public override List<SemanticResult> Visit(ArrayElementsNode node)
        {
            node.TypeReturn = "object[]";
            var errors = new List<SemanticResult>();
            foreach (var exp in node.Elements)
            {
                errors.AddRange(VisitNode(exp));
            }
            return errors;
        }

        public override List<SemanticResult> Visit(ComparisonModNode node)
        {
            var errors = new List<SemanticResult>();

            if (node.Expression != null)
            {
                errors.AddRange(VisitNode(node.Expression));
                node.TypeReturn = node.Expression.TypeReturn;
            }
            else if (node.SelectStmtNonParens != null)
            {
                errors.AddRange(VisitNode(node.SelectStmtNonParens));
                node.TypeReturn = node.SelectStmtNonParens.TypeReturn;
            }
            return errors;
        }


        public override List<SemanticResult> Visit(BasicFunctionCallNode node)
        {
            var errors = new List<SemanticResult>();

            var functionName = (node.SchemaQualifiednameNonType.IdentifierNonType.Text);
            foreach (var vexOrName in node.VexOrNamedNotations)
                errors.AddRange(VisitNode(vexOrName));
            var predefinedFunctions = PredefinedFunctions.FindAll(
                f => f.Item1 == functionName && f.Item2.Count == node.VexOrNamedNotations.Count);
            if (predefinedFunctions != null && predefinedFunctions.Count > 0)
            {
                var bndFunctions = false;
                foreach (var pf in predefinedFunctions)
                {
                    var bndParams = true;
                    for (int i = 0; i < pf.Item2.Count; i++)
                    {
                        if (pf.Item2[i] != node.VexOrNamedNotations[i].TypeReturn)
                        {
                            bndParams = false;
                            break;
                        }
                    }
                    bndFunctions = bndFunctions || bndParams;
                    if (bndFunctions)
                    {
                        node.TypeReturn = pf.Item3;
                        break;
                    }
                }
            }
            else if (functionName.ToLower() == "if" && node.VexOrNamedNotations.Count == 3 &&
                node.VexOrNamedNotations[0].TypeReturn == "bool")
                node.TypeReturn = node.VexOrNamedNotations[1].TypeReturn;
            else if (functionName.ToLower() == "count" && node.VexOrNamedNotations.Count == 1)
                node.TypeReturn = "int";
            else
            {
                var sps = Catalog.StoreProcedures.Values.Where(f => f.Name == functionName).ToList();
                var internalFunctions = Catalog.InternalFunctions.Values.Where(f => f.Name == functionName).ToList();

                if (sps.Count == 0 && internalFunctions.Count == 0)
                    errors.Add(new ErrorResult(String.Format("Semeantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column)));
                else
                {
                    StoreProcedure sp = null;
                    foreach (var f in sps.Concat(internalFunctions))
                    {
                        var bnd = false;
                        if (f.Params.Where(p => !p.OutMode).ToList().Count == node.VexOrNamedNotations.Count)
                        {
                            bnd = true;
                            for (int i = 0; i < node.VexOrNamedNotations.Count; i++)
                                if (f.Params[i].DestinyType != node.VexOrNamedNotations[i].TypeReturn)
                                {
                                    bnd = false;
                                    break;
                                }
                        }
                        if (bnd)
                        {
                            sp = f;
                            if (sps.Contains(sp))
                                errors.Add(new FunctionResult() { Sp = sp });
                            break;
                        }
                    }
                    if (sp == null)
                        errors.Add(new ErrorResult(String.Format("Semeantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column)));
                    else
                        node.TypeReturn = sp.ReturnType;
                }
            }

            return errors;
        }

        public override List<SemanticResult> Visit(FunctionConstructNode node)
        {
            var errors = new List<SemanticResult>();
            foreach (var expression in node.Expressions)
                errors.AddRange(VisitNode(expression));

            if (node.Coalesce || node.Greatest || node.Least || node.Nullif || node.XmlConcat)
                node.TypeReturn = node.Expressions[0].TypeReturn;
            else if (node.Grouping)
                node.TypeReturn = "int";
            else if (node.Row)
            {
                foreach (var expression in node.Expressions)
                {
                    if (node.TypeReturn == null || node.TypeReturn == "")
                        node.TypeReturn = expression.TypeReturn;
                    else
                        node.TypeReturn += expression.TypeReturn;
                }
            }

            return errors;
        }

        public override List<SemanticResult> Visit(ExtractFunctionNode node)
        {
            node.TypeReturn = "double";
            return VisitNode(node.Expression);
        }

        public override List<SemanticResult> Visit(TrimStringValueFunctionNode node)
        {
            var errors = VisitNode(node.Chars).Concat(VisitNode(node.Str)).ToList();
            if (node.Chars.TypeReturn != "string" || node.Str.TypeReturn != "string")
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column)));
            node.TypeReturn = "string";
            return errors;
        }

        public override List<SemanticResult> Visit(SubstringStringValueFunctionNode node)
        {
            var errors = new List<SemanticResult>();
            foreach (var exp in node.Expressions)
            {
                errors.AddRange(VisitNode(exp));
            }
            var end = node.Expressions.Count - 1;
            if (node.For && node.Expressions[end].TypeReturn != "int")
            {
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                    Sp.Name, node.Expressions[end].Line, node.Expressions[end].Column)));
                end--;
            }
            if (node.From && node.Expressions[end].TypeReturn != "int")
            {
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                    Sp.Name, node.Expressions[end].Line, node.Expressions[end].Column)));
                end--;
            }
            for (int i = 0; i <= end; i++)
            {
                if (node.Expressions[i].TypeReturn != "string")
                    errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                    Sp.Name, node.Expressions[i].Line, node.Expressions[i].Column)));
            }
            return errors;
        }

        public override List<SemanticResult> Visit(PositionStringValueFunctionNode node)
        {
            var errors = VisitNode(node.ExpressionB).Concat(VisitNode(node.Expression)).ToList();

            if (node.ExpressionB.TypeReturn != "string")
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                    Sp.Name, node.ExpressionB.Line, node.ExpressionB.Column)));
            if (node.Expression.TypeReturn != "string")
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                    Sp.Name, node.Expression.Line, node.Expression.Column)));
            node.TypeReturn = "int";
            return errors;
        }

        public override List<SemanticResult> Visit(OverlayStringValueFunctionNode node)
        {
            var errors = new List<SemanticResult>();
            foreach (var exp in node.Expressions)
            {
                errors.AddRange(VisitNode(exp));
            }
            var end = node.Expressions.Count - 1;
            if (node.For && node.Expressions[end].TypeReturn != "int")
            {
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                      Sp.Name, node.Expressions[end].Line, node.Expressions[end].Column)));
                end--;
            }
            if (node.Expressions[end].TypeReturn != "int")
            {
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                      Sp.Name, node.Expressions[end].Line, node.Expressions[end].Column)));
                end--;
            }
            for (int i = 0; i <= end; i++)
            {
                if (node.Expressions[i].TypeReturn != "string")
                {
                    errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                          Sp.Name, node.Expressions[i].Line, node.Expressions[i].Column)));
                }
            }
            node.TypeReturn = "string";
            return errors;
        }

        public override List<SemanticResult> Visit(CollationStringValueFunctionNode node)
        {
            return VisitNode(node.Expression);
        }

        public override List<SemanticResult> Visit(CastSpesificationSystemFunction node)
        {
            var errors = VisitNode(node.Expression);
            errors.AddRange(VisitNode(node.DataType));
            node.TypeReturn = node.DataType.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(XmlElementFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(XmlForestFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(XmlPiFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(XmlRootFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(XmlExistsFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(XmlParseFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(XmlSerializeFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(XmlTabletFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(XmlTableColumnNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(CurrentCatalogSystemFunctionNode node)
        {
            node.TypeReturn = "string";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(CurrentSchemaSystemFunctionNode node)
        {
            node.TypeReturn = "string";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(CurrentUserSystemFunctionNode node)
        {
            node.TypeReturn = "string";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(UserSystemFunctionNode node)
        {
            node.TypeReturn = "string";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(SessionUserSystemFunctionNode node)
        {
            node.TypeReturn = "string";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(CurrentDateFunctionNode node)
        {
            node.TypeReturn = "DateTime";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(CurrentTimeFunctionNode node)
        {
            node.TypeReturn = "DateTime";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(LocalTimeFunctionNode node)
        {
            node.TypeReturn = "DateTime";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(CurrentTimestampFunctionNode node)
        {
            node.TypeReturn = "TimeSpan";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(LocalTimestampFunctionNode node)
        {
            node.TypeReturn = "TimeSpan";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(BoolNode node)
        {
            node.TypeReturn = "bool";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(DefaultNode node)
        {
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(Float4Node node)
        {
            node.TypeReturn = "double";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(Float8Node node)
        {
            node.TypeReturn = "double";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(Int8Node node)
        {
            node.TypeReturn = "long";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(IntNode node)
        {
            node.TypeReturn = "int";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(VarcharNode node)
        {
            node.TypeReturn = "string";
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(SelectOpsNoParensNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(SelectOpsNode node)
        {
            if (node.SelectStmt != null)
                return VisitNode(node.SelectStmt);
            else if (node.SelectPrimary != null)
                return VisitNode(node.SelectPrimary);
            else
            {
                var errors = new List<SemanticResult>();
                foreach (var selectOps in node.SelectOps)
                    errors.AddRange(VisitNode(selectOps));
                return errors;
            }
        }

        public override List<SemanticResult> Visit(SelectPrimaryNode node)
        {
            if (node.SchemaQualifield != null)
            {
                var errors = VisitNode(node.SchemaQualifield);
                node.TypeReturn = node.SchemaQualifield.TypeReturn;
                return errors;
            }
            else
            {
                var errors = new List<SemanticResult>();
                var newSemanticVisitor = new SemanticVisitor(Catalog, Sp,
                    new Dictionary<string, (string,IdentifierType, object)>(VariablesTypes), CursorTypes);
                if (node.FromItems != null && node.FromItems.Count > 0)
                {
                    foreach (var fromItem in node.FromItems)
                        errors.AddRange(newSemanticVisitor.VisitNode(fromItem));
                }
                if (node.SelectList != null)
                {
                    errors.AddRange(newSemanticVisitor.VisitNode(node.SelectList));
                    node.TypeReturn = node.SelectList.TypeReturn;
                }

                if (node.IntoTable != null)
                    foreach (var item in node.IntoTable)
                        errors.AddRange(newSemanticVisitor.VisitNode(item));

                if (node.Expressions != null)
                    foreach (var exp in node.Expressions)
                        errors.AddRange(newSemanticVisitor.VisitNode(exp));

                if (node.GroupByClause != null)
                    errors.AddRange(newSemanticVisitor.VisitNode(node.GroupByClause));

                if (node.WindowsDefinitions != null && node.WindowsDefinitions.Count > 0)
                    foreach (var windowsDefinition in node.WindowsDefinitions)
                        errors.AddRange(newSemanticVisitor.VisitNode(windowsDefinition));

                return errors;
            }
        }

        public override List<SemanticResult> Visit(SelectListNode node)
        {
            var errors = new List<SemanticResult>();
            var type = "";
            foreach (var sublist in node.SelectSubLists)
            {
                errors.AddRange(VisitNode(sublist));
                type += (type == "") ? sublist.TypeReturn : "," + sublist.TypeReturn;
            }
            node.TypeReturn = type;
            return errors;
        }

        public override List<SemanticResult> Visit(SelectSubListNode node)
        {
            var errors = VisitNode(node.Expression);
            node.TypeReturn = node.Expression.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(IdNode node)
        {
            var errors = new List<SemanticResult>();
            if (VariablesTypes.ContainsKey(node.Text))
            {
                node.TypeReturn = VariablesTypes[node.Text].Item1;
                if (VariablesTypes[node.Text].Item2 == IdentifierType.Table)
                    node.Table = VariablesTypes[node.Text].Item3 as Table;
                else if (VariablesTypes[node.Text].Item2 == IdentifierType.Udt)
                    node.Udt = VariablesTypes[node.Text].Item3 as UserDefinedType;
                else if (VariablesTypes[node.Text].Item2 == IdentifierType.TableField)
                    node.TableField = VariablesTypes[node.Text].Item3 as TableField;
                if (VariablesTypes[node.Text].Item2 == IdentifierType.UdtField)
                    node.UdtField = VariablesTypes[node.Text].Item3 as UdtField;
            }
            else
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column)));
            return errors;
        }

        public override List<SemanticResult> Visit(SchemaQualifieldNode node)
        {
            var errors = new List<SemanticResult>();
            Table table = null;
            BaseUserDefinedType udt = null;
            var initialType = "";
            foreach (var identifier in node.Identifiers)
            {
                if (initialType == "")
                {
                    if (VariablesTypes.ContainsKey(identifier.Text))
                        initialType = VariablesTypes[identifier.Text].Item1;
                    else if (CursorTypes.ContainsKey(identifier.Text))
                        initialType = CursorTypes[identifier.Text].Item2;
                    else
                    {
                        udt = Catalog.UserDefinedTypes.Values.FirstOrDefault(u => u.TypeName == identifier.Text);
                        table = Catalog.Tables.Values.FirstOrDefault(t => t.Name == identifier.Text);
                        if (table != null)
                        {
                            identifier.Table = table;
                            initialType = identifier.Text;
                        }
                        else if (udt != null)
                        {
                            identifier.Udt = udt;
                            initialType = identifier.Text;
                        }
                        else
                            errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, identifier.Line, identifier.Column)));
                    }
                }
                else
                {
                    table = Catalog.Tables.Values.FirstOrDefault(t => t.Name == initialType);
                    udt = Catalog.UserDefinedTypes.Values.FirstOrDefault(u => u.TypeName == initialType);
                    if (table != null)
                    {
                        var fieldT = table.Fields.FirstOrDefault(f => f.Name == identifier.Text);
                        if (fieldT != null)
                        {
                            identifier.TableField = fieldT;
                            initialType = fieldT.DestinyType;
                        }
                        else
                            errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, identifier.Line, identifier.Column)));
                    }
                    else if (udt != null && udt.IsUDT)
                    {
                        var fieldUDT = (udt as UserDefinedType).Fields.FirstOrDefault(f => f.Name == identifier.Text);
                        if (fieldUDT != null)
                        {
                            identifier.UdtField = fieldUDT;
                            initialType = fieldUDT.DestinyType;
                        }
                        else
                            errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, identifier.Line, identifier.Column)));
                    }
                    else
                    {
                        if (VariablesTypes.ContainsKey(identifier.Text))
                            initialType = VariablesTypes[identifier.Text].Item1;
                        else
                            errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, identifier.Line, identifier.Column)));
                    }
                }
            }
            node.TypeReturn = initialType;
            return errors;
        }

        public override List<SemanticResult> Visit(SchemaQualifiednameNonTypeNode node)
        {
            var initialType = "";
            if (node.Schema != null)
            {
                if (VariablesTypes.ContainsKey(node.Schema.Text))
                    initialType = VariablesTypes[node.Schema.Text].Item1;
                else
                    initialType = node.Schema.Text;
            }
            if (node.IdentifierNonType != null)
            {
                if (initialType == "")
                {
                    if (VariablesTypes.ContainsKey(node.IdentifierNonType.Text))
                        initialType = VariablesTypes[node.IdentifierNonType.Text].Item1;
                    else
                    {
                        var table = Catalog.Tables.Values.FirstOrDefault(t => t.Name == node.IdentifierNonType.Text);
                        var udt = Catalog.UserDefinedTypes.Values.FirstOrDefault(u => u.TypeName == node.IdentifierNonType.Text);
                        if (table != null)
                        {
                            initialType = table.Name;
                        }
                        else if (udt != null)
                            initialType = udt.TypeName;
                    }
                }
            }
            node.TypeReturn = initialType;
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(FrameBoundNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(FrameClauseNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(FromItemSimpleNode node)
        {
            var errors = VisitNode(node.Item);
            if (node.AliasClause != null)
            {
                errors = errors.Concat(VisitNode(node.AliasClause)).ToList();
                node.Alias = node.AliasClause.Alias.Text;
            }
            return errors;
        }

        public override List<SemanticResult> Visit(FromItemCrossJoinNode node)
        {
            var errors = VisitNode(node.Item1).Concat(VisitNode(node.Item2)).ToList();
            return errors;
        }

        public override List<SemanticResult> Visit(FromItemOnExpressionNode node)
        {
            var errors = VisitNode(node.Item1).Concat(VisitNode(node.Item2)).ToList();
            return errors;
        }

        public override List<SemanticResult> Visit(FromItemUsingNode node)
        {
            var errors = VisitNode(node.Item1).Concat(VisitNode(node.Item2)).ToList();
            return errors;
        }

        public override List<SemanticResult> Visit(FromItemNaturalNode node)
        {
            var errors = VisitNode(node.Item1).Concat(VisitNode(node.Item2)).ToList();
            return errors;
        }

        public override List<SemanticResult> Visit(ExceptionStatementNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(ExecuteStatementNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(TransactionStatementNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(FromPrimary1Node node)
        {
            var errors = VisitNode(node.SchemaQualifield);
            var alias = node.SchemaQualifield.Identifiers[0].Text;
            var tableName = alias;
            if (node.AliasClause != null)
                alias = node.AliasClause.Alias.Text;
            node.Alias = alias;
            VariablesTypes.Add(alias, (node.SchemaQualifield.TypeReturn, IdentifierType.Variable, null));
            var table = Catalog.Tables.Values.FirstOrDefault(t => t.Name == tableName);
            var udt = Catalog.UserDefinedTypes.Values.FirstOrDefault(u => u.TypeName == tableName);
            if (table != null)
                foreach (var field in table.Fields)
                    if (!VariablesTypes.ContainsKey(field.Name))
                        VariablesTypes.Add(field.Name, (field.DestinyType, IdentifierType.TableField, field));
            if (udt != null && udt is UserDefinedType)
                foreach (var field in (udt as UserDefinedType).Fields)
                    if (!VariablesTypes.ContainsKey(field.Name))
                        VariablesTypes.Add(field.Name, (field.DestinyType, IdentifierType.UdtField, field));
            return errors;
        }

        public override List<SemanticResult> Visit(FromPrimary2Node node)
        {
            var newSemanticVisitor = new SemanticVisitor(Catalog, Sp,
                new Dictionary<string, (string, IdentifierType, object)>(VariablesTypes), CursorTypes);
            var errors = VisitNode(node.TableSubquery);
            var alias = node.AliasClause.Alias.Text;
            if (VariablesTypes.ContainsKey(alias))
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column)));
            else
            {
                var table = Catalog.Tables.Values.FirstOrDefault(t => t.Name == node.TableSubquery.TypeReturn);
                if (table != null)
                    VariablesTypes.Add(alias, (node.TableSubquery.TypeReturn, IdentifierType.Table, table));
                else
                    VariablesTypes.Add(alias, (node.TableSubquery.TypeReturn, IdentifierType.Variable, null));
            }
            return errors;
        }

        public override List<SemanticResult> Visit(FromPrimary3Node node)
        {
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(FromPrimary4Node node)
        {
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(FromFunctionColumnDefNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(AliasClauseNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(VexOrNamedNotationNode node)
        {
            var errors = VisitNode(node.Expression);
            node.TypeReturn = node.Expression.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(ArgumentNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(IndirectionIdentifierNode node)
        {
            var errors = VisitNode(node.Identifier);
            foreach (var item in node.Indirections)
            {
                errors.AddRange(VisitNode(item));
                if (item.ColLabel != null)
                {
                    var table = Catalog.Tables.Values.FirstOrDefault(t => t.Name == item.ColLabel.Text);
                    var udt = Catalog.UserDefinedTypes.Values.FirstOrDefault(u => u.TypeName == item.ColLabel.Text);
                    if (table != null)
                        item.ColLabel.Table = table;
                    else if (udt != null)
                        item.ColLabel.Udt = udt;
                    else if (!VariablesTypes.ContainsKey(item.ColLabel.Text))
                        errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                            Sp.Name, item.ColLabel.Line, item.ColLabel.Column)));
                }
                else errors.AddRange(VisitNode(item));
                        
            }
            return errors;
        }

        public override List<SemanticResult> Visit(IndirectionNode node)
        {
            var errors = new List<SemanticResult>();
            if(node.Expressions != null)
                foreach (var exp in node.Expressions)
                    errors.AddRange(VisitNode(exp));
            return errors;
        }

        public override List<SemanticResult> Visit(ReturnStmtNode node)
        {
            var errors = new List<SemanticResult>();
            if (node.Stmt != null)
                errors.AddRange(VisitNode(node.Stmt));
            if (node.Expression != null)
                errors.AddRange(VisitNode(node.Expression));
            return errors;
        }

        public override List<SemanticResult> Visit(PerformStmtNode node)
        {
            var errors = new List<SemanticResult>();
            var newSemanticVisitor = new SemanticVisitor(Catalog, Sp, VariablesTypes, CursorTypes);

            if (node.FromItems != null && node.FromItems.Count > 0)
                foreach (var fromItem in node.FromItems)
                    errors.AddRange(newSemanticVisitor.VisitNode(fromItem));

            if (node.SelectList != null)
            {
                errors.AddRange(newSemanticVisitor.VisitNode(node.SelectList));
                node.TypeReturn = node.SelectList.TypeReturn;
            }

            if (node.IntoTable != null)
                foreach (var item in node.IntoTable)
                    errors.AddRange(newSemanticVisitor.VisitNode(item));

            foreach (var exp in node.Expressions)
                errors.AddRange(newSemanticVisitor.VisitNode(exp));

            if (node.GroupByClause != null)
                errors.AddRange(newSemanticVisitor.VisitNode(node.GroupByClause));

            if (node.WindowsDefinitions != null && node.WindowsDefinitions.Count > 0)
                foreach (var windowsDefinition in node.WindowsDefinitions)
                    errors.AddRange(newSemanticVisitor.VisitNode(windowsDefinition));

            if (node.SelectOps != null)
                errors.AddRange(VisitNode(node.SelectOps));

            if (node.AfterOps != null)
                foreach (var afterOp in node.AfterOps)
                    errors.AddRange(VisitNode(afterOp));

            return errors;
        }

        public override List<SemanticResult> Visit(IfStmtNode node)
        {
            var errors = new List<SemanticResult>();
            foreach (var exp in node.Expressions)
            {
                errors.AddRange(VisitNode(exp));
                if (exp.TypeReturn != "bool")
                    errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, exp.Line, exp.Column)));
            }
            foreach (var statements in node.Statements)
                foreach (var statement in statements)
                    errors.AddRange(VisitNode(statement));
            if (node.ElseStatements != null)
                foreach (var statement in node.ElseStatements)
                    errors.AddRange(VisitNode(statement));
            return errors;
        }

        public override List<SemanticResult> Visit(AssingStmtNode node)
        {
            var errors = VisitNode(node.Var).Concat(VisitNode(node.Stmt)).ToList();
            if (!(node.Stmt is PerformStmtNode &&
                TypeInfo.TypesInfo.ContainsKey((node.Stmt as PerformStmtNode).TypeReturn) &&
                TypeInfo.TypesInfo.ContainsKey(node.Var.TypeReturn) &&
                TypeInfo.TypesInfo[(node.Stmt as PerformStmtNode).TypeReturn].GeneralType == TypeInfo.TypesInfo[node.Var.TypeReturn].GeneralType) &&
                !(node.Stmt is SelectStmtNonParensNode &&
                TypeInfo.TypesInfo.ContainsKey((node.Stmt as SelectStmtNonParensNode).TypeReturn) &&
                TypeInfo.TypesInfo.ContainsKey(node.Var.TypeReturn) &&
                TypeInfo.TypesInfo[(node.Stmt as SelectStmtNonParensNode).TypeReturn].GeneralType == TypeInfo.TypesInfo[node.Var.TypeReturn].GeneralType))
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Stmt.Line, node.Stmt.Column)));

            return errors;
        }

        public override List<SemanticResult> Visit(VarNode node)
        {
            var errors = new List<SemanticResult>();
            if (node.Expressions != null)
                foreach (var exp in node.Expressions)
                    errors.AddRange(VisitNode(exp));
            if (node.SchemaQualifield != null)
            {
                errors.AddRange(VisitNode(node.SchemaQualifield));
                node.TypeReturn = node.SchemaQualifield.TypeReturn;
            }
            else if (node.Id != null)
            {
                if (!VariablesTypes.ContainsKey(node.Id.Text))
                    errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column)));
                else
                    node.TypeReturn = VariablesTypes[node.Id.Text].Item1;
            }
            return errors;
        }

        public override List<SemanticResult> Visit(RaiseMessageStatementNode node)
        {
            var errors = new List<SemanticResult>();
            if (node.Expressions != null)
                foreach (var exp in node.Expressions)
                    errors.AddRange(VisitNode(exp));
            if (node.RaiseUsing != null)
                foreach (var exp in node.RaiseUsing.Expressions)
                    errors.AddRange(VisitNode(exp));
            return errors;
        }

        public override List<SemanticResult> Visit(AssertMessageStatementNode node)
        {
            var errors = new List<SemanticResult>();
            foreach (var exp in node.Expressions)
                errors.AddRange(VisitNode(exp));
            return errors;
        }

        public override List<SemanticResult> Visit(IdentifierNonTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(DeleteStmtPSqlNode node)
        {
            var errors = new List<SemanticResult>();

            if (node.WithClause != null)
                errors.AddRange(VisitNode(node.WithClause));

            if (node.FromItems != null)
                foreach (var item in node.FromItems)
                    errors.AddRange(VisitNode(item));

            if (node.Expression != null)
                errors.AddRange(VisitNode(node.Expression));

            if (node.SelectList != null)
                errors.AddRange(VisitNode(node.SelectList));

            return errors;
        }

        public override List<SemanticResult> Visit(OpCharsNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(LoopStmtNode node)
        {
            var errors = new List<SemanticResult>();
            var localVariables = new List<(string, string)>();
            if (node.LoopStart != null)
            {
                errors.AddRange(VisitNode(node.LoopStart));
                if (node.LoopStart is ForAliasLoopNode)
                {
                    var loopStart = node.LoopStart as ForAliasLoopNode;
                    localVariables.Add((loopStart.Identifier.Text, loopStart.Expressions.First().TypeReturn));
                }
                else if (node.LoopStart is ForIdListLoopNode)
                {
                    var loopStart = node.LoopStart as ForIdListLoopNode;
                    string[] stmtTypeReturn = null;

                    if (loopStart.Stmt is SelectStatementNode)
                        stmtTypeReturn = (loopStart.Stmt as SelectStatementNode).TypeReturn.Split('_');
                    else if (loopStart.Stmt is ExecuteStmtNode)
                        stmtTypeReturn = (loopStart.Stmt as ExecuteStmtNode).TypeReturn.Split('_');

                    if (loopStart.Identifiers.Count != stmtTypeReturn.Length)
                        errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                            Sp.Name, loopStart.Line, loopStart.Column)));
                    else
                        for (int i = 0; i < loopStart.Identifiers.Count; i++)
                            localVariables.Add((loopStart.Identifiers[i].Text, stmtTypeReturn[i]));
                }
                else if (node.LoopStart is ForCursorLoopNode)
                {
                    var loopStar = node.LoopStart as ForCursorLoopNode;
                    var cursorName = VariablesTypes.Keys.Where(s => s.StartsWith(loopStar.Identifier.Text)).First();
                    localVariables.Add((loopStar.Cursor.Text, VariablesTypes[cursorName].Item1.Split('_').Last()));
                }
                else if (node.LoopStart is ForeachLoopNode)
                {
                    var loopStart = node.LoopStart as ForeachLoopNode;
                    var types = loopStart.Expression.TypeReturn.Split('_');
                    if (loopStart.Identifiers.Count != types.Length)
                        errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column)));
                }
            }
            if (node.Statemets != null)
                foreach (var statement in node.Statemets)
                    errors.AddRange(VisitNode(statement));
            if (node.Expression != null)
                errors.AddRange(VisitNode(node.Expression));
            return errors;
        }

        public override List<SemanticResult> Visit(OpNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(ConflictObjectNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(ConflictActionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(DeclareStatementNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(ModularTypeDeclarationNode node)
        {
            var errors = new List<SemanticResult>(VisitNode(node.Schema));
            node.TypeReturn = node.Schema.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(ModularRowTypeDeclarationNode node)
        {
            var errors = new List<SemanticResult>(VisitNode(node.Schema));
            node.TypeReturn = node.Schema.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(AnalizeModeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(WhileLoopNode node)
        {
            var errors = VisitNode(node.Expressions[0]);
            if (node.Expressions[0].TypeReturn != "bool")
                errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                    Sp.Name, node.Expressions[0].Line, node.Expressions[0].Column)));
            return errors;
        }

        public override List<SemanticResult> Visit(ForAliasLoopNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(ForIdListLoopNode node)
        {
            var errors = new List<SemanticResult>();
            errors.AddRange(VisitNode(node.Stmt));
            var stmtType = (node.Stmt is SelectStatementNode) ? (node.Stmt as SelectStatementNode).TypeReturn :
                (node.Stmt as ExecuteStmtNode).TypeReturn;
            foreach (var id in node.Identifiers)
                if (!VariablesTypes.ContainsKey(id.Text))
                    errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column)));
            return errors;
        }

        public override List<SemanticResult> Visit(ForCursorLoopNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(ForeachLoopNode node)
        {
            var errors = new List<SemanticResult>();
            foreach (var id in node.Identifiers)
                if (!VariablesTypes.ContainsKey(id.Text))
                    errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column)));
            errors.AddRange(VisitNode(node.Expression));
            return errors;
        }

        public override List<SemanticResult> Visit(InsertStmtPSqlNode node)
        {
            var errors = new List<SemanticResult>();
            if (node.WithClause != null)
                errors.AddRange(VisitNode(node.WithClause));
            if (node.SelectStmt != null)
                errors.AddRange(VisitNode(node.SelectStmt));
            if (node.InsertColumns != null)
                foreach (var item in node.InsertColumns)
                    errors.AddRange(VisitNode(item));
            if (node.SelectList != null)
                errors.AddRange(VisitNode(node.SelectList));
            return errors;
        }

        public override List<SemanticResult> Visit(AfterOpsFetchNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(AfterOpsForNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(AfterOpsLimitNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(AfterOpsOffsetNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(OrderByClauseNode node)
        {
            return new List<SemanticResult>();
        }

        public override List<SemanticResult> Visit(SortSpecifierNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(OrderSpecificationNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(AllOpRefNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(ExecuteStmtNode node)
        {
            var errors = VisitNode(node.Expression);
            if (node.UsingExpression != null)
                foreach (var exp in node.UsingExpression)
                    errors.AddRange(VisitNode(exp));
            node.TypeReturn = node.Expression.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(UpdateStmtPSqlNode node)
        {
            var errors = new List<SemanticResult>();
            if (node.WithClause != null)
                errors.AddRange(VisitNode(node.WithClause));

            foreach (var set in node.UpdateSets)
                errors.AddRange(Visit(set));

            if (node.FromItems != null)
                foreach (var item in node.FromItems)
                    errors.AddRange(VisitNode(item));

            if (node.Expression != null)
                errors.AddRange(VisitNode(node.Expression));

            if (node.SelectList != null)
                errors.AddRange(VisitNode(node.SelectList));

            return errors;
        }

        public override List<SemanticResult> Visit(ValuesStmtNode node)
        {
            var errors = new List<SemanticResult>();
            var returnType = "(";
            foreach (var valuesValues in node.Values)
            {
                errors.AddRange(VisitNode(valuesValues));
                returnType += (returnType == "") ? valuesValues.TypeReturn : "," + valuesValues.TypeReturn;
            }
            node.TypeReturn = returnType + ")";
            return errors;
        }

        public override List<SemanticResult> Visit(UpdateSetNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(ValuesValuesNode node)
        {
            var errors = new List<SemanticResult>();
            var typeReturn = "(";
            foreach (var exp in node.Expressions)
            {
                errors.AddRange(VisitNode(exp));
                typeReturn += (typeReturn == "") ? exp.TypeReturn : "," + exp.TypeReturn;
            }
            node.TypeReturn = typeReturn + ")";
            return errors;
        }
        public bool LessEqualType(string typeA, string typeB)
        {
            if (typeB == "object")
                return true;
            else if (typeA == typeB)
                return true;
            else if (TypeInfo.TypesInfo.ContainsKey(typeA) && TypeInfo.TypesInfo.ContainsKey(typeB) && TypeInfo.TypesInfo[typeA].GeneralType == TypeInfo.TypesInfo[typeB].GeneralType)
                return TypeInfo.TypesInfo[typeA].Index <= TypeInfo.TypesInfo[typeB].Index;
            else
                return false;
        }
        public override List<SemanticResult> Visit(CursorStatementNode node)
        {
            var errors = new List<SemanticResult>();
            if (node.Open && node.Var != null)
            {
                errors.AddRange(VisitNode(node.Var));
                if (node.Stmt != null)
                    errors.AddRange(VisitNode(node.Stmt));
                else if (node.Options != null)
                    foreach (var op in node.Options)
                        errors.AddRange(VisitNode(op));
            }
            else if (node.Fetch)
            {
                errors.AddRange(VisitNode(node.Var));
                if (!CursorTypes.ContainsKey(node.Var.SchemaQualifield.Identifiers[0].Text))
                    errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Var.Line, node.Var.Column)));
                else
                {
                    var cursorTypeReturn = CursorTypes[node.Var.SchemaQualifield.Identifiers[0].Text].Item2;
                    if (node.IntoTable != null)
                    { }
                }
            }
            else if (node.Move)
            {
                errors.AddRange(VisitNode(node.Var));
                if (!VariablesTypes.ContainsKey(node.Var.SchemaQualifield.Identifiers[0].Text))
                    errors.Add(new ErrorResult(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Var.Line, node.Var.Column)));
            }
            return errors;
        }

        public override List<SemanticResult> Visit(OptionNode node)
        {
            var errors = VisitNode(node.Expression);
            node.TypeReturn = node.Expression.TypeReturn;
            return errors;
        }

        public override List<SemanticResult> Visit(IsNotDistinctFromNode node)
        {
            throw new NotImplementedException();
        }

        public override List<SemanticResult> Visit(AllSimpleOpNode node)
        {
            throw new NotImplementedException();
        }

    }
}
