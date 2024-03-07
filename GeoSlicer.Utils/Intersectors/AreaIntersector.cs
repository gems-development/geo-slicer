using System;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.Intersectors;

public class AreaIntersector
{
    private readonly double _epsilon;

    private readonly ICoordinateComparator _coordinateComparator;
    private readonly EpsilonCoordinateComparator _epsilonCoordinateComparator;
    private readonly LineService _lineService;

    public AreaIntersector(ICoordinateComparator coordinateComparator, LineService lineService, double epsilon)
    {
        _coordinateComparator = coordinateComparator;
        _epsilonCoordinateComparator = new EpsilonCoordinateComparator(epsilon);
        _epsilon = epsilon;
        _lineService = lineService;
    }

    public bool CheckIntersection(AreaIntersectionType requiredType, LineSegment areaLine, LineSegment currentLine)
    {
        return CheckIntersection(requiredType, areaLine.P0, areaLine.P1, currentLine.P0, currentLine.P1);
    }

    public bool CheckIntersection(
        AreaIntersectionType requiredType,
        Coordinate areaLinePoint1, Coordinate areaLinePoint2,
        Coordinate currentLinePoint1, Coordinate currentLinePoint2)
    {
        AreaIntersectionType actualType =
            GetIntersection(areaLinePoint1, areaLinePoint2, currentLinePoint1, currentLinePoint2);
        return (actualType & requiredType) != 0;
    }

    public AreaIntersectionType GetIntersection(LineSegment areaLine, LineSegment currentLine)
    {
        return GetIntersection(areaLine.P0, areaLine.P1, currentLine.P0, currentLine.P1);
    }

    private AreaIntersectionType GetIntersection(Coordinate areaLinePoint1, Coordinate areaLinePoint2,
        Coordinate currentLinePoint1, Coordinate currentLinePoint2)
    {
        if (_lineService.IsCoordinateInSegmentBorders(currentLinePoint1, areaLinePoint1, areaLinePoint2)
            && _lineService.IsCoordinateInSegmentBorders(currentLinePoint2, areaLinePoint1, areaLinePoint2))
        {
            return AreaIntersectionType.Inside;
        }

        if ((_lineService.IsCoordinateInSegmentBorders(currentLinePoint1, areaLinePoint1, areaLinePoint2)
             && !_lineService.IsCoordinateInSegmentBorders(currentLinePoint2, areaLinePoint1, areaLinePoint2))
            || (_lineService.IsCoordinateInSegmentBorders(currentLinePoint2, areaLinePoint1, areaLinePoint2)
                && !_lineService.IsCoordinateInSegmentBorders(currentLinePoint1, areaLinePoint1, areaLinePoint2)))
        {
            return AreaIntersectionType.PartlyInside;
        }

        if (_lineService.IsCoordinateInSegmentBorders(areaLinePoint1, currentLinePoint1, currentLinePoint2)
            || _lineService.IsCoordinateInSegmentBorders(areaLinePoint2, currentLinePoint1, currentLinePoint2)
            || _lineService.IsCoordinateInSegmentBorders(areaLinePoint1.X, areaLinePoint2.Y, currentLinePoint1, currentLinePoint2)
            || _lineService.IsCoordinateInSegmentBorders(areaLinePoint2.X, areaLinePoint1.Y, currentLinePoint1, currentLinePoint2))
        {
            return !_lineService.IsRectangleOnOneSideOfLine(currentLinePoint1, currentLinePoint2, 
                areaLinePoint1, areaLinePoint2) ? AreaIntersectionType.Overlay : AreaIntersectionType.Outside;
        }

        if (Math.Min(currentLinePoint1.X, currentLinePoint2.X) <= Math.Min(areaLinePoint1.X, areaLinePoint2.X) + _epsilon
            && Math.Max(currentLinePoint1.X, currentLinePoint2.X) >= Math.Max(areaLinePoint1.X, areaLinePoint2.X) - _epsilon
            && Math.Min(currentLinePoint1.Y, currentLinePoint2.Y) >= Math.Min(areaLinePoint1.Y, areaLinePoint2.Y) - _epsilon
            && Math.Max(currentLinePoint1.Y, currentLinePoint2.Y) <= Math.Max(areaLinePoint1.Y, areaLinePoint2.Y) + _epsilon)
        {
            return AreaIntersectionType.Overlay;
        }
        return AreaIntersectionType.Outside;
    }
}