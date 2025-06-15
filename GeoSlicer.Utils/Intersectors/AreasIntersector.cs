using System;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.Intersectors;

/// <summary>
/// Проверяет, пересекаются ли прямоугольные оболочки.
/// Вызов перед методом определения пересечения может ускорить ваш код. 
/// </summary>
public class AreasIntersector
{
    public bool IsIntersects(Coordinate areaLine1Point1, Coordinate areaLine1Point2,
        Coordinate areaLine2Point1, Coordinate areaLine2Point2)
    {
        return !(Math.Min(areaLine1Point1.X, areaLine1Point2.X) > Math.Max(areaLine2Point1.X, areaLine2Point2.X))
               && !(Math.Max(areaLine1Point1.X, areaLine1Point2.X) < Math.Min(areaLine2Point1.X, areaLine2Point2.X))
               && !(Math.Max(areaLine1Point1.Y, areaLine1Point2.Y) < Math.Min(areaLine2Point1.Y, areaLine2Point2.Y))
               && !(Math.Min(areaLine1Point1.Y, areaLine1Point2.Y) > Math.Max(areaLine2Point1.Y, areaLine2Point2.Y));
    }
    
    public bool IsIntersects(double areaLine1Point1X, double areaLine1Point1Y,
        double areaLine1Point2X, double areaLine1Point2Y,
        double areaLine2Point1X, double areaLine2Point1Y,
        double areaLine2Point2X, double areaLine2Point2Y)
    {
        return !(Math.Min(areaLine1Point1X, areaLine1Point2X) > Math.Max(areaLine2Point1X, areaLine2Point2X))
               && !(Math.Max(areaLine1Point1X, areaLine1Point2X) < Math.Min(areaLine2Point1X, areaLine2Point2X))
               && !(Math.Max(areaLine1Point1Y, areaLine1Point2Y) < Math.Min(areaLine2Point1Y, areaLine2Point2Y))
               && !(Math.Min(areaLine1Point1Y, areaLine1Point2Y) > Math.Max(areaLine2Point1Y, areaLine2Point2Y));
    }
}