using System.Collections.Generic;

namespace GeoSlicer.Utils;

public static class MyIEnumerableExtensions
{
    public static IEnumerable<(T, T)> Pairwise<T>(this IEnumerable<T> source)
    {
        using var it = source.GetEnumerator();
        if (!it.MoveNext())
            yield break;

        T previous = it.Current;

        while (it.MoveNext())
            yield return (previous, previous = it.Current);
    }
}