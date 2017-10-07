using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmaJpg2Mp3
{
    static class ObjectExtensions
    {
        public static async Task PipeAsync<T>(this T @object, Func<T, Task> action) => await action(@object);
        public static async Task<TReturn> PipeAsync<T, TReturn>(this T @object, Func<T, Task<TReturn>> func) => await func(@object);

        public static void Pipe<T>(this T @object, Action<T> action) => action(@object);
        public static TReturn Pipe<T, TReturn>(this T @object, Func<T, TReturn> func) => func(@object);
    }
}
