using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
            this.defaultFieldMapper = defaultFieldMapper ?? new SimpleFieldMapper();
        }

        public DataAccessor(IDbConnection connection): this(connection, null){}

        private static string ProcessSql(string sql)
        {
            var counter = 0;
            var stringBuilder = new StringBuilder();
            foreach (var c in sql)
            {
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

        private static string ProcessSpSql(string name, int paramCount)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("exec {0} ", name);
            for (var i = 0; i < paramCount; i++)
            {
                stringBuilder.Append("@p" + i);
                if (i < paramCount - 1)
                {
                    stringBuilder.Append(", ");
                }
            }
            return stringBuilder.ToString();
        }

        public static void PrepareCommandParameters(IDbCommand command, params object[] parameters)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                var p = command.CreateParameter();
                p.ParameterName = "@p" + i;
                p.Value = parameters[i] ?? DBNull.Value;
                command.Parameters.Add(p);
            }
        }

        private void PrepareCommand(IDbCommand command, string sql, params object[] parameters)
        {            
            command.CommandText =  ProcessSql(sql);
            PrepareCommandParameters(command, parameters);
        }

        private void PrepareSpCommand(IDbCommand command, string name, params object[] parameters)
        {
            command.CommandText = ProcessSpSql(name, parameters.Length);
            PrepareCommandParameters(command, parameters);
        }

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
            var t = default(T);
            using (var command = connection.CreateCommand())
            {
                PrepareCommand(command, sql, parameters);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        t = DataUtils.GetObject<T>(reader, fieldMapper);
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
        /// Returns a single dictionary object as a result of running SQL statement, default mapper is used to map the fields to keys.
        /// </summary>
        /// <param name="sql">The SQL query</param>
        /// <param name="parameters">The paramaters passed to the SQL query</param>
        /// <returns>dictionary object or null</returns>
        public IDictionary<string, object> GetObject(string sql, params object[] parameters)
        {
            return GetObject(sql, defaultFieldMapper, parameters);
        }

        /// <summary>
        /// eturns a single dictionary object as a result of running SQL statement
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="fieldMapper"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IDictionary<string, object> GetObject(string sql, IFieldMapper fieldMapper, params object[] parameters)
        {
            IDictionary<string, object> result = null;
            using (var command = connection.CreateCommand())
            {
                PrepareCommand(command, sql, parameters);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = DataUtils.GetObject(reader, fieldMapper);
                    }
                }
            }
            return result;
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
            List<T> items;
            using (var command = connection.CreateCommand())
            {
                PrepareCommand(command, sql, parameters);
                using (var reader = command.ExecuteReader())
                {
                    items = DataUtils.GetList<T>(reader, fieldMapper);
                }
            }
            return items;
        }


        /// <summary>
        /// Returns a generic list of objects as a result of running SQL statement, default mapper is used to map the fields to properties.
        /// </summary>
        /// <typeparam name="T">The generic type of the list</typeparam>
        /// <param name="sql">The SQL query</param>
        /// <param name="parameters">The paramaters passed to the SQL query</param>
        /// <returns>Generic list of objects of type T</returns>
        public List<T> GetList<T>(string sql, params object[] parameters)
        {
            return GetList<T>(sql, defaultFieldMapper, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="fieldMapper"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<IDictionary<string, object>> GetList(string sql, IFieldMapper fieldMapper, params object[] parameters)
        {
            List<IDictionary<string, object>> items;
            using (var command = connection.CreateCommand())
            {
                PrepareCommand(command, sql, parameters);
                using (var reader = command.ExecuteReader())
                {
                    items = DataUtils.GetList(reader, fieldMapper);
                }
            }
            return items;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<IDictionary<string, object>> GetList(string sql, params object[] parameters)
        {
            return GetList(sql, defaultFieldMapper, parameters);
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
            var t = default(T);
            using (var command = connection.CreateCommand())
            {
                PrepareCommand(command, sql, parameters);
                using (var reader = command.ExecuteReader())
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
            List<T> items;
            using (var command = connection.CreateCommand())
            {
                PrepareCommand(command, sql, parameters);
                using (var reader = command.ExecuteReader())
                {
                    items = DataUtils.GetSingleFieldList<T>(reader, fieldIndex);
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
            var t = default(T);
            using (var command = connection.CreateCommand())
            {
                PrepareSpCommand(command, name, parameters);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        t = DataUtils.GetObject<T>(reader, fieldMapper);
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
            List<T> items;
            using (var command = connection.CreateCommand())
            {
                PrepareSpCommand(command, name, parameters);
                using (var reader = command.ExecuteReader())
                {
                    items = DataUtils.GetList<T>(reader, fieldMapper);
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
            using (var command = connection.CreateCommand())
            {
                PrepareCommand(command, sql, parameters);
                var t = (T)command.ExecuteScalar();
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
            using (var command = connection.CreateCommand())
            {
                PrepareCommand(command, sql, parameters);
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Use ExecuteNonQuery instead to have the naming as ADO.net
        /// </summary>
        [Obsolete]
        public int ExecuteUpdate(string sql, params object[] parameters)
        {
            return ExecuteNonQuery(sql, parameters);
        }
    }
}
