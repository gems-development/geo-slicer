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

    public LineIntersector(ICoordinateComparator coordinateComparator, LineService lineService, double epsilon)
    {
        _coordinateComparator = coordinateComparator;
        _epsilonCoordinateComparator = new EpsilonCoordinateComparator(epsilon);
        _epsilon = epsilon;
        _lineService = lineService;
    }

    public bool CheckIntersection(IntersectionType requiredType, LineSegment a, LineSegment b)
    {
        return CheckIntersection(requiredType, a.P0, a.P1, b.P0, b.P1);
    }

    public bool CheckIntersection(
        IntersectionType requiredType,
        Coordinate a1, Coordinate a2,
        Coordinate b1, Coordinate b2)
    {
        IntersectionType actualType =
            GetIntersection(a1, a2, b1, b2, out double x, out double y, out bool isIntersects);
        return (actualType & requiredType) != 0;
    }

    public (IntersectionType, Coordinate?) GetIntersection(Coordinate a1, Coordinate a2, Coordinate b1, Coordinate b2)
    {
        IntersectionType intersectionType =
            GetIntersection(a1, a2, b1, b2, out double x, out double y, out bool isIntersects);
        Coordinate? coordinate = null;
        if (isIntersects)
        {
            coordinate = new Coordinate(x, y);
        }

        return (intersectionType, coordinate);
    }

    public (IntersectionType, Coordinate?) GetIntersection(LineSegment a, LineSegment b)
    {
        return GetIntersection(a.P0, a.P1, b.P0, b.P1);
    }

    // Возвращает тип пересечения и точку пересечения в out переменных 
    private IntersectionType GetIntersection(Coordinate a1, Coordinate a2, Coordinate b1, Coordinate b2, out double x,
        out double y, out bool isIntersects)
    {
        (double a, double b, double c) canonicalA = LineService.ToCanonical(a1, a2);
        (double a, double b, double c) canonicalB = LineService.ToCanonical(b1, b2);
        GetIntersectionCoordinate(canonicalA, canonicalB, out x, out y, out isIntersects);

        // Прямые параллельны, проверка на Extension, Part, Equals, Contains, Overlay, NoIntersection
        if (!isIntersects)
        {
            // Проверка, есть ли между прямыми расстояние
            if (_lineService.IsLineEquals(canonicalA, canonicalB))
            {
                return GetParallelIntersection(a1, a2, b1, b2);
            }

            return IntersectionType.NoIntersection;
        }

        // Есть точка пересечения, проверка на Inner, Corner, TyShaped, Outside
        if (_coordinateComparator.IsEquals(a1, b1) ||
            _coordinateComparator.IsEquals(a1, b2) ||
            _coordinateComparator.IsEquals(a2, b1) ||
            _coordinateComparator.IsEquals(a2, b2))
        {
            return IntersectionType.Corner;
        }

        if (
            (_epsilonCoordinateComparator.IsEquals(a1, x, y)
             || _epsilonCoordinateComparator.IsEquals(a2, x, y))
            && _lineService.IsCoordinateInSegmentBorders(x, y, b1, b2)
            ||
            (_epsilonCoordinateComparator.IsEquals(b1, x, y)
             || _epsilonCoordinateComparator.IsEquals(b2, x, y))
            && _lineService.IsCoordinateInSegmentBorders(x, y, a1, a2)
        )
        {
            return IntersectionType.TyShaped;
        }

        // Если пересечение внутри одного отрезка, то оно и внутри другого
        if (_lineService.IsCoordinateInSegmentBorders(x, y, a1, a2) &&
            _lineService.IsCoordinateInSegmentBorders(x, y, b1, b2))
        {
            return IntersectionType.Inner;
        }

        return IntersectionType.Outside;
    }

    // Проверяет на типы пересечения Equal, Part, Extension, Contains, Overlay
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
        if ((_lineService.IsCoordinateInSegment(b1, a1, a2)
             || _lineService.IsCoordinateInSegment(b2, a1, a2))
            && (_lineService.IsCoordinateInSegment(a1, b1, b2)
                || _lineService.IsCoordinateInSegment(a2, b1, b2)))
        {
            return IntersectionType.Overlay;
        }

        return IntersectionType.Outside;
    }

    // Возвращает точку пересечения в out переменных
    private void GetIntersectionCoordinate(
        (double a, double b, double c) line1,
        (double a, double b, double c) line2,
        out double x,
        out double y,
        out bool isIntersects)
    {
        double delta = line1.a * line2.b - line2.a * line1.b;

        if (Math.Abs(delta) <= _epsilon)
        {
            isIntersects = false;
            x = 0;
            y = 0;
            return;
        }

        x = (line2.b * line1.c - line1.b * line2.c) / delta;
        y = (line1.a * line2.c - line2.a * line1.c) / delta;
        isIntersects = true;
    }
}