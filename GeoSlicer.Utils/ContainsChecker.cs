using System;
using System.Linq;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils;

public class ContainsChecker
{
    private readonly LineService _lineService;
    private readonly double _epsilon;

    public ContainsChecker(LineService lineService, double epsilon)
    {
        _lineService = lineService;
        _epsilon = epsilon;
    }

    /// <summary>
    /// Проверяет, находится ли точка внутри геометрии
    /// </summary>
    public bool IsPointInPolygon(Coordinate point, Polygon polygon, out bool isTangent)
    {
        return IsPointInLinearRing(point, polygon.Shell, out isTangent) &&
               polygon.Holes.All(ring => !IsPointInLinearRing(point, ring, out _));
    }

    /// <summary>
    /// Проверяет, находится ли точка внутри геометрии
    /// </summary>
    public bool IsPointInLinearRing(Coordinate point, LinearRing ring, out bool isTangent)
    {
        isTangent = false;
        // Если точка за пределами оболочки кольца, выходим сразу
        if (!IsPointInBorders(point, ring))
        {
            return false;
        }

        // Метод трассировки луча
        int count = 0;
        Coordinate[] coordinates = ring.Coordinates;
        int ringLen = coordinates.Length;
        for (int i = 0; i < ringLen - 1; i++)
        {
            if (_lineService.IsCoordinateInSegment(point, coordinates[(i + ringLen) % ringLen],
                    coordinates[(i + 1 + ringLen) % ringLen]))
            {
                isTangent = true;
                return true;
            }

            if (IsIntersectHorizontalRayWithSegment(point, coordinates[(i + ringLen) % ringLen],
                    coordinates[(i + 1 + ringLen) % ringLen]))
            {
                count++;
            }
        }

        return count % 2 != 0;

        bool IsIntersectHorizontalRayWithSegment(Coordinate p, Coordinate l1, Coordinate l2)
        {
            Coordinate maxCoord;
            Coordinate minCoord;

            if (l1.Y > l2.Y)
            {
                maxCoord = l1;
                minCoord = l2;
            }
            else
            {
                maxCoord = l2;
                minCoord = l1;
            }

            if (minCoord.Y >= p.Y + _epsilon || maxCoord.Y < p.Y
                || minCoord.X < p.X && maxCoord.X < p.X)
            {
                return false;
            }

            if (maxCoord.X > p.X && minCoord.X > p.X)
            {
                return true;
            }

            double product = LineService.VectorProduct(
                minCoord.X - p.X, minCoord.Y - p.Y, p.X - maxCoord.X, p.Y - maxCoord.Y);

            return product < 0;
        }
    }

    /// <summary>
    /// Проверяет, находится ли точка внутри оболочки отрезка
    /// </summary>
    public bool IsPointInBorders(Coordinate coordinate, Coordinate a, Coordinate b)
    {
        return IsPointInBorders(coordinate.X, coordinate.Y, a.X, a.Y, b.X, b.Y);
    }

    /// <summary>
    /// Проверяет, находится ли точка внутри оболочки геометрии
    /// </summary>
    public bool IsPointInBorders(Coordinate coordinate, Geometry geometry)
    {
        Envelope envelope = geometry.EnvelopeInternal;
        return IsPointInBorders(
            coordinate.X, coordinate.Y,
            envelope.MaxX, envelope.MinX,
            envelope.MaxY, envelope.MinY);
    }

    /// <summary>
    /// Проверяет, находится ли точка внутри оболочки отрезка
    /// </summary>
    public bool IsPointInBorders(double targetX, double targetY, double x1, double x2, double y1, double y2)
    {
        if (Math.Abs(x1 - x2) <= _epsilon)
        {
            return targetY >= Math.Min(y1, y2) - _epsilon &&
                   targetY <= Math.Max(y1, y2) + _epsilon;
        }

        return targetY >= Math.Min(y1, y2) - _epsilon
               && targetY <= Math.Max(y1, y2) + _epsilon
               && targetX >= Math.Min(x1, x2) - _epsilon
               && targetX <= Math.Max(x1, x2) + _epsilon;
    }
}