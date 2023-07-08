﻿using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace GMTK2023
{
    // ReSharper disable once InconsistentNaming
    public static class IEnumerableExt
    {
        public static void Iter<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }

        public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> items, Func<T, bool> pred) =>
            items.Where(it => !pred(it));

        public static T? TryRandom<T>(this IReadOnlyCollection<T> items)
        {
            if (!items.Any()) return default;

            var index = Random.Range(0, items.Count);
            return items.ElementAt(index);
        }
    }
}