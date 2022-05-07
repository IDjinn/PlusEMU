using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Plus.Utilities.Collections
{
    public static class DictionaryExtensions
    {
        public static TValue GetRandomValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) where TKey : notnull
        {
            var rand = Random.Shared;
            return dictionary.ElementAt(rand.Next(0, dictionary.Count)).Value;
        }
    }
}