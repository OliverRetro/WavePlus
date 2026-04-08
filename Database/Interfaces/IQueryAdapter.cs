using System;
using System.Data;

namespace Plus.Database.Interfaces
{
    public interface IQueryAdapter : IRegularQueryAdapter, IDisposable
    {
        DataRow GetRowOrInsert(string selectQuery, Action<IQueryAdapter> selectParameters, string insertQuery, Action<IQueryAdapter> insertParameters);
        long InsertQuery();
        void RunQuery();
        void UpdateOrInsert(string updateQuery, Action<IQueryAdapter> updateParameters, string insertQuery, Action<IQueryAdapter> insertParameters);
    }
}
