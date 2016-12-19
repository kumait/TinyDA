using System;
using System.Collections.Generic;
using System.Data;
using TinyDA.Mappers;

namespace TinyDA.Data
{
    public static class DataUtils
    {
        public static IDictionary<string, int> GetColumnToPropertyMap(IDataReader reader, Type type, IMapper fieldMapper)
        {
            IDictionary<string, int> fieldMap = new Dictionary<string, int>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var propertyName = fieldMapper.GetPropertyName(reader.GetName(i));
                if (propertyName != null)
                {
                    fieldMap.Add(propertyName, i);
                }
            }
            return fieldMap;
        }

        public static T GetObject<T>(IDataReader reader, IDictionary<string, int> fieldMap)
        {
            var t = Activator.CreateInstance<T>();
            foreach (var pi in t.GetType().GetProperties())
            {
                if (!fieldMap.ContainsKey(pi.Name)) continue;
                var fieldIndex = fieldMap[pi.Name];
                if (reader[fieldIndex].GetType() != typeof(DBNull))
                {
                    pi.SetValue(t, reader[fieldIndex], null);
                }
            }
            return t;
        }

        public static List<T> GetSingleColumnList<T>(IDataReader reader, int fieldIndex)
        {
            var items = new List<T>();
            while (reader.Read())
            {
                var t = (T)reader[fieldIndex];
                items.Add(t);
            }
            return items;
        }

        public static T GetObject<T>(IDataReader reader, IMapper fieldMapper)
        {
            return GetObject<T>(reader, GetColumnToPropertyMap(reader, typeof(T), fieldMapper));
        }

        public static List<T> GetList<T>(IDataReader reader, IMapper fieldMapper)
        {
            var fieldMap = GetColumnToPropertyMap(reader, typeof(T), fieldMapper);
            var items = new List<T>();
            while (reader.Read())
            {
                var item = GetObject<T>(reader, fieldMap);
                items.Add(item);
            }
            return items;
        }

        public static IDictionary<string, object> GetObject(IDataReader reader, IMapper fieldMapper)
        {
            var result = new Dictionary<string, object>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var key = fieldMapper.GetPropertyName(reader.GetName(i));
                result[key] = reader.GetValue(i);
            }
            return result;
        }

        public static List<IDictionary<string, object>> GetList(IDataReader reader, IMapper fieldMapper)
        {
            var result = new List<IDictionary<string, object>>();
            while (reader.Read())
            {
                result.Add(GetObject(reader, fieldMapper));
            }
            return result;
        }
    }
}
