using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TinyDA.Mappers
{
    /// <summary>
    /// Maps field names to property names depedning on attributes
    /// </summary>
    public class AttributeFieldMapper: IFieldMapper
    {
        private Type targetType;

        public AttributeFieldMapper(Type targetType)
        {
            this.targetType = targetType;
        }

        private PropertyInfo findProperty(string fieldName)
        {
            PropertyInfo[] properties = targetType.GetProperties();
            foreach(PropertyInfo p in properties)
            {
                Column col = p.GetCustomAttribute<Column>();
                if (col != null && col.GetName() == fieldName)
                {
                    return p;
                }
            }
            return null;
        }

        public string MapField(string fieldName)
        {
            PropertyInfo p = findProperty(fieldName);
            return p != null ? p.Name : null;
        }

    }
}
