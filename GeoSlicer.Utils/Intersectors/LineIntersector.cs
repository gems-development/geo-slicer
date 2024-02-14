using System;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.Intersectors;

using CanonicalLine = Tuple<double, double, double>;

public class LineIntersector
{
    private readonly double _epsilon;

    private readonly ICoordinateComparator _coordinateComparator;
    private readonly EpsilonCoordinateComparator _epsilonCoordinateComparator;
    private readonly LineService _lineService;

    public LineIntersector(ICoordinateComparator coordinateComparator, double epsilon = 1E-5)
    {
        _coordinateComparator = coordinateComparator;
        _epsilonCoordinateComparator = new EpsilonCoordinateComparator(epsilon);
        _epsilon = epsilon;
        _lineService = new LineService(epsilon);
    }

    public bool CheckIntersection(IntersectionType requiredType, LineSegment a, LineSegment b)
    {
        return CheckIntersection(requiredType, a.P0, a.P1, b.P0, b.P1);
    }

    public (IntersectionType, Coordinate?) GetIntersection(LineSegment a, LineSegment b)
    {
        return GetIntersection(a.P0, a.P1, b.P0, b.P1);
    }

    public bool CheckIntersection(
        IntersectionType requiredType,
        Coordinate a1, Coordinate a2,
        Coordinate b1, Coordinate b2)
    {
        IntersectionType actualType = GetIntersection(a1, a2, b1, b2).Item1;
        return (actualType & requiredType) != 0;
    }

    public (IntersectionType, Coordinate?) GetIntersection(Coordinate a1, Coordinate a2, Coordinate b1, Coordinate b2)
    {
        (double a, double b, double c) canonicalA = LineService.ToCanonical(a1, a2);
        (double a, double b, double c) canonicalB = LineService.ToCanonical(b1, b2);
        Coordinate? intersectionCoordinate = GetIntersectionCoordinate(canonicalA, canonicalB);


        // Прямые параллельны, проверка на Extension, Part, Equals, Contains, Overlay, NoIntersection
        if (intersectionCoordinate is null)
        {
            // Проверка, есть ли между прямыми расстояние
            if (_lineService.IsLineEquals(canonicalA, canonicalB))
                return (GetParallelIntersection(a1, a2, b1, b2), null);
            return (IntersectionType.NoIntersection, null);
        }

        // Есть точка пересечения, проверка на Inner, Corner, TyShaped, Outside

        if (_coordinateComparator.IsEquals(a1, b1) ||
            _coordinateComparator.IsEquals(a1, b2) ||
            _coordinateComparator.IsEquals(a2, b1) ||
            _coordinateComparator.IsEquals(a2, b2))
        {
            return (IntersectionType.Corner, intersectionCoordinate);
        }

        if (
            (_epsilonCoordinateComparator.IsEquals(intersectionCoordinate, a1)
             || _epsilonCoordinateComparator.IsEquals(intersectionCoordinate, a2))
            && _lineService.IsCoordinateInSegmentBorders(intersectionCoordinate, b1, b2)
            ||
            (_epsilonCoordinateComparator.IsEquals(intersectionCoordinate, b1)
             || _epsilonCoordinateComparator.IsEquals(intersectionCoordinate, b2))
            && _lineService.IsCoordinateInSegmentBorders(intersectionCoordinate, a1, a2)
        )
        {
            return (IntersectionType.TyShaped, intersectionCoordinate);
        }

        // Если пересечение внутри одного отрезка, то оно и внутри другого
        if (_lineService.IsCoordinateInSegmentBorders(intersectionCoordinate, a1, a2) &&
            _lineService.IsCoordinateInSegmentBorders(intersectionCoordinate, b1, b2))
        {
            return (IntersectionType.Inner, intersectionCoordinate);
        }

        return (IntersectionType.Outside, intersectionCoordinate);
    }


    private IntersectionType GetParallelIntersection(
        Coordinate a1, Coordinate a2,
        Coordinate b1, Coordinate b2)
    {
        IntersectionType? CheckEnds(Coordinate a1, Coordinate a2, Coordinate b1, Coordinate b2)
        {
            if (_coordinateComparator.IsEquals(a1, b1))
            {
                if (_coordinateComparator.IsEquals(a2, b2))
                {
                    return IntersectionType.Equals;
                }

                if (_lineService.IsCoordinateInSegmentBorders(a2, b1, b2) ||
                    _lineService.IsCoordinateInSegmentBorders(b2, a1, a2))
                {
                    return IntersectionType.Part;
                }

                return IntersectionType.Extension;
            }

            return null;
        }

        // Проверка на Extension, Part, Equals
        IntersectionType? result = CheckEnds(a1, a2, b1, b2);
        if (result is not null)
            return (IntersectionType)result;
        result = CheckEnds(a1, a2, b2, b1);
        if (result is not null)
            return (IntersectionType)result;
        result = CheckEnds(a2, a1, b1, b2);
        if (result is not null)
            return (IntersectionType)result;
        result = CheckEnds(a2, a1, b2, b1);
        if (result is not null)
            return (IntersectionType)result;

        // Проверка на Contains
        if (_lineService.IsCoordinateInSegmentBorders(b1, a1, a2)
            && _lineService.IsCoordinateInSegmentBorders(b2, a1, a2)
            || _lineService.IsCoordinateInSegmentBorders(a1, b1, b2)
            && _lineService.IsCoordinateInSegmentBorders(a2, b1, b2))
        {
            return IntersectionType.Contains;
        }

        //Проверка на Overlay
        if ((_lineService.IsCoordinateInSegmentBorders(b1, a1, a2)
             || _lineService.IsCoordinateInSegmentBorders(b2, a1, a2)) 
            && (_lineService.IsCoordinateInSegmentBorders(a1, b1, b2)
                || _lineService.IsCoordinateInSegmentBorders(a2, b1, b2)))
        {
            return IntersectionType.Overlay;
        }
        return IntersectionType.Outside;
    }

    public Coordinate? GetIntersectionCoordinate(
        Coordinate a1, Coordinate a2,
        Coordinate b1, Coordinate b2)
    {
        return GetIntersectionCoordinate(LineService.ToCanonical(a1, a2), LineService.ToCanonical(b1, b2));
    }

    private Coordinate? GetIntersectionCoordinate(
        (double a, double b, double c) line1,
        (double a, double b, double c) line2)
    {
        double delta = line1.a * line2.b - line2.a * line1.b;

        if (Math.Abs(delta) <= _epsilon)
        {
            return null;
        }

        double x = (line2.b * line1.c - line1.b * line2.c) / delta;
        double y = (line1.a * line2.c - line2.a * line1.c) / delta;
        return new Coordinate(x, y);
    }
}