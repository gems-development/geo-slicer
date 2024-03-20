using System;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils;

public class Normalizer
{
    private double _xShift;
    private double _yShift;

    public void Fit(Coordinate[] coordinates)
    {
        double minX = Double.MinValue;
        double minY = Double.MinValue;
        double maxX = Double.MaxValue;
        double maxY = Double.MaxValue;

        foreach (Coordinate coordinate in coordinates)
        {
            minX = Math.Min(coordinate.X, minX);
            minY = Math.Min(coordinate.X, minY);
            maxX = Math.Max(coordinate.X, maxX);
            maxY = Math.Max(coordinate.X, maxY);
        }

        _xShift = maxX / 2 + minX / 2;
        _yShift = maxY / 2 + minY / 2;
    }

    public void Shift(Coordinate geometry, bool isBack = false)
    {
        geometry.X -= _xShift * (isBack ? -1 : 1);
        geometry.Y -= _yShift * (isBack ? -1 : 1);
    }

    public void Shift(CoordinateSequence geometry, bool isBack = false)
    {
        // Получение по индексу возвращает копию, поэтому невозможно вызвать для каждой координаты Shift(Coorditane)
        // Используем методы, которые, надеюсь, написаны правильно и не создают копий
        for (int i = 0; i < geometry.Count; i++)
        {
            geometry.SetX(i, geometry.GetX(i) - _xShift * (isBack ? -1 : 1));
            geometry.SetY(i, geometry.GetY(i) - _yShift * (isBack ? -1 : 1));
        }
    }

    public void Shift(LineString geometry, bool isBack = false)
    {
        Shift(geometry.CoordinateSequence, isBack);
    }

    public void Shift(LinearRing geometry, bool isBack = false)
    {
        Shift(geometry.CoordinateSequence, isBack);
    }

    public void Shift(MultiLineString geometry, bool isBack = false)
    {
        foreach (var lineString in geometry)
        {
            Shift((LineString)lineString, isBack);
        }
    }

    public void Shift(Polygon geometry, bool isBack = false)
    {
        Shift(geometry.Shell, isBack);
        foreach (var hole in geometry.Holes)
        {
            Shift(hole, isBack);
        }
    }
    
    public void Shift(MultiPolygon geometry, bool isBack = false)
    {
        foreach (var polygon in geometry)
        {
            Shift((Polygon)polygon, isBack);
        }
    }
    
    
}