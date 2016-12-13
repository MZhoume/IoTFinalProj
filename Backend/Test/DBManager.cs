using System;
using System.Collections.Generic;
using System.Data.Common;
using Dapper;
using Mono.Data.Sqlite;

namespace Test
{
    public class DBManager
    {
        const string _connString = @"Data Source=data.db";

        DbConnection _conn = new SqliteConnection(_connString);

        public IEnumerable<T> Query<T>(string sql, object param = null)
        {
            _conn.Open();
            var result =  _conn.Query<T>(sql, param);
            _conn.Close();
            return result;
        }

        public int Execute(string sql, object param = null)
        {
            _conn.Open();
            var result = _conn.Execute(sql, param);
            _conn.Close();
            return result;
        }
    }
}
