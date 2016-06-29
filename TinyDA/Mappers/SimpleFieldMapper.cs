using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyDA.Mappers
{
    /// <summary>
    /// Uses field names as property names with no change
    /// </summary>
    public class SimpleFieldMapper: IFieldMapper
    {
        public string MapField(string fieldName)
        {
            return fieldName;
        }
    }
}
