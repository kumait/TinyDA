using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyDA.Data
{
    public class NaturalNamingConverter: INamingConverter
    {
        public string FromDB(string value)
        {
            string s = value.ToLower();
            bool flag = false;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                if (ch == '_')
                {
                    flag = true;
                }
                else
                {
                    bool shouldCapitalize = flag || i == 0;
                    if (shouldCapitalize)
                        sb.Append(ch.ToString().ToUpper());
                    else
                        sb.Append(ch);
                    flag = false;
                }
            }
            return sb.ToString();
        }

        public string ToDB(string value)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                char ch = value[i];
                if (char.IsUpper(ch) && i > 0)
                {
                    sb.Append("_");
                }
                sb.Append(ch.ToString().ToUpper());
            }
            return sb.ToString();
        }
    }
}
