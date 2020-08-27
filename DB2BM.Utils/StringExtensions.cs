using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DB2BM.Utils
{
    public static class StringExtensions
    {
        public static string ToPascal(this string _s)
        {
            var s = "";
            var ls = _s.Split();
            foreach (var item in ls)
            {
                s += item;
            }
            var result = "";
            var index = 0;
            var len = s.Length;

            while (index < len)
            {
                char ch;
                while (index < len)
                {
                    ch = s[index++];
                    if (char.IsLetterOrDigit(ch))
                    {
                        result += char.ToUpper(ch);
                        break;
                    }
                }

                while (index < len)
                {
                    ch = s[index++];
                    if (char.IsLetterOrDigit(ch))
                        result += ch;
                    else
                        break;
                }
            }
            return result;
        }
        public static string ToCamel(this string _s)
        {
            var s = "";
            var ls = _s.Split();
            foreach (var item in ls)
            {
                s += item;
            }
            StringBuilder result = new StringBuilder(ToPascal(s));
            if (result.Length > 0)
                result[0] = char.ToLower(result[0]);
            return result.ToString();
        }
    }
}
