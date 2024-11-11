using System;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils;

public class LineService
{
    private readonly double _epsilon;


    public LineService(double epsilon)
    {
        _epsilon = epsilon;
    }

    public static double VectorProduct(Coordinate firstVec, Coordinate secondVec)
    {
        return firstVec.X * secondVec.Y - secondVec.X * firstVec.Y;
    }

    public static  double VectorProduct(Coordinate firstVecPoint1, Coordinate firstVecPoint2, Coordinate secondVecPoint1,
        Coordinate secondVecPoint2)
    {
        return (firstVecPoint2.X - firstVecPoint1.X) * (secondVecPoint2.Y - secondVecPoint1.Y) -
               (secondVecPoint2.X - secondVecPoint1.X) * (firstVecPoint2.Y - firstVecPoint1.Y);
    }

    public static  double VectorProduct(Coordinate firstVecPoint1, Coordinate firstVecPoint2, Coordinate secondVecPoint1,
        double secondVecPoint2X, double secondVecPoint2Y)
    {
        return (firstVecPoint2.X - firstVecPoint1.X) * (secondVecPoint2Y - secondVecPoint1.Y) -
               (secondVecPoint2X - secondVecPoint1.X) * (firstVecPoint2.Y - firstVecPoint1.Y);
    }

    public static  double VectorProduct(double firstVecX, double firstVecY, double secondVecX, double secondVecY)
    {
        return firstVecX * secondVecY - secondVecX * firstVecY;
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
        return Math.Abs(VectorProduct(coordinate.X - first.X, coordinate.Y - first.Y,
            second.X - coordinate.X, second.Y - coordinate.Y)) < _epsilon;
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
        if (Math.Abs(first.Y - second.Y) <= _epsilon)
        {
            return x >= Math.Min(first.X, second.X) - _epsilon &&
                   x <= Math.Max(first.X, second.X) + _epsilon;
        }

        return y >= Math.Min(first.Y, second.Y) - _epsilon
               && y <= Math.Max(first.Y, second.Y) + _epsilon
               && x >= Math.Min(first.X, second.X) - _epsilon
               && x <= Math.Max(first.X, second.X) + _epsilon;
    }
    
    public bool IsCoordinateInIntervalBorders(Coordinate coordinate, Coordinate first, Coordinate second)
    {
        ICoordinateComparator coordinateComparator = new EpsilonCoordinateComparator();

        if (coordinateComparator.IsEquals(coordinate, first) || coordinateComparator.IsEquals(coordinate, second))
        {
            return false;
        }

        return IsCoordinateInSegmentBorders(coordinate.X, coordinate.Y, first, second);
    }


    public bool IsLineEquals(double a1, double b1, double c1, double a2, double b2, double c2)
    {
        bool res = Math.Abs(a1 * b2 - b1 * a2) <= _epsilon
                   && Math.Abs(a1 * c2 - c1 * a2) <= _epsilon
                   && Math.Abs(b1 * c2 - c1 * b2) <= _epsilon;
        return res;
    }

    public bool IsRectangleOnOneSideOfLine(Coordinate linePoint1, Coordinate linePoint2, Coordinate currentPoint1,
        Coordinate currentPoint2)
    {
        //Проверить, что все VectorProduct имеют один знак
        return VectorProduct(linePoint1, linePoint2, linePoint2, currentPoint1) < 0
               && VectorProduct(linePoint1, linePoint2, linePoint2, currentPoint2) < 0
               && VectorProduct(linePoint1, linePoint2, linePoint2, currentPoint1.X, currentPoint2.Y) < 0
               && VectorProduct(linePoint1, linePoint2, linePoint2, currentPoint2.X, currentPoint1.Y) < 0
               || VectorProduct(linePoint1, linePoint2, linePoint2, currentPoint1) > 0
               && VectorProduct(linePoint1, linePoint2, linePoint2, currentPoint2) > 0
               && VectorProduct(linePoint1, linePoint2, linePoint2, currentPoint1.X, currentPoint2.Y) > 0
               && VectorProduct(linePoint1, linePoint2, linePoint2, currentPoint2.X, currentPoint1.Y) > 0;
    }
    
    private double? CalculatePhiFromZeroTo2Pi(double x, double y)
    {
        return x switch
        {
            > 0 when y >= 0 => Math.Atan(y / x),
            > 0 when y < 0 => Math.Atan(y / x) + 2 * Math.PI,
            < 0 => Math.Atan(y / x) + Math.PI,
            0 when y > 0 => Math.PI / 2,
            0 when y < 0 => 3 * Math.PI / 2,
            0 when y == 0 => null,
            _ => null
        };
    }

    private double? CalculatePhiFromMinusPiToPlusPi(double x, double y)
    {
        return x switch
        {
            > 0 => Math.Atan(y / x),
            < 0 when y >= 0 => Math.Atan(y / x) + Math.PI,
            < 0 when y < 0 => Math.Atan(y / x) - Math.PI,
            0 when y > 0 => Math.PI / 2,
            0 when y < 0 => -Math.PI / 2,
            0 when y == 0 => null,
            _ => null
        };
    }

    public bool InsideTheAngle(
        Coordinate vectorPointA1,
        Coordinate vectorPointA2,
        Coordinate anglePointB1,
        Coordinate anglePointB2,
        Coordinate anglePointB3)
    {
        var vectorB1 = new Coordinate(anglePointB3.X - anglePointB2.X,
            anglePointB3.Y - anglePointB2.Y);
        var phiB1 = CalculatePhiFromMinusPiToPlusPi(vectorB1.X, vectorB1.Y);
        if (phiB1 == null) return true;
        const int sign = -1;
        var rotatedVectorAx = (vectorPointA2.X - vectorPointA1.X) * Math.Cos(sign * (double)phiB1) -
                              (vectorPointA2.Y - vectorPointA1.Y) * Math.Sin(sign * (double)phiB1);
        var rotatedVectorAy = (vectorPointA2.X - vectorPointA1.X) * Math.Sin(sign * (double)phiB1) +
                              (vectorPointA2.Y - vectorPointA1.Y) * Math.Cos(sign * (double)phiB1);
        var phiA = CalculatePhiFromZeroTo2Pi(rotatedVectorAx, rotatedVectorAy);
        var rotatedVectorB2X = (anglePointB1.X - anglePointB2.X) * Math.Cos(sign * (double)phiB1) -
                               (anglePointB1.Y - anglePointB2.Y) * Math.Sin(sign * (double)phiB1);
        var rotatedVectorB2Y = (anglePointB1.X - anglePointB2.X) * Math.Sin(sign * (double)phiB1) +
                               (anglePointB1.Y - anglePointB2.Y) * Math.Cos(sign * (double)phiB1);
        var phiB2 = CalculatePhiFromZeroTo2Pi(rotatedVectorB2X, rotatedVectorB2Y);
        return phiA <= phiB2 + _epsilon;
    }
    
    public bool InsideTheAngleWithoutBorders(
        Coordinate vectorPointA1,
        Coordinate vectorPointA2,
        Coordinate anglePointB1,
        Coordinate anglePointB2,
        Coordinate anglePointB3)
    {
        var vectorB1 = new Coordinate(anglePointB3.X - anglePointB2.X,
            anglePointB3.Y - anglePointB2.Y);
        var phiB1 = CalculatePhiFromMinusPiToPlusPi(vectorB1.X, vectorB1.Y);
        if (phiB1 == null) return true;
        const int sign = -1;
        var rotatedVectorAx = (vectorPointA2.X - vectorPointA1.X) * Math.Cos(sign * (double)phiB1) -
                              (vectorPointA2.Y - vectorPointA1.Y) * Math.Sin(sign * (double)phiB1);
        var rotatedVectorAy = (vectorPointA2.X - vectorPointA1.X) * Math.Sin(sign * (double)phiB1) +
                              (vectorPointA2.Y - vectorPointA1.Y) * Math.Cos(sign * (double)phiB1);
        var phiA = CalculatePhiFromZeroTo2Pi(rotatedVectorAx, rotatedVectorAy);
        var rotatedVectorB2X = (anglePointB1.X - anglePointB2.X) * Math.Cos(sign * (double)phiB1) -
                               (anglePointB1.Y - anglePointB2.Y) * Math.Sin(sign * (double)phiB1);
        var rotatedVectorB2Y = (anglePointB1.X - anglePointB2.X) * Math.Sin(sign * (double)phiB1) +
                               (anglePointB1.Y - anglePointB2.Y) * Math.Cos(sign * (double)phiB1);
        var phiB2 = CalculatePhiFromZeroTo2Pi(rotatedVectorB2X, rotatedVectorB2Y);
        return phiA > 0 + _epsilon && phiA < phiB2 - _epsilon;
    }
}