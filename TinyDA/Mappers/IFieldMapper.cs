using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyDA.Mappers
{
    public interface IFieldMapper
    {
        string MapField(string fieldName);
    }
}
