using DB2BM.Abstractions.AST.Expressions;
using DB2BM.Abstractions.AST.Statements;
using DB2BM.Abstractions.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions.AST.Types
{
    public abstract class PredefinedTypeNode : ASTNode
    {
        public PredefinedTypeNode(int line, int column) : base(line, column)
        {
        }
    }

    public class BigintTypeNode : PredefinedTypeNode
    {
        public BigintTypeNode(int line, int column) : base(line, column)
        {
        }

        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class BitTypeNode : PredefinedTypeNode
    {
        public BitTypeNode(int line, int column) : base(line, column)
        {
        }
        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class BitVaryingTypeNode : PredefinedTypeNode
    {
        public int Length { get; set; }

        public BitVaryingTypeNode(int line, int column) : base(line, column)
        {
        }
        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class BooleanTypeNode : PredefinedTypeNode
    {
        public BooleanTypeNode(int line, int column) : base(line, column)
        {
        }
        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class DecTypeNode : PredefinedTypeNode
    {
        public int Precision { get; set; }
        public int Scale { get; set; }
        public DecTypeNode(int line, int column) : base(line, column)
        {
        }
        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public class DecimalTypeNode : PredefinedTypeNode
    {
        public int Precision { get; set; }
        public int Scale { get; set; }

        public DecimalTypeNode(int line, int column) : base(line, column)
        {
        }
        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class DoublePrecisionTypeNode : PredefinedTypeNode
    {
        public DoublePrecisionTypeNode(int line, int column) : base(line, column)
        {
        }
        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class FloatTypeNode : PredefinedTypeNode
    {
        public int Precision { get; set; }
        public int Scale { get; set; }

        public FloatTypeNode(int line, int column) : base(line, column)
        {
        }
        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public class IntTypeNode : PredefinedTypeNode
    {
        public IntTypeNode(int line, int column) : base(line, column)
        {
        }
        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class IntegerTypeNode : PredefinedTypeNode
    {
        public IntegerTypeNode(int line, int column) : base(line, column)
        {
        }
        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class IntervalTypeNode : PredefinedTypeNode
    {
        public int Lenght { get; set; }
        public IntervalFieldNode Interval { get; set; }
        public IntervalTypeNode(int line, int column) : base(line, column)
        {
        }
        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class CharTypeNode : PredefinedTypeNode
    {
        public CharTypeNode(int line, int column) : base(line, column)
        {
        }
        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class CharVaryingTypeNode : PredefinedTypeNode
    {
        public int Length { get; set; }
        public CharVaryingTypeNode(int line, int column) : base(line, column)
        {
        }
        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class NCharTypeNode : PredefinedTypeNode
    {
        public NCharTypeNode(int line, int column) : base(line, column)
        {
        }
        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }


    public class NCharVaryingTypeNode : PredefinedTypeNode
    {
        public int Length { get; set; }
        public NCharVaryingTypeNode(int line, int column) : base(line, column)
        {
        }
        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class NumericTypeNode : PredefinedTypeNode
    {
        public int Precision { get; set; }
        public int Scale { get; set; }
        public NumericTypeNode(int line, int column) : base(line, column)
        {
        }
        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class RealTypeNode : PredefinedTypeNode
    {
        public RealTypeNode(int line, int column) : base(line, column)
        {
        }
        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class SmallintTypeNode : PredefinedTypeNode
    {
        public SmallintTypeNode(int line, int column) : base(line, column)
        {
        }
        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class TimeTypeNode : PredefinedTypeNode
    {
        public int Length { get; set; }
        public TimeTypeNode(int line, int column) : base(line, column)
        {
        }
        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class VarcharTypeNode : PredefinedTypeNode
    {
        public int Length { get; set; }

        public VarcharTypeNode(int line, int column) : base(line, column)
        {
        }
        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class OtherTypeNode : PredefinedTypeNode
    {
        public SchemaQualifiednameNonTypeNode SchemaQualifiednameNonType { get; set; }

        public List<ExpressionNode> Expressions { get; set; }

        public OtherTypeNode(int line, int column) : base(line, column)
        {
        }
        public override TResult Accept<TResult>(ASTVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
