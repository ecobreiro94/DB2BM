using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions
{
    public enum GeneralType
    {
        Numeric,
        String,
        Bool,
        Object,
        DateTime,
        Array
    }
    //Array []
    //hash <>
    //sets {}
    public class TypeInfo
    {
        public static Dictionary<string, TypeInfo> TypesInfo = new Dictionary<string, TypeInfo>()
        {
            { "dynamic" , new TypeInfo(){ TypeName = "object", GeneralType = GeneralType.Object, Index = 0, SpecificType = "object"} },
            { "bool", new TypeInfo(){ TypeName = "bool", GeneralType = GeneralType.Bool, Index = 1, SpecificType = "bool"} },
            { "short", new TypeInfo(){ TypeName = "short", GeneralType = GeneralType.Numeric, Index = 2, SpecificType = "short"} },
            { "int", new TypeInfo(){ TypeName = "int", GeneralType = GeneralType.Numeric, Index = 3, SpecificType = "int"} },
            { "long", new TypeInfo(){ TypeName = "long", GeneralType = GeneralType.Numeric, Index = 4, SpecificType = "long"} },
            { "decimal", new TypeInfo(){ TypeName = "decimal", GeneralType = GeneralType.Numeric, Index = 5, SpecificType = "decimal"} },
            { "float", new TypeInfo(){ TypeName = "float", GeneralType = GeneralType.Numeric, Index = 6, SpecificType = "float"} },
            { "double", new TypeInfo(){ TypeName = "double", GeneralType = GeneralType.Numeric, Index = 7, SpecificType = "double"} },
            { "string", new TypeInfo() { TypeName = "string", GeneralType = GeneralType.String, Index = 8, SpecificType = "string"} },
            { "DateTime", new TypeInfo(){ TypeName = "DateTime", GeneralType = GeneralType.DateTime, Index = 9, SpecificType = "DateTime"} },
            { "TimeSpan", new TypeInfo(){ TypeName = "DateTime", GeneralType = GeneralType.DateTime, Index = 10, SpecificType = "TimeSpan"} },
            { "bool[]", new TypeInfo(){ TypeName = "bool[]", GeneralType = GeneralType.Array, Index = 12, SpecificType = "bool"} },
            { "object[]", new TypeInfo(){ TypeName = "object[]", GeneralType = GeneralType.Array, Index = 13, SpecificType = "object"} }
        };
        public string TypeName { get; set; }

        public GeneralType GeneralType { get; set; }

        public int Index { get; set; }

        public string SpecificType { get; set; }
    }
}
