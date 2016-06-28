using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyDA.Data
{
    public interface INamingConverter
    {
        string FromDB(string value);
        string ToDB(string value);
    }
}
