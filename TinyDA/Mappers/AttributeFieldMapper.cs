using System;
using System.Linq;
using System.Reflection;

namespace TinyDA.Mappers
{
    /// <summary>
    /// Maps field names to property names depedning on attributes
    /// </summary>
    public class AttributeFieldMapper: IFieldMapper
    {
        private readonly Type targetType;

        public AttributeFieldMapper(Type targetType)
        {
            this.targetType = targetType;
        }

        /*
        private PropertyInfo FindProperty(string fieldName)
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
        */ 

        private PropertyInfo FindProperty(string fieldName)
        {
            var properties = targetType.GetProperties();
            foreach (var p in properties)
            {
                var attrs = p.GetCustomAttributes(typeof(Column), false);
                if (attrs.Length == 0) continue;
                var col = (Column) attrs[0];
                if (col != null && col.GetName() == fieldName)
                {
                    return p;
                }
            }
            return null;
        }

        public string MapField(string fieldName)
        {
            PropertyInfo p = FindProperty(fieldName);
            return p != null ? p.Name : null;
        }

    }
}
