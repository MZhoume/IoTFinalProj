using System;
using System.Collections.Generic;
using System.Data.Common;
using Dapper;
using Mono.Data.Sqlite;

namespace Backend
{
    public class DBManager
    {
        private static string _connString = @"Data Source=data.db";

        private DbConnection _conn = new SqliteConnection(_connString);

        public DBManager()
        {
        }

        public IEnumerable<T> Query<T>(string sql, object param = null)
        {
            return _conn.Query<T>(sql, param);
        }

        public int Execute(string sql, object param = null)
        {
            return _conn.Execute(sql, param);
        }
    }
}
