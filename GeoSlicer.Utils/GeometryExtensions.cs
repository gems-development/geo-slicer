using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils;

public static class GeometryExtensions
{
    /// <summary>
    /// Проверяет на равенство со смещением 2 LinearRing, представленных
    /// IEnumerable&lt;Coordinate&gt; 
    /// </summary>
    public static bool IsEqualsRings(this IEnumerable<Coordinate> source, IEnumerable<Coordinate> other)
    {
        // Пропускаем первые точки, чтобы убрать дубликат, повторяем source 2 раза и ищем в нем other
        return source.Skip(1).Concat(source.Skip(1)).Contains(other.Skip(1));
    }

    /// <summary>
    /// Проверяет на равенство со смещением 2 LinearRing
    /// </summary>
    public static bool IsEqualsRings(this LinearRing source, LinearRing other)
    {
        return source.Coordinates.IsEqualsRings(other.Coordinates);
    }

    /// <summary>
    /// Проверяет на равенство со смещением 2 Polygon, не учитывая порядок дыр
    /// </summary>
    public static bool IsEqualsPolygons(this Polygon source, Polygon other)
    {
        return source.Shell.IsEqualsRings(other.Shell)
               && source.Holes.IsEqualsRingCollection(other.Holes);
    }

    /// <summary>
    /// Проверяет на равенство со смещением 2 коллекции LinearRing, представленных
    /// IEnumerable&lt;Coordinate&gt;, не учитывая порядок
    /// </summary>
    public static bool IsEqualsRingCollection(this IEnumerable<IEnumerable<Coordinate>> source,
        IEnumerable<IEnumerable<Coordinate>> other)
    {
        return source.Count() == other.Count()
               && source.All(enumerable => other.Any(coordinates => coordinates.IsEqualsRings(enumerable)))
               && other.All(enumerable => source.Any(coordinates => coordinates.IsEqualsRings(enumerable)));
    }

    /// <summary>
    /// Проверяет на равенство со смещением 2 коллекции LinearRing, не учитывая порядок
    /// </summary>
    public static bool IsEqualsRingCollection(this IEnumerable<LinearRing> source,
        IEnumerable<LinearRing> other)
    {
        return source.Select(ring => ring.Coordinates).IsEqualsRingCollection(other.Select(ring => ring.Coordinates));
    }

    /// <summary>
    /// Проверяет на равенство со смещением 2 коллекции Polygon, не учитывая порядок дыр
    /// </summary>
    public static bool IsEqualsPolygonCollections(this IEnumerable<Polygon> source, IEnumerable<Polygon> other)
    {
        return source.Count() == other.Count()
               && source.All(polygon => other.Any(polygon.IsEqualsPolygons))
               && other.All(polygon => source.Any(polygon.IsEqualsPolygons));
    }
}