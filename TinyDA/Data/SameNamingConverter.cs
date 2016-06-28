using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyDA.Data
{
    public class SameNamingConverter: INamingConverter
    {
        public string FromDB(string value)
        {
            return value;
        }

        public string ToDB(string value)
        {
            return value;
        }
    }
}
