using System;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils;

public static class VectorService
{
    private static double? CalculatePhiFromZeroTo2Pi(double x, double y)
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

    private static double? CalculatePhiFromMinusPiToPlusPi(double x, double y)
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

    public static bool InsideTheAngle(
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
        return phiA < phiB2;
    }
    
    //Перемещает координату firstCoord по биссектрисе угла, образуемого прямыми (coordAdjacentLine, firstCoord)
    //и (firstCoord, secondCoord), на длинну stepSize.
    //Возвращает массив из двух координат - сдвинутую координату внутрь угла и за угол, либо наоборот.
    //С погрешностью tolerance сравнивается угол между  прямыми (coordAdjacentLine, firstCoord) и (firstCoord, secondCoord)
    //на равенство со 180°.
    public static Coordinate[] ShiftPointAlongBisector(
        Coordinate coordAdjacentLine,
        Coordinate firstCoord, 
        Coordinate secondCoord,
        double stepSize,
        double tolerance)
    {
        double x1 = firstCoord.X - coordAdjacentLine.X;
        double y1 = firstCoord.Y - coordAdjacentLine.Y;
        double x2 = firstCoord.X - secondCoord.X;
        double y2 = firstCoord.Y - secondCoord.Y;
        
        double shearVectorX;
        double shearVectorY;
        
        if (Math.Abs(Cos(x1, y1, x2, y2) + 1) < tolerance)
        {
            RotateVector90Degrees(x1, y1, out shearVectorX, out shearVectorY);
        }
        else
        {
            OrthonormalizeVector(ref x1, ref y1);
            OrthonormalizeVector(ref x2, ref y2);
            shearVectorX = x1 + x2;
            shearVectorY = y1 + y2;
        }
        
        OrthonormalizeVector(ref shearVectorX, ref shearVectorY);
        shearVectorX *= stepSize;
        shearVectorY *= stepSize;

        Coordinate[] res = new Coordinate[2];
        res[0] = new Coordinate(firstCoord.X + shearVectorX, firstCoord.Y + shearVectorY);
        res[1] = new Coordinate(firstCoord.X - shearVectorX, firstCoord.Y - shearVectorY);
        return res;
    }

    public static void RotateVector90Degrees(double x1, double y1, out double x2, out double y2)
    {
        x2 = -y1;
        y2 = x1;
    }

    public static void OrthonormalizeVector(ref double x1, ref double y1)
    {
        x1 *= 10;
        y1 *= 10;
        double vectorLength = Math.Sqrt(x1 * x1 + y1 * y1);
        x1 /= vectorLength;
        y1 /= vectorLength;
    }

    public static double Cos(double x1, double y1, double x2, double y2)
    {
        x1 *= 10;
        y1 *= 10;
        x2 *= 10;
        y2 *= 10;
        return (x1 * x2 + y1 * y2) / (Math.Sqrt(x1 * x1 + y1 * y1) * Math.Sqrt(x2 * x2 + y2 * y2));
    }
}