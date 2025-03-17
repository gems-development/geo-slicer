using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.Intersectors.CoordinateComparators;

/// <summary>
/// Проверяет координаты на равенство
/// </summary>
public interface ICoordinateComparator
{
    bool IsEquals(Coordinate a, Coordinate b);
}