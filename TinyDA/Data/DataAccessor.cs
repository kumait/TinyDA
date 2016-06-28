using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TinyDA.Data
{
    public class DataAccessor: IDataAccessor
    {
        private readonly IDbConnection connection;
        private readonly INamingConverter namingConverter;

        public DataAccessor(IDbConnection connection, INamingConverter namingConverter)
        {
            this.connection = connection;
            this.namingConverter = namingConverter;
        }

        public DataAccessor(IDbConnection connection) : this(connection, new SameNamingConverter()) { }

        private IDictionary<string, int> GetFieldToPropertyMap(IDataReader reader, Type type)
        {
            IDictionary<string, int> fieldMap = new Dictionary<string, int>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                fieldMap.Add(namingConverter.FromDB(reader.GetName(i)), i);
            }
            return fieldMap;
        }

        private string processSql(string sql)
        {
            int counter = 0;
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < sql.Length; i++)
            {
                char c = sql[i];
                if (c == '?')
                {
                    stringBuilder.Append("@p" + counter++);
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString();
        }

        private string processSPSQL(string name, int paramCount)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("exec {0} ", name);
            for (int i = 0; i < paramCount; i++)
            {
                stringBuilder.Append("@p" + i);
                if (i < paramCount - 1)
                {
                    stringBuilder.Append(", ");
                }
            }
            return stringBuilder.ToString();
        }

        private void prepareCommandParameters(IDbCommand command, params object[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                IDbDataParameter p = command.CreateParameter();
                p.ParameterName = "@p" + i;
                p.Value = parameters[i] != null ? parameters[i] : DBNull.Value;
                command.Parameters.Add(p);
            }
        }

        private void prepareCommand(IDbCommand command, string sql, params object[] parameters)
        {            
            command.CommandText = processSql(sql);
            prepareCommandParameters(command, parameters);
        }

        private void prepareSPCommand(IDbCommand command, string name, params object[] parameters)
        {            
            command.CommandText = processSPSQL(name, parameters.Length);
            prepareCommandParameters(command, parameters);
        }

        private T GetObject<T>(IDataReader reader, IDictionary<string, int> fieldMap)
        {
            T t = Activator.CreateInstance<T>();            
            foreach (PropertyInfo pi in t.GetType().GetProperties())
            {
                if (fieldMap.ContainsKey(pi.Name))
                {
                    int fieldIndex = fieldMap[pi.Name];
                    if (reader[fieldIndex].GetType() != typeof(DBNull))
                    {
                        pi.SetValue(t, reader[fieldIndex], null);
                    }                    
                }
            }
            return t;
        }

        private T GetObject<T>(IDataReader reader)
        {
            return GetObject<T>(reader, GetFieldToPropertyMap(reader, typeof(T)));
        }

        private List<T> GetList<T>(IDataReader reader)
        {
            IDictionary<string, int> fieldMap = GetFieldToPropertyMap(reader, typeof(T));
            List<T> items = new List<T>();
            while (reader.Read())
            {
                T item = GetObject<T>(reader, fieldMap);
                items.Add(item);
            }
            return items;
        }

        private List<T> GetSingleFieldList<T>(IDataReader reader, int fieldIndex)
        {
            List<T> items = new List<T>();
            while (reader.Read())
            {
                T t = (T)reader[fieldIndex];
                items.Add(t);
            }
            return items;
        }

        // ==================== Public Methods ===============================

        public T GetObject<T>(string sql, params object[] parameters)
        {
            T t = default(T);
            using (IDbCommand command = connection.CreateCommand())
            {
                prepareCommand(command, sql, parameters);
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        t = GetObject<T>(reader);
                    }
                }
            }
            return t;
        }

        public List<T> GetList<T>(string sql, params object[] parameters)
        {
            List<T> items = new List<T>();
            using (IDbCommand command = connection.CreateCommand())
            {
                prepareCommand(command, sql, parameters);
                using (IDataReader reader = command.ExecuteReader())
                {
                    items = GetList<T>(reader);
                }
            }
            return items;
        }

        public T GetValue<T>(string sql, int fieldIndex, params object[] parameters)
        {
            T t = default(T);
            using (IDbCommand command = connection.CreateCommand())
            {
                prepareCommand(command, sql, parameters);
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        t = (T)reader[fieldIndex];
                    }
                }
            }
            return t;
        }

        public List<T> GetValues<T>(string sql, int fieldIndex, params object[] parameters)
        {
            List<T> items = new List<T>();
            using (IDbCommand command = connection.CreateCommand())
            {
                prepareCommand(command, sql, parameters);
                using (IDataReader reader = command.ExecuteReader())
                {
                    items = GetSingleFieldList<T>(reader, fieldIndex);
                }
            }
            return items;
        }

        public T ExecuteScalar<T>(string sql, params object[] parameters)
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                T t = default(T);
                prepareCommand(command, sql, parameters);
                t = (T)command.ExecuteScalar();
                return t;
            }
        }

        public int ExecuteUpdate(string sql, params object[] parameters)
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                prepareCommand(command, sql, parameters);
                return command.ExecuteNonQuery();
            }
        }

        public T GetObjectSP<T>(string name, params object[] parameters)
        {
            T t = default(T);
            using (IDbCommand command = connection.CreateCommand())
            {
                prepareSPCommand(command, name, parameters);
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        t = GetObject<T>(reader);
                    }
                }

                return t;
            }   
        }

        public List<T> GetListSP<T>(string name, params object[] parameters)
        {
            List<T> items = new List<T>();
            using (IDbCommand command = connection.CreateCommand())
            {
                prepareSPCommand(command, name, parameters);
                using (IDataReader reader = command.ExecuteReader())
                {
                    items = GetList<T>(reader);
                }
            }
            return items;
        }
    }
}
