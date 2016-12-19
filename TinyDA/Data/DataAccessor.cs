﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TinyDA.Mappers;

namespace TinyDA.Data
{
    public class DataAccessor: IDataAccessor
    {
        private readonly IDbConnection connection;
        private readonly IDbTransaction transaction;

        private readonly IMapper defaultMapper;

        public DataAccessor(IDbConnection connection, IDbTransaction transaction, IMapper defaultMapper)
        {
            this.connection = connection;
            this.transaction = transaction;
            this.defaultMapper = defaultMapper ?? new SimpleMapper();
        }

        public DataAccessor(IDbConnection connection, IMapper defaultMapper)
            : this(connection, null, defaultMapper) { }

        public DataAccessor(IDbConnection connection, IDbTransaction transaction)
            : this(connection, transaction, null) { }

        public DataAccessor(IDbConnection connection)
            : this(connection, null, null) { }

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
            var stringBuilder = new StringBuilder();
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

        private IDbCommand CreateCommand()
        {
            var command = connection.CreateCommand();
            if (transaction != null)
            {
                command.Transaction = transaction;
            }
            return command;
        }

        /// <summary>
        /// Returns a single object as a result of running SQL statement
        /// </summary>
        /// <typeparam name="T">The type of returned the object</typeparam>
        /// <param name="sql">The SQL query</param>
        /// <param name="mapper">The field mapper used to map field names to property names</param>
        /// <param name="parameters">The paramaters passed to the SQL query</param>
        /// <returns>The object of type T</returns>
        public T GetObject<T>(string sql, IMapper mapper, params object[] parameters)
        {
            var t = default(T);
            using (var command = CreateCommand())
            {
                PrepareCommand(command, sql, parameters);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        t = DataUtils.GetObject<T>(reader, mapper);
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
            return GetObject<T>(sql, defaultMapper, parameters);
        }

        /// <summary>
        /// Returns a single dictionary object as a result of running SQL statement, default mapper is used to map the fields to keys.
        /// </summary>
        /// <param name="sql">The SQL query</param>
        /// <param name="parameters">The paramaters passed to the SQL query</param>
        /// <returns>dictionary object or null</returns>
        public IDictionary<string, object> GetObject(string sql, params object[] parameters)
        {
            return GetObject(sql, defaultMapper, parameters);
        }

        /// <summary>
        /// eturns a single dictionary object as a result of running SQL statement
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="mapper"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IDictionary<string, object> GetObject(string sql, IMapper mapper, params object[] parameters)
        {
            IDictionary<string, object> result = null;
            using (var command = CreateCommand())
            {
                PrepareCommand(command, sql, parameters);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = DataUtils.GetObject(reader, mapper);
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
        /// <param name="mapper">The field mapper used to map field names to property names</param>
        /// <param name="parameters">The paramaters passed to the SQL query</param>
        /// <returns>Generic list of objects of type T</returns>
        public List<T> GetList<T>(string sql, IMapper mapper, params object[] parameters)
        {
            List<T> items;
            using (var command = CreateCommand())
            {
                PrepareCommand(command, sql, parameters);
                using (var reader = command.ExecuteReader())
                {
                    items = DataUtils.GetList<T>(reader, mapper);
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
            return GetList<T>(sql, defaultMapper, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="mapper"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<IDictionary<string, object>> GetList(string sql, IMapper mapper, params object[] parameters)
        {
            List<IDictionary<string, object>> items;
            using (var command = CreateCommand())
            {
                PrepareCommand(command, sql, parameters);
                using (var reader = command.ExecuteReader())
                {
                    items = DataUtils.GetList(reader, mapper);
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
            return GetList(sql, defaultMapper, parameters);
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
            using (var command = CreateCommand())
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
            using (var command = CreateCommand())
            {
                PrepareCommand(command, sql, parameters);
                using (var reader = command.ExecuteReader())
                {
                    items = DataUtils.GetSingleColumnList<T>(reader, fieldIndex);
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
        /// <param name="mapper">The field mapper used to map field names to property names</param>
        /// <param name="parameters">The paramaters passed to the stored procedure</param>
        /// <returns>The object of type T</returns>
        public T GetObjectSP<T>(string name, IMapper mapper, params object[] parameters)
        {
            var t = default(T);
            using (var command = CreateCommand())
            {
                PrepareSpCommand(command, name, parameters);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        t = DataUtils.GetObject<T>(reader, mapper);
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
            return GetObjectSP<T>(name, defaultMapper, parameters);
        }


        /// <summary>
        /// Returns a generic list of objects as a result of running a stored procedure. If more than result set is returned
        /// from the stored procedure, the first one is used to map the result and the rest are ignored.
        /// </summary>
        /// <typeparam name="T">The generic type of the list</typeparam>
        /// <param name="name">The stored procedure name</param>
        /// <param name="mapper">The field mapper used to map field names to property names</param>
        /// <param name="parameters">The paramaters passed to the stored procedure</param>
        /// <returns>Generic list of objects of type T</returns>
        public List<T> GetListSP<T>(string name, IMapper mapper, params object[] parameters)
        {
            List<T> items;
            using (var command = CreateCommand())
            {
                PrepareSpCommand(command, name, parameters);
                using (var reader = command.ExecuteReader())
                {
                    items = DataUtils.GetList<T>(reader, mapper);
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
            return GetListSP<T>(name, defaultMapper, parameters);
        }

        public List<IDictionary<string, object>> GetListSP(string name, IMapper mapper, params object[] parameters)
        {
            List<IDictionary<string, object>> items;
            using (var command = CreateCommand())
            {
                PrepareSpCommand(command, name, parameters);
                using (var reader = command.ExecuteReader())
                {
                    items = DataUtils.GetList(reader, mapper);
                }
            }
            return items;
        }

        public List<IDictionary<string, object>> GetListSP(string name, params object[] parameters)
        {
            return GetListSP(name, defaultMapper, parameters);
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
            using (var command = CreateCommand())
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
            using (var command = CreateCommand())
            {
                PrepareCommand(command, sql, parameters);
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Use ExecuteNonQuery instead
        /// </summary>
        [Obsolete]
        public int ExecuteUpdate(string sql, params object[] parameters)
        {
            return ExecuteNonQuery(sql, parameters);
        }

        /// <summary>
        /// Inserts an entity in the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="table"></param>
        /// <param name="identityColumn"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public T Insert<T>(object entity, string table, string identityColumn, IMapper mapper)
        {
            var poperties = entity.GetType().GetProperties();
            var columns = new List<string>();
            var questionMarks = new List<string>();
            var values = new List<object>();

            foreach (var pi in poperties)
            {
                var columnName = mapper.GetColumnName(pi.Name);
                if (columnName != identityColumn)
                {
                    columns.Add(columnName);
                    questionMarks.Add("?");
                    values.Add(pi.GetValue(entity));
                }
            }

            var sb = new StringBuilder();
            sb.AppendFormat("insert into {0} ", table);
            sb.AppendFormat("({0})", StringUtils.GenerateCommaSeparatedString(columns, null, null));

            if (identityColumn != null)
            {
                sb.AppendFormat(" output inserted.{0}", identityColumn);
            }

            sb.Append(" values ");
            sb.AppendFormat("({0})", StringUtils.GenerateCommaSeparatedString(questionMarks, null, null));

            var sql = sb.ToString();

            return ExecuteScalar<T>(sql, values.ToArray());
        }

        public T Insert<T>(object entity, string table, string identityColumn)
        {
            return Insert<T>(entity, table, identityColumn, defaultMapper);
        }

        public int Update(object entity, string table, string selectionColumn, object selectionValue, IMapper mapper)
        {
            var poperties = entity.GetType().GetProperties();
            var columns = new List<string>();
            var values = new List<object>();
            
            foreach (var pi in poperties)
            {
                var columnName = mapper.GetColumnName(pi.Name);
                columns.Add(columnName);
                values.Add(pi.GetValue(entity));
            }
            values.Add(selectionValue);

            var sb = new StringBuilder();
            sb.AppendFormat("update {0} set ", table);
            for (int i = 0; i < columns.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                sb.AppendFormat("{0} = ?", columns[i]);
            }

            sb.AppendFormat(" where {0} = ?", selectionColumn);
            var sql = sb.ToString();

            return ExecuteNonQuery(sql, values.ToArray());
        }

        public void Insert(object entity, string table, IMapper mapper)
        {
            var poperties = entity.GetType().GetProperties();
            var columns = new List<string>();
            var questionMarks = new List<string>();
            var values = new List<object>();

            foreach (var pi in poperties)
            {
                var columnName = mapper.GetColumnName(pi.Name);
                columns.Add(columnName);
                questionMarks.Add("?");
                values.Add(pi.GetValue(entity));
            }

            var sb = new StringBuilder();
            sb.AppendFormat("insert into {0} ", table);
            sb.AppendFormat("({0})", StringUtils.GenerateCommaSeparatedString(columns, null, null));
            
            sb.Append(" values ");
            sb.AppendFormat("({0})", StringUtils.GenerateCommaSeparatedString(questionMarks, null, null));

            var sql = sb.ToString();
            ExecuteNonQuery(sql, values.ToArray());
        }

        public void Insert(object entity, string table)
        {
            Insert(entity, table, defaultMapper);
        }
    }
}
