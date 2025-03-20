using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils;

public static class MyIEnumerableExtensions
{
    
    /// <summary>
    /// Возвращает элементы <paramref name="source"/> попарно (01, 12, 23, 34...)
    /// </summary>
    public static IEnumerable<(T, T)> Pairwise<T>(this IEnumerable<T> source)
    {
        using var it = source.GetEnumerator();
        if (!it.MoveNext())
            yield break;

        T previous = it.Current;

        while (it.MoveNext())
            yield return (previous, previous = it.Current);
    }
    
    public static bool Contains<T>(this IEnumerable<T> source, IEnumerable<T> other)
    {
        T first = other.First();
        int otherLength = other.Count();
        while (source.Any())
        {
            source = source.SkipWhile(arg => !Equals(arg, first));
            if (source.Take(otherLength).SequenceEqual(other))
            {
                return true;
            }

            source = source.Skip(1);
        }

        return false;

    }


    
}
