using System;
using System.Collections.Generic;
using System.Text;
using DB2BM.Abstractions.Interfaces;

namespace DB2BM.Abstractions.Entities
{
    public enum ParameterMode { In, Out, InOut }

    public class Parameter
    {
        //public Function Function { get; set; }

        public string Name { get; set; }

        public string OriginType { get; set; }

        public string DestinyType { get; set; }

        public int OrdinalPosition { get; set; }

        public string IsResult { get; set; }

        public ParameterMode ParameterMode { get; set; }

        public bool OutMode => ParameterMode != ParameterMode.In;

        public override string ToString()
        {
            return (Name == null )? OriginType : OriginType + " " + Name;
        }
    }
}
