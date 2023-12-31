﻿using System.Collections.Generic;

namespace AdventureInc
{
    // ReSharper disable once InconsistentNaming
    public static class IDictionaryExt
    {
        public static TValue? TryGet<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key) =>
            dict.TryGetValue(key, out var value)
                ? value
                : default;

        public static TValue? TryGet<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) =>
            dict.TryGetValue(key, out var value)
                ? value
                : default;
    }
}