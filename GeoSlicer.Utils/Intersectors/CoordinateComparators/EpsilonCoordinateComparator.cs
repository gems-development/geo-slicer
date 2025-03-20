using System;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.Intersectors.CoordinateComparators;

/// <summary>
/// Проверяет координаты на равенство,
/// считая координаты равными, если Манхеттенское расстояние между ними меньше espilon
/// </summary>
public class EpsilonCoordinateComparator : ICoordinateComparator
{
    private readonly double _epsilon;

    public EpsilonCoordinateComparator(double epsilon = 0)
    {
        _epsilon = epsilon;
    }

    public bool IsEquals(Coordinate a, Coordinate b)
    {
        if (_epsilon == 0)
        {
            return a.Equals2D(b);
        }

        return Math.Abs(a.X - b.X) <= _epsilon && Math.Abs(a.Y - b.Y) <= _epsilon;
    }

    public bool IsEquals(Coordinate a, double x, double y)
    {
        return Math.Abs(a.X - x) <= _epsilon && Math.Abs(a.Y - y) <= _epsilon;
    }
    
    public bool IsEquals(double a, double b)
    {
        return Math.Abs(a - b) <= _epsilon;
    }
}