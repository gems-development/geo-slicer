using System;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils;

public static class VectorService
{
    //Перемещает координату firstCoord по биссектрисе угла, образуемого прямыми (coordAdjacentLine, firstCoord)
    //и (firstCoord, secondCoord), на длинну stepSize.
    //Направление перемещения не определено(например метод может сдвинуть точку не внутрь угла).
    //Чтобы переместить в другом направлении, поменяйте флаг direction на false.
    //С погрешностью tolerance сравнивается угол между  прямыми (coordAdjacentLine, firstCoord) и (firstCoord, secondCoord)
    //на равенство со 180°.
    public static Coordinate ShiftPointAlongBisector(
        Coordinate coordAdjacentLine,
        Coordinate firstCoord, 
        Coordinate secondCoord,
        double stepSize,
        double tolerance, 
        bool direction = true)
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
        if (!direction)
        {
            shearVectorX *= -1;
            shearVectorY *= -1;
        }

        return new Coordinate(firstCoord.X + shearVectorX, firstCoord.Y+ shearVectorY);
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