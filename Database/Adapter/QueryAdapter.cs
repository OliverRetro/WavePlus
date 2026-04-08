using System;
using System.Data;
using MySqlConnector;
using Plus.Core;
using Plus.Database.Interfaces;

namespace Plus.Database.Adapter
{
    public class QueryAdapter : IRegularQueryAdapter
    {
        protected IDatabaseClient Client;
        protected MySqlCommand Command;

        public bool DbEnabled = true;

        public QueryAdapter(IDatabaseClient client)
        {
            Client = client;
        }

        public void AddParameter(string parameterName, object val)
        {
            Command.Parameters.AddWithValue(parameterName, val);
        }

        public bool FindsResult()
        {
            bool hasRows = false;
            try
            {
                using (MySqlDataReader reader = Command.ExecuteReader())
                {
                    hasRows = reader.HasRows;
                }
            }
            catch (Exception exception)
            {
                ExceptionLogger.LogQueryError(Command.CommandText, exception);
            }

            return hasRows;
        }

        public int GetInteger()
        {
            int result = 0;
            try
            {
                object obj2 = Command.ExecuteScalar();
                if (obj2 != null)
                {
                    int.TryParse(obj2.ToString(), out result);
                }
            }
            catch (Exception exception)
            {
                ExceptionLogger.LogQueryError(Command.CommandText, exception);
            }

            return result;
        }

        public DataRow GetRow
            ()
        {
            DataRow row = null;
            try
            {
                DataSet dataSet = new();
                using (MySqlDataAdapter adapter = new(Command))
                {
                    adapter.Fill(dataSet);
                }

                if ((dataSet.Tables.Count > 0) && (dataSet.Tables[0].Rows.Count == 1))
                {
                    row = dataSet.Tables[0].Rows[0];
                }
            }
            catch (Exception exception)
            {
                ExceptionLogger.LogQueryError(Command.CommandText, exception);
            }

            return row;
        }

        public string GetString()
        {
            string str = string.Empty;
            try
            {
                object obj2 = Command.ExecuteScalar();
                if (obj2 != null)
                {
                    str = obj2.ToString();
                }
            }
            catch (Exception exception)
            {
                ExceptionLogger.LogQueryError(Command.CommandText, exception);
            }

            return str;
        }

        public DataTable GetTable()
        {
            var dataTable = new DataTable();
            if (!DbEnabled)
                return dataTable;

            try
            {
                using (MySqlDataAdapter adapter = new(Command))
                {
                    adapter.Fill(dataTable);
                }
            }
            catch (Exception exception)
            {
                ExceptionLogger.LogQueryError(Command.CommandText, exception);
            }

            return dataTable;
        }

        public void RunQuery(string query)
        {
            if (!DbEnabled)
                return;

            SetQuery(query);
            RunQuery();
        }

        public void SetQuery(string query)
        {
            Command.Parameters.Clear();
            Command.CommandText = query;
        }

        public long InsertQuery()
        {
            if (!DbEnabled)
                return 0;

            long lastInsertedId = 0L;
            try
            {
                Command.ExecuteScalar();
                lastInsertedId = Command.LastInsertedId;
            }
            catch (Exception exception)
            {
                ExceptionLogger.LogQueryError(Command.CommandText, exception);
            }

            return lastInsertedId;
        }

        public void RunQuery()
        {
            if (!DbEnabled)
                return;

            try
            {
                Command.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                ExceptionLogger.LogQueryError(Command.CommandText, exception);
            }
        }

        public DataRow GetRowOrInsert(string selectQuery, Action<IQueryAdapter> selectParameters, string insertQuery, Action<IQueryAdapter> insertParameters)
        {
            if (!DbEnabled)
                return null;

            SetQuery(selectQuery);
            selectParameters?.Invoke((IQueryAdapter)this);

            DataRow row = GetRow();
            if (row != null)
                return row;

            try
            {
                SetQuery(insertQuery);
                insertParameters?.Invoke((IQueryAdapter)this);
                Command.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                if (!(exception is MySqlException mySqlException) || mySqlException.Number != 1062)
                {
                    ExceptionLogger.LogQueryError(Command.CommandText, exception);
                    return null;
                }
            }

            SetQuery(selectQuery);
            selectParameters?.Invoke((IQueryAdapter)this);
            return GetRow();
        }

        public void UpdateOrInsert(string updateQuery, Action<IQueryAdapter> updateParameters, string insertQuery, Action<IQueryAdapter> insertParameters)
        {
            if (!DbEnabled)
                return;

            try
            {
                SetQuery(updateQuery);
                updateParameters?.Invoke((IQueryAdapter)this);

                if (Command.ExecuteNonQuery() > 0)
                    return;
            }
            catch (Exception exception)
            {
                ExceptionLogger.LogQueryError(Command.CommandText, exception);
                return;
            }

            try
            {
                SetQuery(insertQuery);
                insertParameters?.Invoke((IQueryAdapter)this);
                Command.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                if (exception is MySqlException mySqlException && mySqlException.Number == 1062)
                {
                    SetQuery(updateQuery);
                    updateParameters?.Invoke((IQueryAdapter)this);
                    RunQuery();
                    return;
                }

                ExceptionLogger.LogQueryError(Command.CommandText, exception);
            }
        }
    }
}
