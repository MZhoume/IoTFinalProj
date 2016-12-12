using System.Timers;
using System.Collections.Generic;

namespace Backend.Helpers
{
    public static class Daemon
    {
        static List<Timer> _tasks = new List<Timer>();

        public static int Create(double interval, ElapsedEventHandler action, bool startNow = false, bool oneTime = false)
        {
            var timer = new Timer(interval);
            timer.Elapsed += action;

            _tasks.Add(timer);

            if (oneTime)
            {
                timer.Elapsed += (s, e) =>
                {
                    timer.Stop();
                    _tasks.Remove(timer);
                };
            }

            if (startNow)
            {
                timer.Start();
            }

            return _tasks.Count - 1;
        }

        public static void StartAll() => _tasks.ForEach(t => t.Start());

        public static void StopAll() => _tasks.ForEach(t => t.Stop());

        public static void Remove(int id)
        {
            var timer = _tasks[id];
            timer.Stop();
            _tasks.Remove(timer);
        }
    }
}
