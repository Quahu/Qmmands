using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Qmmands
{
    internal class AsyncEvent<T> where T : class
    {
        public IReadOnlyList<T> Handlers => _handlers;

        private ImmutableList<T> _handlers;
        private readonly object _lock = new object();

        public AsyncEvent()
            => _handlers = ImmutableList.Create<T>();

        public void Hook(T handler)
        {
            lock (_lock)
            {
                _handlers = _handlers.Add(handler);
            }
        }

        public void Unhook(T handler)
        {
            lock (_lock)
            {
                _handlers = _handlers.Remove(handler);
            }
        }
    }

    internal static class AsyncEventExtensions
    {
        public static async Task InvokeAsync(this AsyncEvent<Func<Task>> asyncEvent)
        {
            for (var i = 0; i < asyncEvent.Handlers.Count; i++)
                await asyncEvent.Handlers[i].Invoke().ConfigureAwait(false);
        }

        public static async Task InvokeAsync<T1>(this AsyncEvent<Func<T1, Task>> asyncEvent, T1 arg1)
        {
            for (var i = 0; i < asyncEvent.Handlers.Count; i++)
                await asyncEvent.Handlers[i].Invoke(arg1).ConfigureAwait(false);
        }

        public static async Task InvokeAsync<T1, T2>(this AsyncEvent<Func<T1, T2, Task>> asyncEvent, T1 arg1, T2 arg2)
        {
            for (var i = 0; i < asyncEvent.Handlers.Count; i++)
                await asyncEvent.Handlers[i].Invoke(arg1, arg2).ConfigureAwait(false);
        }

        public static async Task InvokeAsync<T1, T2, T3>(this AsyncEvent<Func<T1, T2, T3, Task>> asyncEvent, T1 arg1, T2 arg2, T3 arg3)
        {
            for (var i = 0; i < asyncEvent.Handlers.Count; i++)
                await asyncEvent.Handlers[i].Invoke(arg1, arg2, arg3).ConfigureAwait(false);
        }

        public static async Task InvokeAsync<T1, T2, T3, T4>(this AsyncEvent<Func<T1, T2, T3, T4, Task>> asyncEvent, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            for (var i = 0; i < asyncEvent.Handlers.Count; i++)
                await asyncEvent.Handlers[i].Invoke(arg1, arg2, arg3, arg4).ConfigureAwait(false);
        }
    }
}
