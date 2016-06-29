using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyDA.Mappers
{
    public class CustomFieldMapper: IFieldMapper
    {
        private Func<string, string> lambda;

        public CustomFieldMapper(Func<string, string> lambda)
        {
            this.lambda = lambda;
        }

        public string MapField(string fieldName)
        {
            return lambda(fieldName);
        }
    }
}
