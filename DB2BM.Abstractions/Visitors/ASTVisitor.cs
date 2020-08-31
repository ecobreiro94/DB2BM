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
using System.Linq;
using System.Reflection;

namespace DB2BM.Abstractions.Visitors
{
    public abstract class ASTVisitor<TResult>
    {
        public TResult VisitNode(IVisitable node)
        {
            return node.Accept<TResult>(this);
        }

        
        public abstract TResult Visit(FunctionBlockNode node);//

        public abstract TResult Visit(AliasDeclarationNode node);//

        public abstract TResult Visit(CursorDeclarationNode node);//

        public abstract TResult Visit(OrdinalTypeDeclarationNode node);//

        public abstract TResult Visit(IsNotDistinctFromNode node);//

        public abstract TResult Visit(AllSimpleOpNode node);//

        public abstract TResult Visit(OtherTypeNode node);//

        public abstract TResult Visit(OtherOpBinaryNode node);//

        public abstract TResult Visit(DataTypeNode node);//

        public abstract TResult Visit(NCharTypeNode node);//

        public abstract TResult Visit(NCharVaryingTypeNode node);//

        public abstract TResult Visit(NumericTypeNode node);//

        public abstract TResult Visit(RealTypeNode node);//

        public abstract TResult Visit(BigintTypeNode node);//

        public abstract TResult Visit(SmallintTypeNode node);//

        public abstract TResult Visit(BitTypeNode node);//

        public abstract TResult Visit(TimeTypeNode node);//

        public abstract TResult Visit(BitVaryingTypeNode node);//

        public abstract TResult Visit(VarcharTypeNode node);//

        public abstract TResult Visit(BooleanTypeNode node);//
        
        public abstract TResult Visit(DecTypeNode node);//

        public abstract TResult Visit(DecimalTypeNode node);//

        public abstract TResult Visit(DollarNumberNode node);//

        public abstract TResult Visit(DoublePrecisionTypeNode node);//

        public abstract TResult Visit(AtTimeZoneNode node);//

        public abstract TResult Visit(FloatTypeNode node);//

        public abstract TResult Visit(IntTypeNode node);//

        public abstract TResult Visit(IntegerTypeNode node);//

        public abstract TResult Visit(IntervalTypeNode node);//

        public abstract TResult Visit(CharTypeNode node);//

        public abstract TResult Visit(CharVaryingTypeNode node);//

        public abstract TResult Visit(AddAnalizeNode node);//

        public abstract TResult Visit(AddClusterNode node);//

        public abstract TResult Visit(AddDeallocatteNode node);//

        public abstract TResult Visit(AddListenNode node);//

        public abstract TResult Visit(AddPrepareNode node);//

        public abstract TResult Visit(AddReassignNode node);//

        public abstract TResult Visit(AddRefreshNode node);//

        public abstract TResult Visit(AddReindexNode node);//

        public abstract TResult Visit(AddResetNode node);//

        public abstract TResult Visit(AddUnlistenNode node);//

        public abstract TResult Visit(CopyStmtNode node);//

        public abstract TResult Visit(ExplainStmtNode node);//

        public abstract TResult Visit(ShowStmtNode node);//

        public abstract TResult Visit(CallFunctionCallNode node);//

        public abstract TResult Visit(WithClauseNode node);//

        public abstract TResult Visit(WithQueryNode node);//

        public abstract TResult Visit(CaseStmtNode node);//

        public abstract TResult Visit(WindowsDefinitionNode node);//

        public abstract TResult Visit(NullOrderingNode node);//

        public abstract TResult Visit(CastExpressionNode node);//
        
        public abstract TResult Visit(IsNullNode node);//

        public abstract TResult Visit(IsNotNullNode node);//

        public abstract TResult Visit(BetweenNode node);//

        public abstract TResult Visit(InNode node);//

        public abstract TResult Visit(OfNode node);//

        public abstract TResult Visit(SelectStatementNode node);//

        public abstract TResult Visit(SelectStmtNonParensNode node);//

        public abstract TResult Visit(TruncateStmtNode node);//

        public abstract TResult Visit(IsTrueNode node);//

        public abstract TResult Visit(IsNotTrueNode node);//

        public abstract TResult Visit(IsFalseNode node);//

        public abstract TResult Visit(IsNotFalseNode node);//

        public abstract TResult Visit(CollateNode node);//

        public abstract TResult Visit(LikeBinaryNode node);//

        public abstract TResult Visit(ILikeBinaryNode node);//

        public abstract TResult Visit(NotLikeBinaryNode node);//

        public abstract TResult Visit(NotILikeBinaryNode node);//

        public abstract TResult Visit(SimilarToBinaryNode node);//

        public abstract TResult Visit(NotSimilarToBinaryNode node);//

        public abstract TResult Visit(NotNode node);//

        public abstract TResult Visit(AbsoluteValueNode node);//

        public abstract TResult Visit(BitwiseNotNode node);//

        public abstract TResult Visit(FactorialNode node);//

        public abstract TResult Visit(OtherOpUnaryNode node);//

        public abstract TResult Visit(IsDocumentNode node);//
        
        public abstract TResult Visit(IsNotDocumentNode node);//

        public abstract TResult Visit(IsUnknownNode node);//

        public abstract TResult Visit(IsNotUnknownNode node);//

        public abstract TResult Visit(BaseTypeCoercionNode node);//

        public abstract TResult Visit(IntervalTypeCoercionNode node);//

        public abstract TResult Visit(EqualNode node);//

        public abstract TResult Visit(GreaterEqualNode node);//

        public abstract TResult Visit(GreaterNode node);//

        public abstract TResult Visit(IsDistinctFromNode node);//

        public abstract TResult Visit(LessEqualNode node);//

        public abstract TResult Visit(LessNode node);//

        public abstract TResult Visit(NotEqualNode node);//

        public abstract TResult Visit(ExponentiationNode node);//

        public abstract TResult Visit(ModuloNode node);//

        public abstract TResult Visit(MultiplicationNode node);//

        public abstract TResult Visit(StringConcatNode node);//

        public abstract TResult Visit(SubtractionNode node);//

        public abstract TResult Visit(AdditionNode node);//

        public abstract TResult Visit(BitwiseAndNode node);//

        public abstract TResult Visit(BitwiseOrNode node);//

        public abstract TResult Visit(BitwiseShiftLeftNode node);//

        public abstract TResult Visit(BitwiseShiftRightNode node);//

        public abstract TResult Visit(BitwiseXorNode node);//

        public abstract TResult Visit(DivisionNode node);//

        public abstract TResult Visit(AndNode node);//

        public abstract TResult Visit(OrNode node);//


        public abstract TResult Visit(ExpInDirectionNode node);//

        public abstract TResult Visit(ExpressionListNode node);//

        public abstract TResult Visit(NullNode node);//

        public abstract TResult Visit(MultiplyNode node);//

        public abstract TResult Visit(CaseExpressionNode node);//

        public abstract TResult Visit(ExistsNode node);//

        public abstract TResult Visit(IndirectionVarNode node);//

        public abstract TResult Visit(ValueExpressionPrimaryNode node);//
        
        public abstract TResult Visit(IntervalFieldNode node);//

        public abstract TResult Visit(DatetimeOverlapsNode node);//

        public abstract TResult Visit(TableColsNode node);//

        public abstract TResult Visit(UserNameNode node);//

        public abstract TResult Visit(GroupByClauseNode node);//

        public abstract TResult Visit(ArrayTypeNode node);//

        public abstract TResult Visit(ArrayToSelectNode node);//

        public abstract TResult Visit(ArrayElementsNode node);//

        public abstract TResult Visit(ComparisonModNode node);//

        public abstract TResult Visit(BasicFunctionCallNode node);//

        public abstract TResult Visit(FunctionConstructNode node);//

        public abstract TResult Visit(ExtractFunctionNode node);//

        public abstract TResult Visit(TrimStringValueFunctionNode node);//

        public abstract TResult Visit(SubstringStringValueFunctionNode node);//

        public abstract TResult Visit(PositionStringValueFunctionNode node);//

        public abstract TResult Visit(OverlayStringValueFunctionNode node);//

        public abstract TResult Visit(CollationStringValueFunctionNode node);//

        public abstract TResult Visit(CastSpesificationSystemFunction node);//

        public abstract TResult Visit(XmlElementFunctionNode node);//

        public abstract TResult Visit(XmlForestFunctionNode node);//

        public abstract TResult Visit(XmlPiFunctionNode node);//

        public abstract TResult Visit(XmlRootFunctionNode node);//

        public abstract TResult Visit(XmlExistsFunctionNode node);//

        public abstract TResult Visit(XmlParseFunctionNode node);//

        public abstract TResult Visit(XmlSerializeFunctionNode node);//

        public abstract TResult Visit(XmlTabletFunctionNode node);//

        public abstract TResult Visit(XmlTableColumnNode node);//

        public abstract TResult Visit(CurrentCatalogSystemFunctionNode node);//

        public abstract TResult Visit(CurrentSchemaSystemFunctionNode node);//

        public abstract TResult Visit(CurrentUserSystemFunctionNode node);//

        public abstract TResult Visit(UserSystemFunctionNode node);//

        public abstract TResult Visit(SessionUserSystemFunctionNode node);//

        public abstract TResult Visit(CurrentDateFunctionNode node);//

        public abstract TResult Visit(CurrentTimeFunctionNode node);//

        public abstract TResult Visit(LocalTimeFunctionNode node);//

        public abstract TResult Visit(CurrentTimestampFunctionNode node);//

        public abstract TResult Visit(LocalTimestampFunctionNode node);//
        
        public abstract TResult Visit(BoolNode node);//

        public abstract TResult Visit(DefaultNode node);//

        public abstract TResult Visit(Float4Node node);//

        public abstract TResult Visit(Float8Node node);//

        public abstract TResult Visit(Int8Node node);//

        public abstract TResult Visit(IntNode node);//

        public abstract TResult Visit(VarcharNode node);//

        public abstract TResult Visit(SelectOpsNoParensNode node);//

        public abstract TResult Visit(SelectOpsNode node);//

        public abstract TResult Visit(SelectPrimaryNode node);//

        public abstract TResult Visit(SelectListNode node);//

        public abstract TResult Visit(SelectSubListNode node);//

        public abstract TResult Visit(IdNode node);//

        public abstract TResult Visit(SchemaQualifieldNode node);//

        public abstract TResult Visit(SchemaQualifiednameNonTypeNode node);//

        public abstract TResult Visit(FrameBoundNode node);//

        public abstract TResult Visit(FrameClauseNode node);//

        public abstract TResult Visit(FromItemSimpleNode node);//

        public abstract TResult Visit(FromItemCrossJoinNode node);//

        public abstract TResult Visit(FromItemOnExpressionNode node);//

        public abstract TResult Visit(FromItemUsingNode node);//
        
        public abstract TResult Visit(FromItemNaturalNode node);//

        public abstract TResult Visit(ExceptionStatementNode node);//

        public abstract TResult Visit(ExecuteStatementNode node);//

        public abstract TResult Visit(TransactionStatementNode node);//

        public abstract TResult Visit(FromPrimary1Node node);//

        public abstract TResult Visit(FromPrimary2Node node);//

        public abstract TResult Visit(FromPrimary3Node node);//

        public abstract TResult Visit(FromPrimary4Node node);//

        public abstract TResult Visit(FromFunctionColumnDefNode node);//

        public abstract TResult Visit(AliasClauseNode node);//

        public abstract TResult Visit(VexOrNamedNotationNode node);//

        public abstract TResult Visit(ArgumentNode node);//

        public abstract TResult Visit(IndirectionIdentifierNode node);//

        public abstract TResult Visit(IndirectionNode node);//

        public abstract TResult Visit(ReturnStmtNode node);//

        public abstract TResult Visit(PerformStmtNode node);//

        public abstract TResult Visit(IfStmtNode node);//
        
        public abstract TResult Visit(AssingStmtNode node);//

        public abstract TResult Visit(VarNode node);//

        public abstract TResult Visit(RaiseMessageStatementNode node);//

        public abstract TResult Visit(AssertMessageStatementNode node);//

        public abstract TResult Visit(IdentifierNonTypeNode node);//

        public abstract TResult Visit(DeleteStmtPSqlNode node);//

        public abstract TResult Visit(OpCharsNode node);//

        public abstract TResult Visit(LoopStmtNode node);//

        public abstract TResult Visit(OpNode node);//

        public abstract TResult Visit(ConflictObjectNode node);//

        public abstract TResult Visit(ConflictActionNode node);//

        public abstract TResult Visit(DeclarationNode node);//

        public abstract TResult Visit(DeclareStatementNode node);//

        public abstract TResult Visit(ModularTypeDeclarationNode node);//

        public abstract TResult Visit(ModularRowTypeDeclarationNode node);//

        public abstract TResult Visit(AnalizeModeNode node);//

        public abstract TResult Visit(WhileLoopNode node);//
        
        public abstract TResult Visit(ForAliasLoopNode node);//

        public abstract TResult Visit(ForIdListLoopNode node);//

        public abstract TResult Visit(ForCursorLoopNode node);//

        public abstract TResult Visit(ForeachLoopNode node);//
        
        public abstract TResult Visit(InsertStmtPSqlNode node);//

        public abstract TResult Visit(AfterOpsFetchNode node);//

        public abstract TResult Visit(AfterOpsForNode node);//

        public abstract TResult Visit(AfterOpsLimitNode node);//

        public abstract TResult Visit(AfterOpsOffsetNode node);//

        public abstract TResult Visit(OrderByClauseNode node);//

        public abstract TResult Visit(SortSpecifierNode node);//

        public abstract TResult Visit(OrderSpecificationNode node);//

        public abstract TResult Visit(AllOpRefNode node);//

        public abstract TResult Visit(ExecuteStmtNode node);//

        public abstract TResult Visit(UpdateStmtPSqlNode node);//

        public abstract TResult Visit(ValuesStmtNode node);

        public abstract TResult Visit(UpdateSetNode node);

        public abstract TResult Visit(ValuesValuesNode node);//

        public abstract TResult Visit(CursorStatementNode node);//

        public abstract TResult Visit(OptionNode node);//
    }
}
