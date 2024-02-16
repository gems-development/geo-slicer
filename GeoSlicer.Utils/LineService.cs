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

    public static (double a, double b, double c) ToCanonical(Coordinate first, Coordinate second)
    {
        double a = second.Y - first.Y;
        double b = first.X - second.X;
        double c = a * first.X + b * first.Y;
        return (a, b, c);
    }

    public bool IsCoordinateInSegment(Coordinate coordinate, Coordinate first, Coordinate second)
    {
        return IsCoordinateInSegmentBorders(coordinate, first, second) && IsCoordinateAtLine(coordinate, first, second);
    }

    public bool IsCoordinateAtLine(Coordinate coordinate, Coordinate first, Coordinate second)
    {
        if (coordinate.Equals2D(new Coordinate(66.09066550, 57.13496720)))
        {
            int i = 0;
        }
        return Math.Abs(VectorProduct(new Coordinate(coordinate.X - first.X, coordinate.Y - first.Y),
            new Coordinate(second.X - coordinate.X, second.Y - coordinate.Y))) < _epsilon;
    }


    public bool IsCoordinateInSegmentBorders(Coordinate coordinate, Coordinate first, Coordinate second)
    {
        if (Math.Abs(first.X - second.X) <= _epsilon)
        {
            return coordinate.Y > Math.Min(first.Y, second.Y) && coordinate.Y < Math.Max(first.Y, second.Y);
        }

        return coordinate.Y > Math.Min(first.Y, second.Y)
               && coordinate.Y < Math.Max(first.Y, second.Y) 
               && coordinate.X > Math.Min(first.X, second.X)
               && coordinate.X < Math.Max(first.X, second.X);
    }

    public bool IsLineEquals((double a, double b, double c) line1, (double a, double b, double c) line2)
    {
        bool res = Math.Abs(line1.a * line2.b - line1.b * line2.a) <= _epsilon
               && Math.Abs(line1.a * line2.c - line1.c * line2.a) <= _epsilon
               && Math.Abs(line1.b * line2.c - line1.c * line2.b) <= _epsilon;
        return res;
    }
}