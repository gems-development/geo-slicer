using System;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.Intersectors;

/// <summary>
/// Определяет пересечение отрезков и исходящих из них прямых
/// </summary>
public class LinesIntersector
{
    private readonly double _epsilon;

    private readonly EpsilonCoordinateComparator _coordinateComparator;
    private readonly LineService _lineService;

    public LinesIntersector(EpsilonCoordinateComparator coordinateComparator, LineService lineService, double epsilon)
    {
        _coordinateComparator = coordinateComparator;
        _epsilon = epsilon;
        _lineService = lineService;
    }

    /// <summary>
    /// Проверяет, пересекаются ли отрезки с типом, соответствующим переданной маске
    /// </summary>
    public bool CheckIntersection(LinesIntersectionType requiredType, LineSegment a, LineSegment b)
    {
        return CheckIntersection(requiredType, a.P0, a.P1, b.P0, b.P1);
    }

    /// <summary>
    /// Проверяет, пересекаются ли отрезки с типом, соответствующим переданной маске
    /// </summary>
    public bool CheckIntersection(
        LinesIntersectionType requiredType,
        Coordinate a1, Coordinate a2,
        Coordinate b1, Coordinate b2)
    {
        LinesIntersectionType actualType =
            GetIntersection(a1, a2, b1, b2, out Coordinate? _, out double _, out double _, out bool _);
        return (actualType & requiredType) != 0;
    }

    /// <summary>
    /// Ищет пересечение отрезков
    /// </summary>
    /// <returns>Кортеж из типа пересечения и точки пересечения (null при параллельном пересечении)</returns>
    public (LinesIntersectionType, Coordinate?) GetIntersection(Coordinate a1, Coordinate a2, Coordinate b1,
        Coordinate b2)
    {
        LinesIntersectionType linesIntersectionType =
            GetIntersection(a1, a2, b1, b2,
                out Coordinate? resultCoordinate,
                out double x, out double y,
                out bool isIntersects);

        //  Прямые пересекаются, но в какой-то новой (не равной переданным) точке. Создаем ее
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

    /// <summary>
    /// Возвращает тип пересечения.
    /// Возвращает точку пересечения в out переменных. Если точка пересечения равна существующей,
    /// то возвращает Coordinate в <paramref name="result"/>,
    /// иначе <paramref name="x"/> и <paramref name="y"/> координаты точки
    /// </summary>
    private LinesIntersectionType GetIntersection(Coordinate a1, Coordinate a2, Coordinate b1, Coordinate b2,
        out Coordinate? result, out double x,
        out double y, out bool isIntersects)
    {
        result = null;

        // Преобразуем прямые, построенные на отрезках, в канонический вид
        LineService.ToCanonical(a1, a2,
            out double canonical1A, out double canonical1B, out double canonical1C);
        LineService.ToCanonical(b1, b2,
            out double canonical2A, out double canonical2B, out double canonical2C);

        // Получаем координаты пересечения прямых
        // IsIntersects = false <=> они параллельны
        GetIntersectionCoordinate(canonical1A, canonical1B, canonical1C,
            canonical2A, canonical2B, canonical2C,
            out x, out y, out isIntersects);

        // Прямые параллельны, проверка на Extension, Part, Equals, Contains, Overlay, NoIntersection
        if (!isIntersects)
        {
            // Проверка, есть ли между прямыми расстояние
            if (_lineService.IsCoordinateAtLine(a1, b1, b2) &&
                _lineService.IsCoordinateAtLine(a2, b1, b2))
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
            _coordinateComparator.IsEquals(checkable, x, y) &&
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

        // Проверка на Inner
        // Если пересечение внутри одного отрезка, то оно и внутри другого
        if (_lineService.IsCoordinateInSegmentBorders(x, y, a1, a2) &&
            _lineService.IsCoordinateInSegmentBorders(x, y, b1, b2))
        {
            return LinesIntersectionType.Inner;
        }

        return LinesIntersectionType.Outside;
    }

    /// <summary>
    /// Проверяет на типы пересечения Equal, Part, Extension, Contains, Overlay
    /// </summary>
    private LinesIntersectionType GetParallelIntersection(
        Coordinate a1, Coordinate a2,
        Coordinate b1, Coordinate b2,
        ref Coordinate? resultCoordinate)
    {
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

        LinesIntersectionType? CheckEnds(Coordinate x1, Coordinate x2, Coordinate y1, Coordinate y2,
            ref Coordinate? resultInnerCoordinate)
        {
            if (!_coordinateComparator.IsEquals(x1, y1)) return null;
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
    }

    /// <summary>
    /// Проверяет пересечение прямых.
    /// Если прямые пересекаются, возвращает точку пересечения в out переменных.
    /// Возвращает в <paramref name="isIntersects"/> флаг, пересекаются ли прямые.
    /// </summary>
    private void GetIntersectionCoordinate(
        double a1, double b1, double c1,
        double a2, double b2, double c2,
        out double x,
        out double y,
        out bool isIntersects)
    {
        double delta = a1 * b2 - a2 * b1;

        // Прямые параллельны, найти точку пересечения невозможно
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