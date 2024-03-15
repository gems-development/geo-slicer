using System;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.Intersectors;

public class AreasIntersector
{
    public bool CheckIntersection(AreasIntersectionType requiredType, LineSegment areaLine1, LineSegment areaLine2)
    {
        return CheckIntersection(requiredType, areaLine1.P0, areaLine1.P1, areaLine2.P0, areaLine2.P1);
    }

    public bool CheckIntersection(
        AreasIntersectionType requiredType,
        Coordinate areaLine1Point1, Coordinate areaLine1Point2,
        Coordinate areaLine2Point1, Coordinate areaLine2Point2)
    {
        AreasIntersectionType actualType =
            GetIntersection(areaLine1Point1, areaLine1Point2, areaLine2Point1, areaLine2Point2);
        return (actualType & requiredType) != 0;
    }

    public AreasIntersectionType GetIntersection(LineSegment areaLine1, LineSegment areaLine2)
    {
        return GetIntersection(areaLine1.P0, areaLine1.P1, areaLine2.P0, areaLine2.P1);
    }

    public AreasIntersectionType GetIntersection(Coordinate areaLine1Point1, Coordinate areaLine1Point2,
        Coordinate areaLine2Point1, Coordinate areaLine2Point2)
    {
        if (Math.Min(areaLine1Point1.X, areaLine1Point2.X) > Math.Max(areaLine2Point1.X, areaLine2Point2.X)
            || Math.Max(areaLine1Point1.X, areaLine1Point2.X) < Math.Min(areaLine2Point1.X, areaLine2Point2.X)
            || Math.Max(areaLine1Point1.Y, areaLine1Point2.Y) < Math.Min(areaLine2Point1.Y, areaLine2Point2.Y)
            || Math.Min(areaLine1Point1.Y, areaLine1Point2.Y) > Math.Max(areaLine2Point1.Y, areaLine2Point2.Y))
        {
            return AreasIntersectionType.Outside;
        }

        return AreasIntersectionType.Inside;
    }
}