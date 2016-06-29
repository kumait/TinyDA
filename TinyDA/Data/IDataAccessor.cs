using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyDA.Mappers;

namespace TinyDA.Data
{
    public interface IDataAccessor
    {
        T GetObject<T>(string sql, IFieldMapper fieldMapper, params object[] parameters);
        T GetObject<T>(string sql, params object[] parameters);

        T GetValue<T>(string sql, int fieldIndex, params object[] parameters);

        List<T> GetList<T>(string sql, IFieldMapper fieldMapper, params object[] parameters);
        List<T> GetList<T>(string sql, params object[] parameters);

        List<T> GetValues<T>(string sql, int fieldIndex, params object[] parameters);

        T GetObjectSP<T>(string name, IFieldMapper fieldMapper, params object[] parameters);
        T GetObjectSP<T>(string name, params object[] parameters);

        List<T> GetListSP<T>(string name, IFieldMapper fieldMapper, params object[] parameters);
        List<T> GetListSP<T>(string name, params object[] parameters);

        T ExecuteScalar<T>(string sql, params object[] parameters);

        int ExecuteNonQuery(string sql, params object[] parameters);
        
    }
}
