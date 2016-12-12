using System;
using System.Collections.Generic;

namespace Backend.Services
{
    public static class ServiceLocator
    {
        static Dictionary<string, object> _services = new Dictionary<string, object>();

        public static T GetService<T>() where T: new()
        {
            var name = typeof(T).Name;
            if (_services.ContainsKey(name))
            {
                return (T)_services[name];
            }
            else
            {
                var instance = new T();
                _services.Add(name, instance);
                return instance;
            }
        }

        public static T GetService<T, U>() where U: T, new()
        {
            var name = typeof(T).Name;
            if (_services.ContainsKey(name))
            {
                return (T)_services[name];
            }
            else
            {
                var instance = new U();
                _services.Add(name, instance);
                return instance;
            }
        }
    }
}
