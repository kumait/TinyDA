using System.Text;

namespace TinyDA.Mappers
{
    /// <summary>
    /// Converts underscore style field names to pascal case propery names
    /// STUDENT_NAME is mapped to StudentName for example.
    /// </summary>
    public class UnderscoreMapper: IMapper
    {
        public string GetPropertyName(string columnName)
        {
            string s = columnName.ToLower();
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

        public string GetColumnName(string propertyName)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < propertyName.Length; i++)
            {
                char ch = propertyName[i];
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
