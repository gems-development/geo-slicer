using System;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.Intersectors.IntersectionTypes;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.Intersectors;

public class LineAreaIntersector
{
    private readonly double _epsilon;

    private readonly ICoordinateComparator _coordinateComparator;
    private readonly EpsilonCoordinateComparator _epsilonCoordinateComparator;
    private readonly LineService _lineService;

    public LineAreaIntersector(ICoordinateComparator coordinateComparator, LineService lineService, double epsilon)
    {
        _coordinateComparator = coordinateComparator;
        _epsilonCoordinateComparator = new EpsilonCoordinateComparator(epsilon);
        _epsilon = epsilon;
        _lineService = lineService;
    }

    public bool CheckIntersection(LineAreaIntersectionType requiredType, LineSegment areaLine, LineSegment currentLine)
    {
        return CheckIntersection(requiredType, areaLine.P0, areaLine.P1, currentLine.P0, currentLine.P1);
    }

    public bool CheckIntersection(
        LineAreaIntersectionType requiredType,
        Coordinate areaLinePoint1, Coordinate areaLinePoint2,
        Coordinate currentLinePoint1, Coordinate currentLinePoint2)
    {
        LineAreaIntersectionType actualType =
            GetIntersection(areaLinePoint1, areaLinePoint2, currentLinePoint1, currentLinePoint2);
        return (actualType & requiredType) != 0;
    }

    public LineAreaIntersectionType GetIntersection(LineSegment areaLine, LineSegment currentLine)
    {
        return GetIntersection(areaLine.P0, areaLine.P1, currentLine.P0, currentLine.P1);
    }

    public LineAreaIntersectionType GetIntersection(Coordinate areaLinePoint1, Coordinate areaLinePoint2,
        Coordinate currentLinePoint1, Coordinate currentLinePoint2)
    {
        if (_lineService.IsCoordinateInSegmentBorders(currentLinePoint1, areaLinePoint1, areaLinePoint2)
            && _lineService.IsCoordinateInSegmentBorders(currentLinePoint2, areaLinePoint1, areaLinePoint2))
        {
            return LineAreaIntersectionType.Inside;
        }

        if ((_lineService.IsCoordinateInSegmentBorders(currentLinePoint1, areaLinePoint1, areaLinePoint2)
             && !_lineService.IsCoordinateInSegmentBorders(currentLinePoint2, areaLinePoint1, areaLinePoint2))
            || (_lineService.IsCoordinateInSegmentBorders(currentLinePoint2, areaLinePoint1, areaLinePoint2)
                && !_lineService.IsCoordinateInSegmentBorders(currentLinePoint1, areaLinePoint1, areaLinePoint2)))
        {
            return LineAreaIntersectionType.PartlyInside;
        }

        if (_lineService.IsCoordinateInSegmentBorders(areaLinePoint1, currentLinePoint1, currentLinePoint2)
            || _lineService.IsCoordinateInSegmentBorders(areaLinePoint2, currentLinePoint1, currentLinePoint2)
            || _lineService.IsCoordinateInSegmentBorders(areaLinePoint1.X, areaLinePoint2.Y, currentLinePoint1, currentLinePoint2)
            || _lineService.IsCoordinateInSegmentBorders(areaLinePoint2.X, areaLinePoint1.Y, currentLinePoint1, currentLinePoint2))
        {
            return !_lineService.IsRectangleOnOneSideOfLine(currentLinePoint1, currentLinePoint2, 
                areaLinePoint1, areaLinePoint2) ? LineAreaIntersectionType.Overlay : LineAreaIntersectionType.Outside;
        }

        if (Math.Min(currentLinePoint1.X, currentLinePoint2.X) <= Math.Min(areaLinePoint1.X, areaLinePoint2.X) + _epsilon
            && Math.Max(currentLinePoint1.X, currentLinePoint2.X) >= Math.Max(areaLinePoint1.X, areaLinePoint2.X) - _epsilon
            && Math.Min(currentLinePoint1.Y, currentLinePoint2.Y) >= Math.Min(areaLinePoint1.Y, areaLinePoint2.Y) - _epsilon
            && Math.Max(currentLinePoint1.Y, currentLinePoint2.Y) <= Math.Max(areaLinePoint1.Y, areaLinePoint2.Y) + _epsilon)
        {
            return LineAreaIntersectionType.Overlay;
        }
        return LineAreaIntersectionType.Outside;
    }
}