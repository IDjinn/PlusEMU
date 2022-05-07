using System;
using System.Collections.Generic;
using System.Linq;

namespace Plus.Utilities.Collections
{
    public static class IEnumerableExtensions
    {
        public static TSource GetRandomValue<TSource>(this IEnumerable<TSource> source)
        {
            var rand = Random.Shared;
            var enumerable = source as TSource[] ?? source.ToArray();
            return enumerable.ElementAt(rand.Next(enumerable.Length));
        }
    }
}