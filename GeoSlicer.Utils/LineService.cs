using System;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils;

public class LineService
{
    private readonly double _epsilon;


    public LineService(double epsilon)
    {
        _epsilon = epsilon;
    }

    public double VectorProduct
    (Coordinate firstVec, Coordinate secondVec)
    {
        double product = firstVec.X * secondVec.Y - secondVec.X * firstVec.Y;
        return product;
    }

    public static void ToCanonical(Coordinate first, Coordinate second,
        out double a, out double b, out double c)
    {
        a = second.Y - first.Y;
        b = first.X - second.X;
        c = a * first.X + b * first.Y;
    }

    public bool IsCoordinateInSegment(Coordinate coordinate, Coordinate first, Coordinate second)
    {
        return IsCoordinateInSegmentBorders(coordinate, first, second) && IsCoordinateAtLine(coordinate, first, second);
    }

    public bool IsCoordinateAtLine(Coordinate coordinate, Coordinate first, Coordinate second)
    {
        return Math.Abs(VectorProduct(new Coordinate(coordinate.X - first.X, coordinate.Y - first.Y),
            new Coordinate(second.X - coordinate.X, second.Y - coordinate.Y))) < _epsilon;
    }


    public bool IsCoordinateInSegmentBorders(Coordinate coordinate, Coordinate first, Coordinate second)
    {
        return IsCoordinateInSegmentBorders(coordinate.X, coordinate.Y, first, second);
    }

    public bool IsCoordinateInSegmentBorders(double x, double y, Coordinate first, Coordinate second)
    {
        if (Math.Abs(first.X - second.X) <= _epsilon)
        {
            return y >= Math.Min(first.Y, second.Y) - _epsilon &&
                   y <= Math.Max(first.Y, second.Y) + _epsilon;
        }

        return y >= Math.Min(first.Y, second.Y) - _epsilon
               && y <= Math.Max(first.Y, second.Y) + _epsilon
               && x >= Math.Min(first.X, second.X) - _epsilon
               && x <= Math.Max(first.X, second.X) + _epsilon;
    }


    public bool IsLineEquals(double a1, double b1, double c1, double a2, double b2, double c2)
    {
        bool res = Math.Abs(a1 * b2 - b1 * a2) <= _epsilon
               && Math.Abs(a1 * c2 - c1 * a2) <= _epsilon
               && Math.Abs(b1 * c2 - c1 * b2) <= _epsilon;
        return res;
    }
}