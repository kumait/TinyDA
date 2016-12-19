using System;
using System.Collections.Generic;
using TinyDA.Mappers;
// ReSharper disable InconsistentNaming

namespace TinyDA.Data
{
    public interface IDataAccessor
    {
        T GetObject<T>(string sql, IMapper mapper, params object[] parameters);
        T GetObject<T>(string sql, params object[] parameters);
        IDictionary<string, object> GetObject(string sql, params object[] parameters);
        IDictionary<string, object> GetObject(string sql, IMapper mapper, params object[] parameters);
        T GetValue<T>(string sql, int fieldIndex, params object[] parameters);
        T GetObjectSP<T>(string name, IMapper mapper, params object[] parameters);
        T GetObjectSP<T>(string name, params object[] parameters);

        List<T> GetList<T>(string sql, IMapper mapper, params object[] parameters);
        List<T> GetList<T>(string sql, params object[] parameters);
        List<IDictionary<string, object>> GetList(string sql, IMapper mapper, params object[] parameters);
        List<IDictionary<string, object>> GetList(string sql, params object[] parameters);
        List<T> GetValues<T>(string sql, int fieldIndex, params object[] parameters);
        List<T> GetListSP<T>(string name, IMapper mapper, params object[] parameters);
        List<T> GetListSP<T>(string name, params object[] parameters);
        List<IDictionary<string, object>> GetListSP(string name, IMapper mapper, params object[] parameters);
        List<IDictionary<string, object>> GetListSP(string name, params object[] parameters);

        
        T ExecuteScalar<T>(string sql, params object[] parameters);
        int ExecuteNonQuery(string sql, params object[] parameters);
        
        [Obsolete]
        int ExecuteUpdate(string sql, params object[] parameters);

        T Insert<T>(object entity, string table, string identityColumn, IMapper mapper);
        T Insert<T>(object entity, string table, string identityColumn);

        void Insert(object entity, string table, IMapper mapper);
        void Insert(object entity, string table);

        int Update(object entity, string table, string selectionColumn, object selectionValue, IMapper mapper);
    }
}
