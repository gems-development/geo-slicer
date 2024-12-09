using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils;

public static class GeometryExtensions
{
    public static bool IsEqualsRings(this IEnumerable<Coordinate> source, IEnumerable<Coordinate> other)
    {
        return source.Skip(1).Concat(source.Skip(1)).Contains(other.Skip(1));
    }
    
    public static bool IsEqualsRings(this LinearRing source, LinearRing other)
    {
        return source.Coordinates.IsEqualsRings(other.Coordinates);
    }

    public static bool IsEqualsPolygons(this Polygon source, Polygon other)
    {
        return source.Shell.IsEqualsRings(other.Shell)
               && source.Holes.IsEqualsRingCollection(other.Holes);
    }

    public static bool IsEqualsRingCollection(this IEnumerable<IEnumerable<Coordinate>> source,
        IEnumerable<IEnumerable<Coordinate>> other)
    {
        return source.All(enumerable => other.Any(coordinates => coordinates.IsEqualsRings(enumerable)))
               && other.All(enumerable => source.Any(coordinates => coordinates.IsEqualsRings(enumerable)))
               && source.Count() == other.Count();
    }
    
    public static bool IsEqualsRingCollection(this IEnumerable<LinearRing> source,
        IEnumerable<LinearRing> other)
    {
        return source.Select(ring => ring.Coordinates).IsEqualsRingCollection(other.Select(ring => ring.Coordinates));
    }

    public static bool IsEqualsPolygonCollections(this IEnumerable<Polygon> source, IEnumerable<Polygon> other)
    {
        return source.All(polygon => other.Any(polygon.IsEqualsPolygons))
               && other.All(polygon => source.Any(polygon.IsEqualsPolygons))
               && source.Count() == other.Count();
    }
}
