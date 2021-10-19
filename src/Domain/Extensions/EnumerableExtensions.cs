using System;
using System.Collections.Generic;
using System.Linq;

namespace PlexRipper.Domain
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Completely flattens a multi-dimensional List
        /// Source: (https://stackoverflow.com/a/21054096/8205497).
        /// </summary>
        /// <param name="source"></param>
        /// <param name="recursion"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> Flatten<T, R>(this IEnumerable<T> source, Func<T, R> recursion)
            where R : IEnumerable<T>
        {
            return source.SelectMany(x => recursion(x) != null && recursion(x).Any() ? recursion(x).Flatten(recursion) : null)
                .Where(x => x != null);
        }
    }
}