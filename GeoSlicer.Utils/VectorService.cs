using System;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils;

public static class VectorService
{
    
    /// <summary>
    /// Перемещает координату <paramref name="firstCoord"/> по биссектрисе угла, образуемого прямыми
    /// (<paramref name="coordAdjacentLine"/>, <paramref name="firstCoord"/>)
    /// и (<paramref name="firstCoord"/>, <paramref name="secondCoord"/>), на длину <paramref name="stepSize"/>.
    /// Возвращает массив из двух координат - сдвинутую координату внутрь угла и за угол, либо наоборот.
    /// С погрешностью <paramref name="tolerance"/> сравнивается угол между  прямыми
    /// (<paramref name="coordAdjacentLine"/>, <paramref name="firstCoord"/>) и
    /// (<paramref name="firstCoord"/>, <paramref name="secondCoord"/>)
    /// на равенство со 180°.
    /// </summary>
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
        double vectorLength = Math.Sqrt(x1 * x1 + y1 * y1);
        x1 /= vectorLength;
        y1 /= vectorLength;
    }

    public static double Cos(double x1, double y1, double x2, double y2)
    {
        return (x1 * x2 + y1 * y2) / (Math.Sqrt(x1 * x1 + y1 * y1) * Math.Sqrt(x2 * x2 + y2 * y2));
    }

    public static double Cos(Coordinate a, Coordinate b, Coordinate c)
    {
        double x1 = a.X - b.X;
        double y1 = a.Y - b.Y;
        double x2 = c.X - b.X;
        double y2 = c.Y - b.Y;
        return Cos(x1, y1, x2, y2);
    }
    
    public static double Cos(Coordinate aStart, Coordinate aEnd, Coordinate bStart, Coordinate bEnd)
    {
        double x1 = aStart.X - aEnd.X;
        double y1 = aStart.Y - aEnd.Y;
        double x2 = bStart.X - bEnd.X;
        double y2 = bStart.Y - bEnd.Y;
        return Cos(x1, y1, x2, y2);
    }
    
    public static bool AngleIsZeroOr180Degrees(Coordinate x, Coordinate y, Coordinate a, Coordinate b, double epsilon)
    {
        return Math.Abs(Math.Abs(Cos(x, y, a, b)) - 1) < epsilon;
    }
}