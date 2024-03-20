using System;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.Intersectors.IntersectionTypes;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.Intersectors;

public class LinesIntersector
{
    private readonly double _epsilon;

    private readonly ICoordinateComparator _coordinateComparator;
    private readonly EpsilonCoordinateComparator _epsilonCoordinateComparator;
    private readonly LineService _lineService;

    public LinesIntersector(ICoordinateComparator coordinateComparator, LineService lineService, double epsilon)
    {
        _coordinateComparator = coordinateComparator;
        _epsilonCoordinateComparator = new EpsilonCoordinateComparator(epsilon);
        _epsilon = epsilon;
        _lineService = lineService;
    }

    public bool CheckIntersection(LinesIntersectionType requiredType, LineSegment a, LineSegment b)
    {
        return CheckIntersection(requiredType, a.P0, a.P1, b.P0, b.P1);
    }

    public bool CheckIntersection(
        LinesIntersectionType requiredType,
        Coordinate a1, Coordinate a2,
        Coordinate b1, Coordinate b2)
    {
        LinesIntersectionType actualType =
            GetIntersection(a1, a2, b1, b2, out Coordinate? _, out double _, out double _, out bool _);
        return (actualType & requiredType) != 0;
    }

    public (LinesIntersectionType, Coordinate?) GetIntersection(Coordinate a1, Coordinate a2, Coordinate b1, Coordinate b2)
    {
        LinesIntersectionType linesIntersectionType =
            GetIntersection(a1, a2, b1, b2,
                out Coordinate? resultCoordinate,
                out double x, out double y,
                out bool isIntersects);

        if (resultCoordinate is null && isIntersects)
        {
            resultCoordinate = new Coordinate(x, y);
        }

        return (linesIntersectionType, resultCoordinate);
    }

    public (LinesIntersectionType, Coordinate?) GetIntersection(LineSegment a, LineSegment b)
    {
        return GetIntersection(a.P0, a.P1, b.P0, b.P1);
    }

    // Возвращает тип пересечения.
    // Возвращает точку пересечения в out переменных. Если точка пересечения равна существующей, то возвращает Coordinate
    // в result, иначе x и y координаты точки
    private LinesIntersectionType GetIntersection(Coordinate a1, Coordinate a2, Coordinate b1, Coordinate b2,
        out Coordinate? result, out double x,
        out double y, out bool isIntersects)
    {
        result = null;
        
        LineService.ToCanonical(a1, a2, 
            out double canonical1A, out double canonical1B, out double canonical1C);
        LineService.ToCanonical(b1, b2, 
            out double canonical2A, out double canonical2B, out double canonical2C);
        GetIntersectionCoordinate(canonical1A, canonical1B, canonical1C,
            canonical2A, canonical2B, canonical2C,
            out x, out y, out isIntersects);

        // Прямые параллельны, проверка на Extension, Part, Equals, Contains, Overlay, NoIntersection
        if (!isIntersects)
        {
            // Проверка, есть ли между прямыми расстояние
            if (_lineService.IsLineEquals(canonical1A, canonical1B, canonical1C,
                    canonical2A, canonical2B, canonical2C))
            {
                return GetParallelIntersection(a1, a2, b1, b2, ref result);
            }

            return LinesIntersectionType.NoIntersection;
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

            return LinesIntersectionType.Corner;
        }

        // Проверка на TyShaped
        bool CheckTyShaped(Coordinate a, Coordinate b, Coordinate checkable, double x, double y) =>
            _epsilonCoordinateComparator.IsEquals(checkable, x, y) &&
            _lineService.IsCoordinateInSegmentBorders(x, y, a, b);

        if (CheckTyShaped(a1, a2, b1, x, y))
        {
            result = b1;
            return LinesIntersectionType.TyShaped;
        }

        if (CheckTyShaped(a1, a2, b2, x, y))
        {
            result = b2;
            return LinesIntersectionType.TyShaped;
        }

        if (CheckTyShaped(b1, b2, a1, x, y))
        {
            result = a1;
            return LinesIntersectionType.TyShaped;
        }

        if (CheckTyShaped(b1, b2, a2, x, y))
        {
            result = a2;
            return LinesIntersectionType.TyShaped;
        }

        // Если пересечение внутри одного отрезка, то оно и внутри другого
        if (_lineService.IsCoordinateInSegmentBorders(x, y, a1, a2) &&
            _lineService.IsCoordinateInSegmentBorders(x, y, b1, b2))
        {
            return LinesIntersectionType.Inner;
        }

        return LinesIntersectionType.Outside;
    }

    // Проверяет на типы пересечения Equal, Part, Extension, Contains, Overlay
    private LinesIntersectionType GetParallelIntersection(
        Coordinate a1, Coordinate a2,
        Coordinate b1, Coordinate b2,
        ref Coordinate? resultCoordinate)
    {
        LinesIntersectionType? CheckEnds(Coordinate x1, Coordinate x2, Coordinate y1, Coordinate y2,
            ref Coordinate? resultInnerCoordinate)
        {
            if (_coordinateComparator.IsEquals(x1, y1))
            {
                if (_coordinateComparator.IsEquals(x2, y2))
                {
                    return LinesIntersectionType.Equals;
                }

                if (_lineService.IsCoordinateInSegmentBorders(x2, y1, y2) ||
                    _lineService.IsCoordinateInSegmentBorders(y2, x1, x2))
                {
                    return LinesIntersectionType.Part;
                }

                resultInnerCoordinate = x1;
                return LinesIntersectionType.Extension;
            }

            return null;
        }

        // Проверка на Extension, Part, Equals
        LinesIntersectionType? result = CheckEnds(a1, a2, b1, b2, ref resultCoordinate);
        if (result is not null)
            return (LinesIntersectionType)result;
        result = CheckEnds(a1, a2, b2, b1, ref resultCoordinate);
        if (result is not null)
            return (LinesIntersectionType)result;
        result = CheckEnds(a2, a1, b1, b2, ref resultCoordinate);
        if (result is not null)
            return (LinesIntersectionType)result;
        result = CheckEnds(a2, a1, b2, b1, ref resultCoordinate);
        if (result is not null)
            return (LinesIntersectionType)result;

        // Проверка на Contains
        if (_lineService.IsCoordinateInSegmentBorders(b1, a1, a2)
            && _lineService.IsCoordinateInSegmentBorders(b2, a1, a2)
            || _lineService.IsCoordinateInSegmentBorders(a1, b1, b2)
            && _lineService.IsCoordinateInSegmentBorders(a2, b1, b2))
        {
            return LinesIntersectionType.Contains;
        }

        //Проверка на Overlay
        if ((_lineService.IsCoordinateInSegment(b1, a1, a2)
             || _lineService.IsCoordinateInSegment(b2, a1, a2))
            && (_lineService.IsCoordinateInSegment(a1, b1, b2)
                || _lineService.IsCoordinateInSegment(a2, b1, b2)))
        {
            return LinesIntersectionType.Overlay;
        }

        return LinesIntersectionType.Outside;
    }

    // Возвращает точку пересечения в out переменных
    private void GetIntersectionCoordinate(
        double a1, double b1, double c1,
        double a2, double b2, double c2,
        out double x,
        out double y,
        out bool isIntersects)
    {
        double delta = a1 * b2 - a2 * b1;

        if (Math.Abs(delta) <= _epsilon)
        {
            isIntersects = false;
            x = 0;
            y = 0;
            return;
        }

        x = (b2 * c1 - b1 * c2) / delta;
        y = (a1 * c2 - a2 * c1) / delta;
        isIntersects = true;
    }
}