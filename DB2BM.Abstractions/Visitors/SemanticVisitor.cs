﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using DB2BM.Abstractions.AST.Statements.Addicional;
using DB2BM.Abstractions.AST.Statements.Base;
using DB2BM.Abstractions.AST.Statements.Control;
using DB2BM.Abstractions.AST.Statements.Cursor;
using DB2BM.Abstractions.AST.Statements.Data;
using DB2BM.Abstractions.AST.Types;
using DB2BM.Abstractions.Entities;
using DB2BM.Abstractions.Entities.UserDefined;
using DB2BM.Abstractions.Visitors;

namespace DB2BM.Abstractions
{
    public class SemanticVisitor : ASTVisitor<List<string>>
    {
        DatabaseCatalog Catalog;
        StoreProcedure Sp;
        Dictionary<string, string> VariablesTypes;

        public SemanticVisitor(DatabaseCatalog catalog, StoreProcedure sp)
        {
            Catalog = catalog;
            Sp = sp;
            VariablesTypes = new Dictionary<string, string>();
            foreach (var param in sp.Params)
                VariablesTypes.Add(param.Name, param.OriginType);
        }
        SemanticVisitor(DatabaseCatalog catalog, StoreProcedure sp, Dictionary<string, string> variablesTypes)
        {
            Catalog = catalog;
            Sp = sp;
            VariablesTypes = variablesTypes;
        }
        public override List<string> Visit(FunctionBlockNode node)
        {
            var errors = new List<string>();
            foreach (var dec in node.Declarations)
                errors.AddRange(VisitNode(dec));
            foreach (var stmt in node.Statements)
                errors.AddRange(VisitNode(stmt));
            errors.AddRange(VisitNode(node.ExceptionStatement));
            return errors;
        }

        public override List<string> Visit(DeclarationNode node)
        {
            var errors = new List<string>();
            if (VariablesTypes.ContainsKey(node.Identifier.Text))
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column));
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
                    errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, instanceIdentifier.Line, instanceIdentifier.Column));
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
                        if (typeDeclaration.Expression.TypeReturn != typeDeclaration.DataType.TypeReturn)
                            errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, typeDeclaration.Expression.Line, typeDeclaration.Expression.Column));
                    }
                    VariablesTypes.Add(node.Identifier.Text, typeDeclaration.DataType.TypeReturn);
                }
            }
            else if (node.TypeDeclaration is CursorDeclarationNode)
            {
                var cursorDeclaration = node.TypeDeclaration as CursorDeclarationNode;
                var newVariablesTypes = new Dictionary<string, string>(VariablesTypes);
                var cursorParamsTypes = "";
                foreach (var arg in cursorDeclaration.ArgumentList)
                {
                    errors.AddRange(VisitNode(arg.DataType));
                    if (!newVariablesTypes.ContainsKey(arg.Identifier.Text))
                    {
                        newVariablesTypes.Add(arg.Identifier.Text, arg.DataType.TypeReturn);
                        cursorParamsTypes += "_" + arg.DataType.TypeReturn;
                    }
                    else
                        errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, arg.Line, arg.Column));
                }
                var newSemanticVisitor = new SemanticVisitor(Catalog, Sp, new Dictionary<string, string>(VariablesTypes));
                errors.AddRange(newSemanticVisitor.VisitNode(cursorDeclaration.SelectStmt));
                cursorParamsTypes += "_" + cursorDeclaration.SelectStmt.TypeReturn;
                VariablesTypes.Add(node.Identifier.Text, "cursor" + cursorParamsTypes);
            }
            return errors;
        }

        public override List<string> Visit(AliasDeclarationNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(CursorDeclarationNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(OrdinalTypeDeclarationNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(OtherTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(OtherOpBinaryNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            node.TypeReturn = SelectNumericType(node.LeftOperand.TypeReturn, node.RightOperand.TypeReturn);
            return errors;
        }

        public override List<string> Visit(DataTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(NCharTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(NCharVaryingTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(NumericTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(RealTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(BigintTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(SmallintTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(BitTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(TimeTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(BitVaryingTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(VarcharTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(BooleanTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(DecTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(DecimalTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(DollarNumberNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(DoublePrecisionTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(AtTimeZoneNode node)
        {
            node.TypeReturn = "timestamp";
            return VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
        }

        public override List<string> Visit(FloatTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(IntTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(IntegerTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(IntervalTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(CharTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(CharVaryingTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(AddAnalizeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(AddClusterNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(AddDeallocatteNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(AddListenNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(AddPrepareNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(AddReassignNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(AddRefreshNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(AddReindexNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(AddResetNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(AddUnlistenNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(CopyStmtNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(ExplainStmtNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(ShowStmtNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(CallFunctionCallNode node)
        {
            return VisitNode(node.FunctionCall);
        }

        public override List<string> Visit(WithClauseNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(WithQueryNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(CaseStmtNode node)
        {
            var errors = new List<string>();
            if (node.HeaderExpression != null)
                errors.AddRange(VisitNode(node.HeaderExpression));
            foreach (var c in node.Cases)
            {
                foreach (var exp in c.Expressions)
                    errors.AddRange(VisitNode(exp));
                foreach (var statement in c.Stmts)
                    errors.AddRange(VisitNode(statement));
            }
            if(node.ElseStmts != null)
                foreach (var statement in node.ElseStmts)
                    errors.AddRange(VisitNode(statement));
            return errors;
        }

        public override List<string> Visit(WindowsDefinitionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(NullOrderingNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(CastExpressionNode node)
        {
            var errors = VisitNode(node.DataType).Concat(VisitNode(node.Expression)).ToList();
            node.TypeReturn = node.DataType.TypeReturn;
            return errors;
        }

        public override List<string> Visit(IsNullNode node)
        {
            node.TypeReturn = "null";
            return new List<string>();
        }

        public override List<string> Visit(IsNotNullNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(BetweenNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(InNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(OfNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(SelectStatementNode node)
        {
            var errors = new List<string>();

            if (node.WithClause != null)
                errors.AddRange(VisitNode(node.WithClause));
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

        public override List<string> Visit(SelectStmtNonParensNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(TruncateStmtNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(IsTrueNode node)
        {
            node.TypeReturn = "boolean";
            return VisitNode(node.Operand);
        }

        public override List<string> Visit(IsNotTrueNode node)
        {
            node.TypeReturn = "boolean";
            return VisitNode(node.Operand);
        }

        public override List<string> Visit(IsFalseNode node)
        {
            node.TypeReturn = "boolean";
            return VisitNode(node.Operand);
        }

        public override List<string> Visit(IsNotFalseNode node)
        {
            node.TypeReturn = "boolean";
            return VisitNode(node.Operand);
        }

        public override List<string> Visit(CollateNode node)
        {
            throw new NotImplementedException();
        }
        private List<string> SemanticCheckLikeNodes(LikeNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            node.TypeReturn = "boolean";
            if (node.LeftOperand.TypeReturn != "varchar")
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column{2}", Sp.Name, node.LeftOperand.Line, node.LeftOperand.Column));
            if (node.RightOperand.TypeReturn != "varchar")
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column{2}", Sp.Name, node.RightOperand.Line, node.RightOperand.Column));
            return errors;
        }
        public override List<string> Visit(LikeBinaryNode node)
        {
            return SemanticCheckLikeNodes(node);
        }

        public override List<string> Visit(ILikeBinaryNode node)
        {
            return SemanticCheckLikeNodes(node);
        }

        public override List<string> Visit(NotLikeBinaryNode node)
        {
            return SemanticCheckLikeNodes(node);
        }

        public override List<string> Visit(NotILikeBinaryNode node)
        {
            return SemanticCheckLikeNodes(node);
        }

        public override List<string> Visit(SimilarToBinaryNode node)
        {
            return SemanticCheckLikeNodes(node);
        }

        public override List<string> Visit(NotSimilarToBinaryNode node)
        {
            return SemanticCheckLikeNodes(node);
        }

        public override List<string> Visit(NotNode node)
        {
            node.TypeReturn = "boolean";
            return VisitNode(node.Operand);
        }

        public override List<string> Visit(AbsoluteValueNode node)
        {
            node.TypeReturn = "int4";
            return VisitNode(node.Operand);
        }

        public override List<string> Visit(BitwiseNotNode node)
        {
            var errors = VisitNode(node.Operand);
            node.TypeReturn = node.Operand.TypeReturn;
            return errors;
        }

        public override List<string> Visit(FactorialNode node)
        {
            var errors = VisitNode(node.Operand);
            node.TypeReturn = node.Operand.TypeReturn;
            return errors;
        }

        public override List<string> Visit(OtherOpUnaryNode node)
        {
            var errors = VisitNode(node.Operand);
            node.TypeReturn = node.Operand.TypeReturn;
            return errors;
        }

        public override List<string> Visit(IsDocumentNode node)
        {
            node.TypeReturn = "boolean";
            return VisitNode(node.Operand);
        }

        public override List<string> Visit(IsNotDocumentNode node)
        {
            node.TypeReturn = "boolean";
            return VisitNode(node.Operand);
        }

        public override List<string> Visit(IsUnknownNode node)
        {
            node.TypeReturn = "boolean";
            return VisitNode(node.Operand);
        }

        public override List<string> Visit(IsNotUnknownNode node)
        {
            node.TypeReturn = "boolean";
            return VisitNode(node.Operand);
        }

        public override List<string> Visit(BaseTypeCoercionNode node)
        {
            var errors = new List<string>();
            errors.AddRange(VisitNode(node.DataType));
            node.TypeReturn = node.DataType.TypeReturn;
            return errors;

        }

        public override List<string> Visit(IntervalTypeCoercionNode node)
        {
            node.TypeReturn = "interval";
            return new List<string>();
        }

        private List<string> SemanticCheckComparisonBynaryOperators(ComparisonBinaryExpressionNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            node.TypeReturn = "boolean";
            if (node.RightOperand.TypeReturn != node.LeftOperand.TypeReturn)
                errors.Add(String.Format("Semantic error SP:{0} Line{1} Column{2}", Sp.Name, node.Line, node.Column));
            return errors;
        }
        public override List<string> Visit(EqualNode node)
        {
            return SemanticCheckComparisonBynaryOperators(node);
        }

        public override List<string> Visit(GreaterEqualNode node)
        {
            return SemanticCheckComparisonBynaryOperators(node);
        }

        public override List<string> Visit(GreaterNode node)
        {
            return SemanticCheckComparisonBynaryOperators(node);
        }

        public override List<string> Visit(IsDistinctFromNode node)
        {
            return SemanticCheckComparisonBynaryOperators(node);
        }

        public override List<string> Visit(LessEqualNode node)
        {
            return SemanticCheckComparisonBynaryOperators(node);
        }

        public override List<string> Visit(LessNode node)
        {
            return SemanticCheckComparisonBynaryOperators(node);
        }

        public override List<string> Visit(NotEqualNode node)
        {
            return SemanticCheckComparisonBynaryOperators(node);
        }

        public override List<string> Visit(ExponentiationNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            node.TypeReturn = node.LeftOperand.TypeReturn;
            return errors;
        }

        public override List<string> Visit(ModuloNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            node.TypeReturn = "int4";
            return errors;
        }
        private string SelectNumericType(string typeA, string typeB)
        {
            var typesPriority = new Dictionary<string, int>
            {
                {"int4", 1 },
                {"int8", 2 },
                {"float4", 3 },
                {"float8", 4 }
            };
            var rTypesPriority = new Dictionary<int, string>
            {
                {1, "int4"},
                {2, "int8"},
                {3, "float4"},
                {4, "float8"}
            };
            if (typesPriority.ContainsKey(typeA) && typesPriority.ContainsKey(typeB))
                return rTypesPriority[Math.Max(typesPriority[typeA], typesPriority[typeB])];
            return typeA;
            
        }
        private List<string> SemanticCheckBinaryArithmeticNodes(ArithmeticsBinaryExpressionNode node)
        {
            var errors = (node.LeftOperand != null) ? VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList() :
                VisitNode(node.RightOperand);
            if (node.LeftOperand == null)
                node.TypeReturn = node.RightOperand.TypeReturn;
            else
                node.TypeReturn = SelectNumericType(node.LeftOperand.TypeReturn, node.RightOperand.TypeReturn);
            return errors;
        }
        public override List<string> Visit(MultiplicationNode node)
        {
            return SemanticCheckBinaryArithmeticNodes(node);
        }

        public override List<string> Visit(StringConcatNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            node.TypeReturn = "varchar";
            if (node.LeftOperand.TypeReturn != "varchar")
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{0}",
                    Sp.Name, node.LeftOperand.Line, node.LeftOperand.Column));
            if (node.RightOperand.TypeReturn != "varchar")
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{0}",
                    Sp.Name, node.RightOperand.Line, node.RightOperand.Column));
            return errors;
        }

        public override List<string> Visit(SubtractionNode node)
        {
            return SemanticCheckBinaryArithmeticNodes(node);
        }

        public override List<string> Visit(AdditionNode node)
        {
            return SemanticCheckBinaryArithmeticNodes(node);
        }

        public override List<string> Visit(BitwiseAndNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            if (node.LeftOperand.TypeReturn != node.RightOperand.TypeReturn)
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column));
            node.TypeReturn = node.LeftOperand.TypeReturn;
            return errors;
        }

        public override List<string> Visit(BitwiseOrNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            if (node.LeftOperand.TypeReturn != node.RightOperand.TypeReturn)
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column));
            node.TypeReturn = node.LeftOperand.TypeReturn;
            return errors;
        }

        public override List<string> Visit(BitwiseShiftLeftNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            if ((node.LeftOperand.TypeReturn != "int4" || node.LeftOperand.TypeReturn != "int8" )&&
                node.LeftOperand.TypeReturn != node.RightOperand.TypeReturn)
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column));
            node.TypeReturn = node.LeftOperand.TypeReturn;
            return errors;
        }

        public override List<string> Visit(BitwiseShiftRightNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            if ((node.LeftOperand.TypeReturn != "int4" || node.LeftOperand.TypeReturn != "int8") &&
                node.LeftOperand.TypeReturn != node.RightOperand.TypeReturn)
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column));
            node.TypeReturn = node.LeftOperand.TypeReturn;
            return errors;
        }

        public override List<string> Visit(BitwiseXorNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            if (node.LeftOperand.TypeReturn != node.RightOperand.TypeReturn)
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column));
            node.TypeReturn = node.LeftOperand.TypeReturn;
            return errors;
        }

        public override List<string> Visit(DivisionNode node)
        {
            return SemanticCheckBinaryArithmeticNodes(node);
        }

        public override List<string> Visit(AndNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            if (node.LeftOperand.TypeReturn != "boolean" && node.LeftOperand.TypeReturn != node.RightOperand.TypeReturn)
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column));
            node.TypeReturn = node.LeftOperand.TypeReturn;
            return errors;
        }

        public override List<string> Visit(OrNode node)
        {
            var errors = VisitNode(node.LeftOperand).Concat(VisitNode(node.RightOperand)).ToList();
            if (node.LeftOperand.TypeReturn != "boolean" && node.LeftOperand.TypeReturn != node.RightOperand.TypeReturn)
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column));
            node.TypeReturn = node.LeftOperand.TypeReturn;
            return errors;
        }

        public override List<string> Visit(ExpInDirectionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(ExpressionListNode node)
        {
            var errors = new List<string>();
            
            foreach (var exp in node.Expressions)
            {
                errors.AddRange(VisitNode(exp));
                if (node.TypeReturn == null)
                    node.TypeReturn = exp.TypeReturn;
                else
                    node.TypeReturn += "_" + exp.TypeReturn;

            }
            return errors;
        }

        public override List<string> Visit(NullNode node)
        {
            node.TypeReturn = "null";
            return new List<string>();
        }

        public override List<string> Visit(MultiplyNode node)
        {
            return new List<string>();
        }

        public override List<string> Visit(CaseExpressionNode node)
        {
            var errors = new List<string>();
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
            for (int i = start; i <= end; i+=2)
            {
                if (headCaseType != null)
                {
                    if (headCaseType != node.Expressions[i].TypeReturn)
                        errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                            Sp.Name, node.Expressions[i].Line, node.Expressions[i].Column));
                }
                else
                    headCaseType = node.Expressions[i].TypeReturn;
                if (elseExpressionType != null)
                {
                    if (headCaseType != node.Expressions[i + 1].TypeReturn)
                        errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                            Sp.Name, node.Expressions[i + 1].Line, node.Expressions[i + 1].Column));
                }
                else
                    elseExpressionType = node.Expressions[i + 1].TypeReturn;
            }
            return errors;
        }

        public override List<string> Visit(ExistsNode node)
        {
            node.TypeReturn = "boolean";
            return VisitNode(node.SelectStmt);
        }

        public override List<string> Visit(IndirectionVarNode node)
        {
            var errors = new List<string>();
            if (VariablesTypes.ContainsKey(node.Identifiers.Text))
            {
                node.TypeReturn = VariablesTypes[node.Identifiers.Text];
                if (node.Indirections != null && node.Indirections.Count > 0)
                {
                    foreach (var indirection in node.Indirections)
                    {
                        if(indirection.ColLabel != null)
                        {
                            var table = Catalog.Tables.Values.First(t => t.Name == node.TypeReturn);
                            if (table != null)
                            {
                                var field = table.Fields.First(f => f.Name == indirection.ColLabel.Text);
                                if (field != null)
                                    node.TypeReturn = field.OriginType;
                                else
                                    errors.Add(String.Format("Semantic error SP:{} Line:{} Column:{}",
                                        Sp.Name, indirection.Line, indirection.Column));
                            }
                            else
                            {
                                var udt = Catalog.UserDefinedTypes.Values.First(t => t.TypeName == node.TypeReturn) as UserDefinedType;
                                if (table != null)
                                {
                                    var field = udt.Fields.First(f => f.Name == indirection.ColLabel.Text);
                                    if (field != null)
                                        node.TypeReturn = field.OriginType;
                                    else
                                        errors.Add(String.Format("Semantic error SP:{} Line:{} Column:{}",
                                            Sp.Name, indirection.Line, indirection.Column));
                                }
                            }
                        }
                    }
                }
            }
            else
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column));
            return errors;
        }

        public override List<string> Visit(ove node)
        {
            var errors = VisitNode(node.SelectStmtNonParens);
            node.TypeReturn = node.SelectStmtNonParens.TypeReturn;
            if(node.Indirections != null && node.Indirections.Count > 0)
                foreach (var indirection in node.Indirections)
                {
                    errors.AddRange(VisitNode(indirection));
                }
            return errors;
        }

        public override List<string> Visit(IntervalFieldNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(DatetimeOverlapsNode node)
        {
            var errors = VisitNode(node.Expression1)
                .Concat(VisitNode(node.Expression2))
                .Concat(VisitNode(node.Expression3))
                .Concat(VisitNode(node.Expression4)).ToList();
            node.TypeReturn = "boolean";
            return errors;
        }

        public override List<string> Visit(TableColsNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(UserNameNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(GroupByClauseNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(ArrayTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(ArrayToSelectNode node)
        {
            var errors = VisitNode(node.SelectStmt);
            node.TypeReturn = "array";
            return errors;
        }

        public override List<string> Visit(ArrayElementsNode node)
        {
            node.TypeReturn = "array";
            var errors = new List<string>();
            foreach (var exp in node.Elements)
            {
                errors.AddRange(VisitNode(exp));
            }
            return errors;
        }

        public override List<string> Visit(ComparisonModNode node)
        {
            var errors = new List<string>();

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

        public override List<string> Visit(BasicFunctionCallNode node)
        {
            var errors = new List<string>();

            var functionName = (node.SchemaQualifiednameNonType.IdentifierNonType.Text);
            var sps = Catalog.StoreProcedures.Values.Where(f => f.Name == functionName).ToList();
            var internalFunctions = Catalog.InternalFunctions.Values.Where(f => f.Name == functionName).ToList();

            if (sps.Count == 0 && internalFunctions.Count == 0)
                errors.Add(String.Format("Semeantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column));
            else
            {
                foreach (var vexOrName in node.VexOrNamedNotations)
                    errors.AddRange(VisitNode(vexOrName));
                StoreProcedure sp = null;
                foreach (var f in sps.Concat(internalFunctions))
                {
                    var bnd = false;
                    if(f.Params.Where(p => p.OutMode).ToList().Count == node.VexOrNamedNotations.Count)
                    {
                        bnd = true;
                        for (int i = 0; i < node.VexOrNamedNotations.Count; i++)
                            if (f.Params[i].OriginType != node.VexOrNamedNotations[i].TypeReturn)
                            {
                                bnd = false;
                                break;
                            }
                    }
                    if(bnd)
                    {
                        sp = f;
                        break;
                    }
                }
                if(sp == null)
                    errors.Add(String.Format("Semeantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column));
                node.TypeReturn =  sp.ReturnClause;
            }

            return errors;
        }

        public override List<string> Visit(FunctionConstructNode node)
        {
            var errors = new List<string>();
            foreach (var expression in node.Expressions)
                errors.AddRange(VisitNode(expression));

            if (node.Coalesce || node.Greatest || node.Least || node.Nullif || node.XmlConcat)
                node.TypeReturn = node.Expressions[0].TypeReturn;
            else if (node.Grouping)
                node.TypeReturn = "int4";
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

        public override List<string> Visit(ExtractFunctionNode node)
        {
            node.TypeReturn = "float8";
            return VisitNode(node.Expression);
        }

        public override List<string> Visit(TrimStringValueFunctionNode node)
        {
            var errors = VisitNode(node.Chars).Concat(VisitNode(node.Str)).ToList();
            if (node.Chars.TypeReturn != "varchar" || node.Str.TypeReturn != "varchar")
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column));
            node.TypeReturn = "varchar";
            return errors;
        }

        public override List<string> Visit(SubstringStringValueFunctionNode node)
        {
            var errors = new List<string>();
            foreach (var exp in node.Expressions)
            {
                errors.AddRange(VisitNode(exp));
            }
            var end = node.Expressions.Count - 1;
            if (node.For && node.Expressions[end].TypeReturn != "int4")
            {
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                    Sp.Name, node.Expressions[end].Line, node.Expressions[end].Column));
                end--;
            }
            if (node.From && node.Expressions[end].TypeReturn != "int4")
            {
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                    Sp.Name, node.Expressions[end].Line, node.Expressions[end].Column));
                end--;
            }
            for (int i = 0; i <= end; i++)
            {
                if(node.Expressions[i].TypeReturn != "varchar")
                    errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                    Sp.Name, node.Expressions[i].Line, node.Expressions[i].Column));
            }
            return errors;
        }

        public override List<string> Visit(PositionStringValueFunctionNode node)
        {
            var errors = VisitNode(node.ExpressionB).Concat(VisitNode(node.Expression)).ToList();

            if(node.ExpressionB.TypeReturn != "varchar")
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                    Sp.Name, node.ExpressionB.Line, node.ExpressionB.Column));
            if (node.Expression.TypeReturn != "varchar")
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                    Sp.Name, node.Expression.Line, node.Expression.Column));
            node.TypeReturn = "int4";
            return errors;
        }

        public override List<string> Visit(OverlayStringValueFunctionNode node)
        {
            var errors = new List<string>();
            foreach (var exp in node.Expressions)
            {
                errors.AddRange(VisitNode(exp));
            }
            var end = node.Expressions.Count-1;
            if (node.For && node.Expressions[end].TypeReturn != "int4")
            {
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                      Sp.Name, node.Expressions[end].Line, node.Expressions[end].Column));
                end--;
            }
            if (node.Expressions[end].TypeReturn != "int4")
            {
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                      Sp.Name, node.Expressions[end].Line, node.Expressions[end].Column));
                end--;
            }
            for (int i = 0; i <= end; i++)
            {
                if (node.Expressions[i].TypeReturn != "varchar")
                {
                    errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                          Sp.Name, node.Expressions[i].Line, node.Expressions[i].Column));
                }
            }
            node.TypeReturn = "varchar";
            return errors;
        }

        public override List<string> Visit(CollationStringValueFunctionNode node)
        {
            return VisitNode(node.Expression);
        }

        public override List<string> Visit(CastSpesificationSystemFunction node)
        {
            var errors = VisitNode(node.Expression);
            errors.AddRange(VisitNode(node.DataType));
            node.TypeReturn = node.DataType.TypeReturn;
            return errors;
        }

        public override List<string> Visit(XmlElementFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(XmlForestFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(XmlPiFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(XmlRootFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(XmlExistsFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(XmlParseFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(XmlSerializeFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(XmlTabletFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(XmlTableColumnNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(CurrentCatalogSystemFunctionNode node)
        {
            node.TypeReturn = "varchar";
            return new List<string>();
        }

        public override List<string> Visit(CurrentSchemaSystemFunctionNode node)
        {
            node.TypeReturn = "varchar";
            return new List<string>();
        }

        public override List<string> Visit(CurrentUserSystemFunctionNode node)
        {
            node.TypeReturn = "varchar";
            return new List<string>();
        }

        public override List<string> Visit(UserSystemFunctionNode node)
        {
            node.TypeReturn = "varchar";
            return new List<string>();
        }

        public override List<string> Visit(SessionUserSystemFunctionNode node)
        {
            node.TypeReturn = "varchar";
            return new List<string>();
        }

        public override List<string> Visit(CurrentDateFunctionNode node)
        {
            node.TypeReturn = "date";
            return new List<string>();
        }

        public override List<string> Visit(CurrentTimeFunctionNode node)
        {
            node.TypeReturn = "time";
            return new List<string>();
        }

        public override List<string> Visit(LocalTimeFunctionNode node)
        {
            node.TypeReturn = "time";
            return new List<string>();
        }

        public override List<string> Visit(CurrentTimestampFunctionNode node)
        {
            node.TypeReturn = "timestamp";
            return new List<string>();
        }

        public override List<string> Visit(LocalTimestampFunctionNode node)
        {
            node.TypeReturn = "timestamp";
            return new List<string>();
        }

        public override List<string> Visit(BoolNode node)
        {
            node.TypeReturn = "boolean";
            return new List<string>();
        }

        public override List<string> Visit(DefaultNode node)
        {
            return new List<string>();
        }

        public override List<string> Visit(Float4Node node)
        {
            node.TypeReturn = "float8";
            return new List<string>();
        }

        public override List<string> Visit(Float8Node node)
        {
            node.TypeReturn = "float8";
            return new List<string>();
        }

        public override List<string> Visit(Int8Node node)
        {
            node.TypeReturn = "int8";
            return new List<string>();
        }

        public override List<string> Visit(IntNode node)
        {
            node.TypeReturn = "int4";
            return new List<string>();
        }

        public override List<string> Visit(VarcharNode node)
        {
            node.TypeReturn = "varchar";
            return new List<string>();
        }

        public override List<string> Visit(SelectOpsNoParensNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(SelectOpsNode node)
        {
            if (node.SelectStmt != null)
                return VisitNode(node.SelectStmt);
            else if (node.SelectPrimary != null)
                return VisitNode(node.SelectPrimary);
            else
            {
                var errors = new List<string>();
                foreach (var selectOps in node.SelectOps)
                    errors.AddRange(VisitNode(selectOps));
                return errors;
            }
        }

        public override List<string> Visit(SelectPrimaryNode node)
        {
            if (node.SchemaQualifield != null)
            {
                var errors = VisitNode(node.SchemaQualifield);
                node.TypeReturn = node.SchemaQualifield.TypeReturn;
                return errors;
            }
            else
            {
                var errors = new List<string>();
                var newSemanticVisitor = new SemanticVisitor(Catalog, Sp, VariablesTypes);
                if (node.FromItems != null && node.FromItems.Count > 0)
                {
                    foreach (var fromItem in node.FromItems)
                    {
                        errors.AddRange(newSemanticVisitor.VisitNode(fromItem));
                    }
                }

                if (node.SelectList != null)
                {
                    errors.AddRange(newSemanticVisitor.VisitNode(node.SelectList));
                    node.TypeReturn = node.SelectList.TypeReturn;
                }

                if (node.IntoTable != null)
                {
                    errors.AddRange(newSemanticVisitor.VisitNode(node.IntoTable));
                    node.TypeReturn = node.IntoTable.TypeReturn;
                }

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

        public override List<string> Visit(SelectListNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(SelectSubListNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(IdNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(SchemaQualifieldNode node)
        {
            var errors = new List<string>();
            Table table = null;
            BaseUserDefinedType udt = null;
            var initialType = "";
            foreach (var identifier in node.Identifiers)
            {
                if (initialType == "")
                {
                    if (VariablesTypes.ContainsKey(identifier.Text))
                        initialType = VariablesTypes[identifier.Text];
                    else
                    {
                        table = Catalog.Tables.Values.First(t => t.Name == identifier.Text);
                        udt = Catalog.UserDefinedTypes.Values.First(u => u.TypeName == identifier.Text);
                        if (table != null)
                            initialType = table.Name;
                        else if (udt != null)
                            initialType = udt.TypeName;
                        else
                            errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, identifier.Line, identifier.Column));
                    }
                }
                else
                {
                    if (table != null)
                    {
                        var fieldT = table.Fields.First(f => f.Name == identifier.Text);
                        if (fieldT != null)
                            initialType = fieldT.OriginType;
                        else
                            errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, identifier.Line, identifier.Column));
                    }
                    else if (udt != null && udt.IsUDT)
                    {
                        var fieldUDT = (udt as UserDefinedType).Fields.First(f => f.Name == identifier.Text);
                        if (fieldUDT != null)
                            initialType = fieldUDT.OriginType;
                        else
                            errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, identifier.Line, identifier.Column));
                    }
                    else
                    {
                        if (VariablesTypes.ContainsKey(identifier.Text))
                            initialType = VariablesTypes[identifier.Text];
                        else
                        {
                            table = Catalog.Tables.Values.First(t => t.Name == identifier.Text);
                            udt = Catalog.UserDefinedTypes.Values.First(u => u.TypeName == identifier.Text);
                            if (table != null)
                                initialType = table.Name;
                            else if (udt != null)
                                initialType = udt.TypeName;
                            else
                                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, identifier.Line, identifier.Column));
                        }
                    }
                }
            }
            node.TypeReturn = initialType;
            return errors;
        }

        public override List<string> Visit(SchemaQualifiednameNonTypeNode node)
        {
            var initialType = "";
            if (node.Schema != null)
            {
                if (VariablesTypes.ContainsKey(node.Schema.Text))
                    initialType = VariablesTypes[node.Schema.Text];
                else
                    initialType = node.Schema.Text;
            }
            if (node.IdentifierNonType != null)
            {
                if (initialType == "")
                {
                    if (VariablesTypes.ContainsKey(node.IdentifierNonType.Text))
                        initialType = VariablesTypes[node.IdentifierNonType.Text];
                    else
                    {
                        var table = Catalog.Tables.Values.First(t => t.Name == node.IdentifierNonType.Text);
                        var udt = Catalog.UserDefinedTypes.Values.First(u => u.TypeName == node.IdentifierNonType.Text);
                        if (table != null)
                            initialType = table.Name;
                        else if (udt != null)
                            initialType = udt.TypeName;
                    }
                }
            }
            node.TypeReturn = initialType;
            return new List<string>();
        }

        public override List<string> Visit(FrameBoundNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(FrameClauseNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(FromItemSimpleNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(FromItemCrossJoinNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(FromItemOnExpressionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(FromItemUsingNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(FromItemNaturalNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(ExceptionStatementNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(ExecuteStatementNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(TransactionStatementNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(FromPrimary1Node node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(FromPrimary2Node node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(FromPrimary3Node node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(FromPrimary4Node node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(FromFunctionColumnDefNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(AliasClauseNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(VexOrNamedNotationNode node)
        {
            var errors = VisitNode(node.Expression);
            node.TypeReturn = node.Expression.TypeReturn;
            return errors;
        }

        public override List<string> Visit(ArgumentNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(IndirectionIdentifierNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(IndirectionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(ReturnStmtNode node)
        {
            var errors = new List<string>();
            if (node.Stmt != null)
                errors.AddRange(VisitNode(node.Stmt));
            if (node.Expression != null)
                errors.AddRange(VisitNode(node.Expression));
            return errors;
        }

        public override List<string> Visit(PerformStmtNode node)
        {
            var errors = new List<string>();
            var newSemanticVisitor = new SemanticVisitor(Catalog, Sp, VariablesTypes);
            if (node.FromItems != null && node.FromItems.Count > 0)
            {
                foreach (var fromItem in node.FromItems)
                {
                    errors.AddRange(newSemanticVisitor.VisitNode(fromItem));
                }
            }

            if (node.SelectList != null)
            {
                errors.AddRange(newSemanticVisitor.VisitNode(node.SelectList));
                node.TypeReturn = node.SelectList.TypeReturn;
            }

            if (node.IntoTable != null)
            {
                errors.AddRange(newSemanticVisitor.VisitNode(node.IntoTable));
                node.TypeReturn = node.IntoTable.TypeReturn;
            }

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

        public override List<string> Visit(IfStmtNode node)
        {
            var errors = new List<string>();
            foreach (var exp in node.Expressions)
            {
                errors.AddRange(VisitNode(exp));
                if (exp.TypeReturn != "boolean")
                    errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, exp.Line, exp.Column));
            }
            foreach (var statements in node.Statements)
                foreach (var statement in statements)
                    errors.AddRange(VisitNode(statement));
            foreach (var statement in node.ElseStatements)
                errors.AddRange(VisitNode(statement));
            return errors;
        }

        public override List<string> Visit(AssingStmtNode node)
        {
            var errors = VisitNode(node.Var).Concat(VisitNode(node.Stmt)).ToList();
            var type = "";
            if (node.Var.Id != null && VariablesTypes.ContainsKey(node.Var.Id.Text))
                type = VariablesTypes[node.Var.Id.Text];
            if (node.Var.SchemaQualifield != null && VariablesTypes.ContainsKey(node.Var.SchemaQualifield.Identifiers[0].Text))
                type = VariablesTypes[node.Var.SchemaQualifield.Identifiers[0].Text];
            if ((node.Stmt is PerformStmtNode && (node.Stmt as PerformStmtNode).TypeReturn != type) ||
                node.Stmt is SelectStmtNonParensNode && (node.Stmt as SelectStmtNonParensNode).TypeReturn != type)
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Stmt.Line, node.Stmt.Column));

            return errors;
        }

        public override List<string> Visit(VarNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(RaiseMessageStatementNode node)
        {
            var errors = new List<string>();
            if (node.Expressions != null)
                foreach (var exp in node.Expressions)
                    errors.AddRange(VisitNode(exp));
            if (node.RaiseUsing != null)
                foreach (var exp in node.RaiseUsing.Expressions)
                    errors.AddRange(VisitNode(exp));
            return errors;
        }

        public override List<string> Visit(AssertMessageStatementNode node)
        {
            var errors = new List<string>();
            foreach (var exp in node.Expressions)
                errors.AddRange(VisitNode(exp));
            return errors;
        }

        public override List<string> Visit(IdentifierNonTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(DeleteStmtPSqlNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(OpCharsNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(LoopStmtNode node)
        {
            var errors = new List<string>();
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
                        errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                            Sp.Name, loopStart.Line, loopStart.Column));
                    else
                        for (int i = 0; i < loopStart.Identifiers.Count; i++)
                            localVariables.Add((loopStart.Identifiers[i].Text, stmtTypeReturn[i]));
                }
                else if (node.LoopStart is ForCursorLoopNode)
                {
                    var loopStar = node.LoopStart as ForCursorLoopNode;
                    var cursorName = VariablesTypes.Keys.Where(s => s.StartsWith(loopStar.Identifier.Text)).First();
                    localVariables.Add((loopStar.Cursor.Text, VariablesTypes[cursorName].Split('_').Last()));
                }
                else if (node.LoopStart is ForeachLoopNode)
                {
                    var loopStart = node.LoopStart as ForeachLoopNode;
                    var types = loopStart.Expression.TypeReturn.Split('_');
                    if (loopStart.Identifiers.Count != types.Length)
                        errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Line, node.Column));
                    else
                        for (int i = 0; i < loopStart.Identifiers.Count; i++)
                        {
                            localVariables.Add((loopStart.Identifiers[i].Text, types[i]));
                        }
                }
            }

            var newVariablesTypes = new Dictionary<string, string>(VariablesTypes);
            foreach (var variable in localVariables)
                newVariablesTypes.Add(variable.Item1, variable.Item2);
            
            var newSemanticVisitor = new SemanticVisitor(Catalog, Sp, newVariablesTypes);
            if(node.Statemets != null)
                foreach (var statement in node.Statemets)
                    errors.AddRange(newSemanticVisitor.VisitNode(statement));
            if (node.Expression != null)
                errors.AddRange(VisitNode(node.Expression));
            return errors;
        }

        public override List<string> Visit(OpNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(ConflictObjectNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(ConflictActionNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(DeclareStatementNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(ModularTypeDeclarationNode node)
        {
            var errors = new List<string>(VisitNode(node.Schema));
            node.TypeReturn = node.Schema.TypeReturn;
            return errors;
        }

        public override List<string> Visit(ModularRowTypeDeclarationNode node)
        {
            var errors = new List<string>(VisitNode(node.Schema));
            node.TypeReturn = node.Schema.TypeReturn;
            return errors;
        }

        public override List<string> Visit(AnalizeModeNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(WhileLoopNode node)
        {
            var errors = VisitNode(node.Expressions[0]);
            if (node.Expressions[0].TypeReturn != "boolean")
                errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}",
                    Sp.Name, node.Expressions[0].Line, node.Expressions[0].Column));
            return errors;
        }

        public override List<string> Visit(ForAliasLoopNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(ForIdListLoopNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(ForCursorLoopNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(ForeachLoopNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(InsertStmtPSqlNode node)
        {
            var errors = new List<string>();
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

        public override List<string> Visit(AfterOpsFetchNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(AfterOpsForNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(AfterOpsLimitNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(AfterOpsOffsetNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(OrderByClauseNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(SortSpecifierNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(OrderSpecificationNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(AllOpRefNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(ExecuteStmtNode node)
        {
            var errors = VisitNode(node.Expression);
            if(node.UsingExpression != null)
                foreach (var exp in node.UsingExpression)
                    errors.AddRange(VisitNode(exp));
            node.TypeReturn = node.Expression.TypeReturn;
            return errors;
        }

        public override List<string> Visit(UpdateStmtPSqlNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(ValuesStmtNode node)
        {
            var errors = new List<string>();
            var returnType = "";
            foreach (var valuesValues in node.Values)
            {
                errors.AddRange(VisitNode(valuesValues));
                returnType += (returnType == "") ? valuesValues.TypeReturn : "_" + valuesValues.TypeReturn;
            }
            node.TypeReturn = returnType;
            return errors;
        }

        public override List<string> Visit(UpdateSetNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(ValuesValuesNode node)
        {
            var errors = new List<string>();
            var typeReturn = "";
            foreach (var exp in node.Expressions)
            {
                errors.AddRange(VisitNode(exp));
                typeReturn += (typeReturn == "") ? exp.TypeReturn : "_" + exp.TypeReturn;
            }
            node.TypeReturn = typeReturn;
            return errors;
        }

        public override List<string> Visit(CursorStatementNode node)
        {
            var errors = new List<string>();
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
                if (!VariablesTypes.ContainsKey(node.Var.SchemaQualifield.Identifiers[0].Text))
                    errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Var.Line, node.Var.Column));
                else
                {
                    var cursorTypeReturn = VariablesTypes[node.Var.SchemaQualifield.Identifiers[0].Text].Split('_').Last();
                    if (node.IntoTable != null)
                    {
                        errors.AddRange(VisitNode(node.IntoTable));
                        if (!VariablesTypes.ContainsKey(node.IntoTable.Identifiers[0].Text) &&
                            cursorTypeReturn != VariablesTypes[node.IntoTable.Identifiers[0].Text])
                            errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Var.Line, node.Var.Column));
                    }
                }
            }
            else if (node.Move)
            {
                errors.AddRange(VisitNode(node.Var));
                if (!VariablesTypes.ContainsKey(node.Var.SchemaQualifield.Identifiers[0].Text))
                    errors.Add(String.Format("Semantic error SP:{0} Line:{1} Column:{2}", Sp.Name, node.Var.Line, node.Var.Column));
            }
            return errors;
        }

        public override List<string> Visit(OptionNode node)
        {
            var errors = VisitNode(node.Expression);
            node.TypeReturn = node.Expression.TypeReturn;
            return errors;
        }

        public override List<string> Visit(IsNotDistinctFromNode node)
        {
            throw new NotImplementedException();
        }

        public override List<string> Visit(AllSimpleOpNode node)
        {
            throw new NotImplementedException();
        }
    }
}