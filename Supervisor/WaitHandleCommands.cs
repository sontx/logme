using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Supervisor
{
    internal class WaitHandleCommands : IDisposable
    {
        private readonly Dictionary<string, AutoResetEvent> events = new Dictionary<string, AutoResetEvent>();

        public void Add(string command)
        {
            events.Add(command, new AutoResetEvent(false));
        }

        public void Wait(string command, int timeout = 30000)
        {
            var @event = events[command];
            if (@event != null)
            {
                @event.Reset();
                @event.WaitOne(timeout);
            }
        }

        public Task WaitAsync(string command, int timeout = 30000)
        {
            return Task.Run(() => Wait(command, timeout));
        }

        public void Set(string command)
        {
            var @event = events[command];
            if (@event != null)
                @event.Set();
        }

        public void Dispose()
        {
            foreach (var pair in events)
            {
                pair.Value.Dispose();
            }
            events.Clear();
        }
    }
}