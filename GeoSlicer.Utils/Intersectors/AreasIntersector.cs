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
}