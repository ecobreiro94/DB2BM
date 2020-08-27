using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Antlr4.StringTemplate;
using DB2BM.Utils;

namespace DB2BM.Extensions.EFCore.Utils
{
    public class CSharpRenderer : StringRenderer
    {
        public override string ToString(object o, string formatString, CultureInfo culture)
        {
            switch (formatString)
            {
                case "pascal":
                    if (o is string o1_str)
                        return o1_str.ToPascal();
                    goto default;
                case "camel":
                    if (o is string o2_str)
                        return o2_str.ToPascal();
                    goto default;
                default:
                    return base.ToString(o, formatString, culture);
            }
        }
    }
}
