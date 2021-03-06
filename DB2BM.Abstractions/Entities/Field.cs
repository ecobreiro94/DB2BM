﻿using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Interfaces;

namespace DB2BM.Abstractions.Entities
{
    public enum AttributeField{ Property, PrimaryKey, ForeingKey , RelationOneMany, RelationManyOne}

    public class BaseField
    {
        public string Name { get; set; }

        public string OriginType { get; set; }

        public string DestinyType { get; set; }

        public int OrdinalPosition { get; set; }

        public string Default { get; set; }

        public bool IsNullable { get; set; }

        public uint? CharacterMaximumLength { get; set; }

        public bool OwnsMany { get; set; }

        public bool IsUDT { get; set; }

        public bool IsUDTEnum { get; set; }
    }

    public class UdtField : BaseField
    {
        public UserDefined.UserDefinedType Udt { get; set; }
    }

    public class TableField : BaseField
    {
        public Table Table { get; set; }

        public string GenName { get; set; }

        public AttributeField Attribute { get; set; }

        public string FieldRelation { get; set; }

        public override string ToString()
        {
            return OriginType + " " + Name;
        }

        public bool IsProperty { get { return Attribute == AttributeField.Property; } }
        public bool IsPrimaryKey { get { return Attribute == AttributeField.PrimaryKey; } }
        public bool IsForeingKey { get { return Attribute == AttributeField.ForeingKey; } }
        public bool IsRelationOneMany { get { return Attribute == AttributeField.RelationOneMany; } }
        public bool IsRelationManyOne { get { return Attribute == AttributeField.RelationManyOne; } }

        public bool IsVirtual { get { return Attribute == AttributeField.RelationOneMany; } }
    }
}
