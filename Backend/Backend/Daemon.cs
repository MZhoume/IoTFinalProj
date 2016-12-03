using System;
using System.Timers;
using System.Collections.Generic;

namespace Backend
{
    public static class Daemon
    {
        private static List<Timer> _tasks = new List<Timer>();

        public static int Create(double interval, ElapsedEventHandler action)
        {
            var timer = new Timer(interval);
            timer.Elapsed += action;

            _tasks.Add(timer);

            return _tasks.Count - 1;
        }

        public static void StartAll()
        {
            foreach (var t in _tasks)
            {
                t.Start();
            }
        }

        public static void StopAll()
        {
            foreach (var t in _tasks)
            {
                t.Stop();
            }
        }

        public static void Remove(int id)
        {
            var timer = _tasks[id];
            timer.Stop();
            _tasks.Remove(timer);
        }
    }
}
