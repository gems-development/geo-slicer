using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.Intersectors.CoordinateComparators;

public class RefCoordinateComparator : ICoordinateComparator
{
    public bool IsEquals(Coordinate a, Coordinate b)
    {
        return ReferenceEquals(a, b);
    }
}