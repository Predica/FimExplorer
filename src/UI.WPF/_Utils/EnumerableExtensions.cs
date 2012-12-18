using System;
using System.Collections.Generic;
using System.Linq;

namespace Predica.FimExplorer.UI.WPF
{
    public static class EnumerableExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> @this)
        {
            return !@this.Any();
        }

        public static bool IsNotEmpty<T>(this IEnumerable<T> @this)
        {
            return !@this.IsEmpty();
        }

        public static void ForEach<T>(this IEnumerable<T> @this, Action<T> operation)
        {
            foreach (var element in @this)
            {
                operation(element);
            }
        }

        public static bool None<T>(this IEnumerable<T> @this, Func<T, bool> predicate)
        {
            return !@this.Any(predicate);
        }
    }
}