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
            GetIntersection(a1, a2, b1, b2, out Coordinate? _, out double _, out double _, out bool _);
        return (actualType & requiredType) != 0;
    }

    public (IntersectionType, Coordinate?) GetIntersection(Coordinate a1, Coordinate a2, Coordinate b1, Coordinate b2)
    {
        IntersectionType intersectionType =
            GetIntersection(a1, a2, b1, b2,
                out Coordinate? resultCoordinate,
                out double x, out double y,
                out bool isIntersects);

        if (resultCoordinate is null && isIntersects)
        {
            resultCoordinate = new Coordinate(x, y);
        }

        return (intersectionType, resultCoordinate);
    }

    public (IntersectionType, Coordinate?) GetIntersection(LineSegment a, LineSegment b)
    {
        return GetIntersection(a.P0, a.P1, b.P0, b.P1);
    }

    // Возвращает тип пересечения.
    // Возвращает точку пересечения в out переменных. Если точка пересечения равна существующей, то возвращает Coordinate
    // в result, иначе x и y координаты точки
    private IntersectionType GetIntersection(Coordinate a1, Coordinate a2, Coordinate b1, Coordinate b2,
        out Coordinate? result, out double x,
        out double y, out bool isIntersects)
    {
        result = null;

        (double a, double b, double c) canonicalA = LineService.ToCanonical(a1, a2);
        (double a, double b, double c) canonicalB = LineService.ToCanonical(b1, b2);
        GetIntersectionCoordinate(canonicalA, canonicalB, out x, out y, out isIntersects);

        // Прямые параллельны, проверка на Extension, Part, Equals, Contains, Overlay, NoIntersection
        if (!isIntersects)
        {
            // Проверка, есть ли между прямыми расстояние
            if (_lineService.IsLineEquals(canonicalA, canonicalB))
            {
                return GetParallelIntersection(a1, a2, b1, b2, ref result);
            }

            return IntersectionType.NoIntersection;
        }

        // Есть точка пересечения, проверка на Corner, TyShaped, Inner, Outside

        // Проверка на Corner
        if (_coordinateComparator.IsEquals(a1, b1) ||
            _coordinateComparator.IsEquals(a1, b2) ||
            _coordinateComparator.IsEquals(a2, b1) ||
            _coordinateComparator.IsEquals(a2, b2))
        {
            if (_coordinateComparator.IsEquals(a1, b1) || _coordinateComparator.IsEquals(a1, b2))
            {
                result = a1;
            }
            else
            {
                result = a2;
            }

            return IntersectionType.Corner;
        }

        // Проверка на TyShaped
        bool CheckTyShaped(Coordinate a, Coordinate b, Coordinate checkable, double x, double y) =>
            _epsilonCoordinateComparator.IsEquals(checkable, x, y) &&
            _lineService.IsCoordinateInSegmentBorders(x, y, a, b);

        if (CheckTyShaped(a1, a2, b1, x, y))
        {
            result = b1;
            return IntersectionType.TyShaped;
        }

        if (CheckTyShaped(a1, a2, b2, x, y))
        {
            result = b2;
            return IntersectionType.TyShaped;
        }

        if (CheckTyShaped(b1, b2, a1, x, y))
        {
            result = a1;
            return IntersectionType.TyShaped;
        }

        if (CheckTyShaped(b1, b2, a2, x, y))
        {
            result = a2;
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
        Coordinate b1, Coordinate b2,
        ref Coordinate? resultCoordinate)
    {
        IntersectionType? CheckEnds(Coordinate x1, Coordinate x2, Coordinate y1, Coordinate y2,
            ref Coordinate? resultInnerCoordinate)
        {
            if (_coordinateComparator.IsEquals(x1, y1))
            {
                if (_coordinateComparator.IsEquals(x2, y2))
                {
                    return IntersectionType.Equals;
                }

                if (_lineService.IsCoordinateInSegmentBorders(x2, y1, y2) ||
                    _lineService.IsCoordinateInSegmentBorders(y2, x1, x2))
                {
                    return IntersectionType.Part;
                }

                resultInnerCoordinate = x1;
                return IntersectionType.Extension;
            }

            return null;
        }

        // Проверка на Extension, Part, Equals
        IntersectionType? result = CheckEnds(a1, a2, b1, b2, ref resultCoordinate);
        if (result is not null)
            return (IntersectionType)result;
        result = CheckEnds(a1, a2, b2, b1, ref resultCoordinate);
        if (result is not null)
            return (IntersectionType)result;
        result = CheckEnds(a2, a1, b1, b2, ref resultCoordinate);
        if (result is not null)
            return (IntersectionType)result;
        result = CheckEnds(a2, a1, b2, b1, ref resultCoordinate);
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