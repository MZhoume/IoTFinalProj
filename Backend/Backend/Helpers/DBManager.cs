using System;
using System.Collections.Generic;
using System.Data.Common;
using Mono.Data.Sqlite;
using DapperExtensions;
using Dapper;

namespace Backend.Helpers
{
    public class DBManager
    {
        const string _connString = @"Data Source=data.db";

        DbConnection _conn = new SqliteConnection(_connString);

        public DBManager()
        {
            DapperExtensions.DapperExtensions.DefaultMapper = typeof(DapperExtensions.Mapper.PluralizedAutoClassMapper<>);
        }

        public T GetById<T>(int id) where T : class
        {
            _conn.Open();
            var item = _conn.Get<T>(id);
            _conn.Close();
            return item;
        }

        public int Insert<T>(T item) where T : class
        {
            _conn.Open();
            var id = _conn.Insert(item);
            _conn.Close();
            return id;
        }

        public IEnumerable<T> Query<T>(string sql, object param = null)
        {
            return _conn.Query<T>(sql, param);
        }

        public int Execute(string sql, object param = null)
        {
            return _conn.Execute(sql, param);
        }

        public void Update<T>(int id, Action<T> action) where T : class
        {
            _conn.Open();
            var item = _conn.Get<T>(id);
            action(item);
            _conn.Update(item);
            _conn.Close();
        }

        public void Delete<T>(int id) where T : class
        {
            _conn.Open();
            var item = _conn.Get<T>(id);
            _conn.Delete(item);
            _conn.Close();
        }

        public IEnumerable<T> GetList<T>(IFieldPredicate predicate) where T : class
        {
            _conn.Open();
            var list = _conn.GetList<T>(predicate);
            _conn.Close();
            return list;
        }
    }
}
