using System;
using System.Collections.Generic;

namespace Lotus.Extensions;

public static class EnumerableExtensions
{
    public static List<T> ToList<T>(this Il2CppSystem.Collections.Generic.HashSet<T> hashSet)
    {
        List<T> list = new();
        foreach (T item in hashSet)
            list.Add(item);
        return list;
    }

    private static Random rng = new Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    public static T RemoveLast<T>(this IList<T> list)
    {
        int index = list.Count - 1;
        T item = list[index];
        list.RemoveAt(index);
        return item;
    }
}