using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using TinyDA.Mappers;

namespace TinyDA.Data
{
    public class DataAccessor: IDataAccessor
    {
        private readonly IDbConnection connection;
        private readonly IFieldMapper defaultFieldMapper;

        public DataAccessor(IDbConnection connection, IFieldMapper defaultFieldMapper)
        {
            this.connection = connection;
            this.defaultFieldMapper = defaultFieldMapper != null ? defaultFieldMapper : new SimpleFieldMapper();
        }

        public DataAccessor(IDbConnection connection): this(connection, null){}

        private IDictionary<string, int> GetFieldToPropertyMap(IDataReader reader, Type type, IFieldMapper fieldMapper)
        {
            IDictionary<string, int> fieldMap = new Dictionary<string, int>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string propertyName = fieldMapper.MapField(reader.GetName(i));
                if (propertyName != null)
                {
                    fieldMap.Add(propertyName, i);
                }
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

        private T GetObject<T>(IDataReader reader, IFieldMapper fieldMapper)
        {
            return GetObject<T>(reader, GetFieldToPropertyMap(reader, typeof(T), fieldMapper));
        }

        private List<T> GetList<T>(IDataReader reader, IFieldMapper fieldMapper)
        {
            IDictionary<string, int> fieldMap = GetFieldToPropertyMap(reader, typeof(T), fieldMapper);
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


        /// <summary>
        /// Returns a single object as a result of running SQL statement
        /// </summary>
        /// <typeparam name="T">The type of returned the object</typeparam>
        /// <param name="sql">The SQL query</param>
        /// <param name="fieldMapper">The field mapper used to map field names to property names</param>
        /// <param name="parameters">The paramaters passed to the SQL query</param>
        /// <returns>The object of type T</returns>
        public T GetObject<T>(string sql, IFieldMapper fieldMapper, params object[] parameters)
        {
            T t = default(T);
            using (IDbCommand command = connection.CreateCommand())
            {
                prepareCommand(command, sql, parameters);
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        t = GetObject<T>(reader, fieldMapper);
                    }
                }
            }
            return t;
        }


        /// <summary>
        /// Returns a single object as a result of running SQL statement, default mapper is used to map the fields to properties.
        /// </summary>
        /// <typeparam name="T">The type of the returned object</typeparam>
        /// <param name="sql">The SQL query</param>        
        /// <param name="parameters">The paramaters passed to the SQL query</param>
        /// <returns>The object of type T</returns>
        public T GetObject<T>(string sql, params object[] parameters)
        {
            return GetObject<T>(sql, defaultFieldMapper, parameters);
        }


        /// <summary>
        /// Returns a generic list of objects as a result of running SQL statement.
        /// </summary>
        /// <typeparam name="T">The generic type of the list</typeparam>
        /// <param name="sql">The SQL query</param>
        /// <param name="fieldMapper">The field mapper used to map field names to property names</param>
        /// <param name="parameters">The paramaters passed to the SQL query</param>
        /// <returns>Generic list of objects of type T</returns>
        public List<T> GetList<T>(string sql, IFieldMapper fieldMapper, params object[] parameters)
        {
            List<T> items = new List<T>();
            using (IDbCommand command = connection.CreateCommand())
            {
                prepareCommand(command, sql, parameters);
                using (IDataReader reader = command.ExecuteReader())
                {
                    items = GetList<T>(reader, fieldMapper);
                }
            }
            return items;
        }


        /// <summary>
        /// Returns a generic list of objects as a result of running SQL statement, default mapper is used to map the fields to properties.
        /// </summary>
        /// <typeparam name="T">The generic type of the list</typeparam>
        /// <param name="sql">The SQL query</param>
        /// <param name="fieldMapper">The field mapper used to map field names to property names</param>
        /// <param name="parameters">The paramaters passed to the SQL query</param>
        /// <returns>Generic list of objects of type T</returns>
        public List<T> GetList<T>(string sql, params object[] parameters)
        {
            return GetList<T>(sql, defaultFieldMapper, parameters);
        }


        /// <summary>
        /// Returns a single value as a result of running SQL statement
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="sql">The SQL query</param>
        /// <param name="fieldIndex">The field index that should be used to get the single value</param>
        /// <param name="parameters">The paramaters passed to the SQL query</param>
        /// <returns>The value of type T</returns>
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


        /// <summary>
        /// Returns a generic list of values as a result of running SQL statement
        /// </summary>
        /// <typeparam name="T">The type of the generic list</typeparam>
        /// <param name="sql">The SQL query</param>
        /// <param name="fieldIndex">The field index that should be used to get the single value</param>
        /// <param name="parameters">The paramaters passed to the SQL query</param>
        /// <returns>The generic list type T</returns>
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

        /// <summary>
        /// Returns a single object as a result of running a stored procedure that returns a result set. If more than result set is returned
        /// from the stored procedure, the first one is used to map the result and the rest are ignored.
        /// </summary>
        /// <typeparam name="T">The type of the returned object</typeparam>
        /// <param name="name">The stored procedure name</param>        
        /// <param name="fieldMapper">The field mapper used to map field names to property names</param>
        /// <param name="parameters">The paramaters passed to the stored procedure</param>
        /// <returns>The object of type T</returns>
        public T GetObjectSP<T>(string name, IFieldMapper fieldMapper, params object[] parameters)
        {
            T t = default(T);
            using (IDbCommand command = connection.CreateCommand())
            {
                prepareSPCommand(command, name, parameters);
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        t = GetObject<T>(reader, fieldMapper);
                    }
                }

                return t;
            }   
        }

        /// <summary>
        /// Returns a single object as a result of running a stored procedure, default mapper is used to map the fields to properties. 
        /// If more than result set is returned from the stored procedure, the first one is used to map the result and the rest are ignored.
        /// </summary>
        /// <typeparam name="T">The type of the returned object</typeparam>
        /// <param name="name">The stored procedure name</param>        
        /// <param name="parameters">The paramaters passed to the stored procedure</param>
        /// <returns>The object of type T</returns>
        public T GetObjectSP<T>(string name, params object[] parameters)
        {
            return GetObjectSP<T>(name, defaultFieldMapper, parameters);
        }


        /// <summary>
        /// Returns a generic list of objects as a result of running a stored procedure. If more than result set is returned
        /// from the stored procedure, the first one is used to map the result and the rest are ignored.
        /// </summary>
        /// <typeparam name="T">The generic type of the list</typeparam>
        /// <param name="name">The stored procedure name</param>
        /// <param name="fieldMapper">The field mapper used to map field names to property names</param>
        /// <param name="parameters">The paramaters passed to the stored procedure</param>
        /// <returns>Generic list of objects of type T</returns>
        public List<T> GetListSP<T>(string name, IFieldMapper fieldMapper, params object[] parameters)
        {
            List<T> items = new List<T>();
            using (IDbCommand command = connection.CreateCommand())
            {
                prepareSPCommand(command, name, parameters);
                using (IDataReader reader = command.ExecuteReader())
                {
                    items = GetList<T>(reader, fieldMapper);
                }
            }
            return items;
        }

        /// <summary>
        /// Returns a generic list of objects as a result of running SQL statement, default mapper is used to map the fields to properties.
        /// If more than result set is returned from the stored procedure, the first one is used to map the result and the rest are ignored.
        /// </summary>
        /// <typeparam name="T">The generic type of the list</typeparam>
        /// <param name="name">The stored procedure name</param>
        /// <param name="parameters">The paramaters passed to the stored procedure</param>
        /// <returns>Generic list of objects of type T</returns>
        public List<T> GetListSP<T>(string name, params object[] parameters)
        {
            return GetListSP<T>(name, defaultFieldMapper, parameters);
        }

        /// <summary>
        /// Executes a scalar SQL statement and returns the result of it.
        /// </summary>
        /// <typeparam name="T">The type of the returned result</typeparam>
        /// <param name="sql">The SQL statement</param>
        /// <param name="parameters">The paramaters passed to the SQL query</param>
        /// <returns>The scalar value of type T</returns>
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

        /// <summary>
        /// Executes an NonQuery SQL statement
        /// </summary>
        /// <param name="sql">The SQL statement</param>
        /// <param name="parameters">The paramaters passed to the SQL query</param>
        /// <returns>The number of affected rows</returns>
        public int ExecuteNonQuery(string sql, params object[] parameters)
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                prepareCommand(command, sql, parameters);
                return command.ExecuteNonQuery();
            }
        }
    }
}
