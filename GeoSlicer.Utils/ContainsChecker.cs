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

    public bool IsPointInPolygon(Coordinate point, Polygon polygon)
    {
        return IsPointInLinearRing(point, polygon.Shell) &&
               polygon.Holes.All(ring => !IsPointInLinearRing(point, ring));
    }

    public bool IsPointInLinearRing(Coordinate point, LinearRing ring)
    {
        if (!IsPointInBorders(point, ring))
        {
            return false;
        }
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

            Coordinate vec1 = new Coordinate(minCoord.X - p.X, minCoord.Y - p.Y);
            Coordinate vec2 = new Coordinate(p.X - maxCoord.X, p.Y - maxCoord.Y);

            double product = _lineService.VectorProduct(vec1, vec2);

            return minCoord.Y < p.Y - _epsilon && maxCoord.Y >= p.Y && product < 0;
        }
        // Метод трассировки луча
        int count = 0;
        for (int i = 0; i < ring.Count - 1; i++)
        {
            if (_lineService.IsCoordinateInSegment(point, ring[(i + ring.Count) % ring.Count],
                    ring[(i + 1 + ring.Count) % ring.Count]))
            {
                count = 1;
                break;
            }

            if (IsIntersectHorizontalRayWithSegment(point, ring[(i + ring.Count) % ring.Count],
                    ring[(i + 1 + ring.Count) % ring.Count]))
            {
                count++;
            }
        }

        return count % 2 != 0;
    }
    
    

    public bool IsPointInBorders(Coordinate coordinate, Coordinate a, Coordinate b)
    {
        return IsPointInBorders(coordinate.X, coordinate.Y, a.X, a.Y, b.X, b.Y);
    }
    
    public bool IsPointInBorders(Coordinate coordinate, Geometry geometry)
    {
        Envelope envelope = geometry.EnvelopeInternal;
        return IsPointInBorders(
            coordinate.X, coordinate.Y,
            envelope.MaxX, envelope.MinX,
            envelope.MaxY, envelope.MinY);
    }

    public bool IsPointInBorders(double targetX, double targetY, double x1, double x2, double y1, double y2)
    {
        // todo Разобраться с дублированием кода (мб внедрить в LineService, а VectorProduct вынести как static
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