using System;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Abstractions
{
    public class DbOption
    {
        public string User { get; set; }

        public string Host { get; set; }

        public string Password { get; set; }

        public string DataBaseName { get; set; }
    }
}
