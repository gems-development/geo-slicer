using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils;

public static class Converters
{
    public static IEnumerable<LineSegment> LineStringToLineSegments(LineString lineString)
    {
        foreach ((Coordinate, Coordinate) pair in lineString.Coordinates.Pairwise())
        {
            yield return new LineSegment(pair.Item1, pair.Item2);
        }
    }
}