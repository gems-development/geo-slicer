using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.Intersectors.CoordinateComparators;

public interface ICoordinateComparator
{
    bool IsEquals(Coordinate a, Coordinate b);
}