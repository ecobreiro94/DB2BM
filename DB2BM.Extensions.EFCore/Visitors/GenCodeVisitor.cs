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
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.EFCore.Visitors
{
    public class GenCodeVisitor : ASTVisitor<string>
    {
        DatabaseCatalog Catalog;
        StoreProcedure Sp;

        public GenCodeVisitor(DatabaseCatalog catalog, StoreProcedure sp)
        {
            Catalog = catalog;
            Sp = sp;
        }
        public override string Visit(FunctionBlockNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AliasDeclarationNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(CursorDeclarationNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(OrdinalTypeDeclarationNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IsNotDistinctFromNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AllSimpleOpNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(OtherTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(OtherOpBinaryNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(DataTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(NCharTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(NCharVaryingTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(NumericTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(RealTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(BigintTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(SmallintTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(BitTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(TimeTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(BitVaryingTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(VarcharTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(BooleanTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(DecTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(DecimalTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(DollarNumberNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(DoublePrecisionTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AtTimeZoneNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(FloatTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IntTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IntegerTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IntervalTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(CharTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(CharVaryingTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AddAnalizeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AddClusterNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AddDeallocatteNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AddListenNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AddPrepareNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AddReassignNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AddRefreshNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AddReindexNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AddResetNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AddUnlistenNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(CopyStmtNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ExplainStmtNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ShowStmtNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(CallFunctionCallNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(WithClauseNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(WithQueryNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(CaseStmtNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(WindowsDefinitionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(NullOrderingNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(CastExpressionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IsNullNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IsNotNullNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(BetweenNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(InNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(OfNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(SelectStatementNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(SelectStmtNonParensNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(TruncateStmtNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IsTrueNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IsNotTrueNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IsFalseNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IsNotFalseNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(CollateNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(LikeBinaryNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ILikeBinaryNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(NotLikeBinaryNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(NotILikeBinaryNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(SimilarToBinaryNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(NotSimilarToBinaryNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(NotNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AbsoluteValueNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(BitwiseNotNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(FactorialNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(OtherOpUnaryNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IsDocumentNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IsNotDocumentNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IsUnknownNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IsNotUnknownNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(BaseTypeCoercionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IntervalTypeCoercionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(EqualNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(GreaterEqualNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(GreaterNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IsDistinctFromNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(LessEqualNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(LessNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(NotEqualNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ExponentiationNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ModuloNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(MultiplicationNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(StringConcatNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(SubtractionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AdditionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(BitwiseAndNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(BitwiseOrNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(BitwiseShiftLeftNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(BitwiseShiftRightNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(BitwiseXorNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(DivisionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AndNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(OrNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ExpInDirectionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ExpressionListNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(NullNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(MultiplyNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(CaseExpressionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ExistsNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IndirectionVarNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ove node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IntervalFieldNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(DatetimeOverlapsNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(TableColsNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(UserNameNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(GroupByClauseNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ArrayTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ArrayToSelectNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ArrayElementsNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ComparisonModNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(BasicFunctionCallNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(FunctionConstructNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ExtractFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(TrimStringValueFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(SubstringStringValueFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(PositionStringValueFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(OverlayStringValueFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(CollationStringValueFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(CastSpesificationSystemFunction node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(XmlElementFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(XmlForestFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(XmlPiFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(XmlRootFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(XmlExistsFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(XmlParseFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(XmlSerializeFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(XmlTabletFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(XmlTableColumnNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(CurrentCatalogSystemFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(CurrentSchemaSystemFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(CurrentUserSystemFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(UserSystemFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(SessionUserSystemFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(CurrentDateFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(CurrentTimeFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(LocalTimeFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(CurrentTimestampFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(LocalTimestampFunctionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(BoolNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(DefaultNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(Float4Node node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(Float8Node node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(Int8Node node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IntNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(VarcharNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(SelectOpsNoParensNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(SelectOpsNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(SelectPrimaryNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(SelectListNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(SelectSubListNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IdNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(SchemaQualifieldNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(SchemaQualifiednameNonTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(FrameBoundNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(FrameClauseNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(FromItemSimpleNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(FromItemCrossJoinNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(FromItemOnExpressionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(FromItemUsingNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(FromItemNaturalNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ExceptionStatementNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ExecuteStatementNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(TransactionStatementNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(FromPrimary1Node node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(FromPrimary2Node node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(FromPrimary3Node node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(FromPrimary4Node node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(FromFunctionColumnDefNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AliasClauseNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(VexOrNamedNotationNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ArgumentNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IndirectionIdentifierNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IndirectionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ReturnStmtNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(PerformStmtNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IfStmtNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AssingStmtNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(VarNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(RaiseMessageStatementNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AssertMessageStatementNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(IdentifierNonTypeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(DeleteStmtPSqlNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(OpCharsNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(LoopStmtNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(OpNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ConflictObjectNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ConflictActionNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(DeclarationNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(DeclareStatementNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ModularTypeDeclarationNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ModularRowTypeDeclarationNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AnalizeModeNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(WhileLoopNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ForAliasLoopNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ForIdListLoopNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ForCursorLoopNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ForeachLoopNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(InsertStmtPSqlNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AfterOpsFetchNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AfterOpsForNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AfterOpsLimitNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AfterOpsOffsetNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(OrderByClauseNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(SortSpecifierNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(OrderSpecificationNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(AllOpRefNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ExecuteStmtNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(UpdateStmtPSqlNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ValuesStmtNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(UpdateSetNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(ValuesValuesNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(CursorStatementNode node)
        {
            throw new NotImplementedException();
        }

        public override string Visit(OptionNode node)
        {
            throw new NotImplementedException();
        }
    }
}
