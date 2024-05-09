using System;
using GeoSlicer.Utils;
using NetTopologySuite.Geometries;

namespace GeoSlicer.DivideAndRuleSlicers.OppositesSlicer;

public static class Utils
{
    public static int GetNearestOpposites(LinearRing ring)
    {
        Coordinate[] coordinates = ring.Coordinates;
        int halfOfLen = coordinates.Length / 2;
        double minDistance = Double.MaxValue;
        int minDistanceIndex = -1;
        for (int i = 0; i < halfOfLen; i++)
        {
            double currentDistance = Math.Abs(coordinates[i].X - coordinates[(i + halfOfLen) % coordinates.Length].X)
                                     + Math.Abs(coordinates[i].Y - coordinates[(i + halfOfLen) % coordinates.Length].Y);
            if (currentDistance < minDistance)
            {
                minDistance = currentDistance;
                minDistanceIndex = i;
            }
        }

        return minDistanceIndex;
    }

    public static int GetOppositesIndexByTriangles(LinearRing ring)
    {
        // todo Рассмотреть как относительную, так и абсолютную выпуклость (делить и не делить на что то (что то может быть разным!))
        double CalculateConvexity(Coordinate a, Coordinate b, Coordinate c)
        {
            // !conv! = min+mid-max = min+(a+b+c-max-min)-max = a+b+c-2max
            // Div it by (a+b+c) -> !conv! = 1 - 2max / (a+b+c)
            double ab = a.Distance(b);
            double bc = b.Distance(c);
            double ca = c.Distance(a);
            return 1 - 2 * Math.Max(ab, Math.Max(bc, ca)) / (ab + bc + ca);
        }

        Coordinate[] coordinates = ring.Coordinates;
        int halfOfLen = coordinates.Length / 2;
        int quarterOfLen = coordinates.Length / 4;


        // Convexity - выпуклость. Чем больше, тем ближе треугольник к равностороннему.
        // Чем меньше, тем он ближе к отрезку
        // Берем 2 противоположные точки, и проверяем 2 треугольника, получаемых добавлением к 2м исходным точкам
        // по очереди 2х точек, находящихся между ними по индексам с одной и с другой стороны
        double maxConvexity = Double.MinValue;
        int betterIndex = -1;

        for (int i = 1; i < halfOfLen + 1; i++)
        {
            
            // Если 2 внутренние точки с одной строны, наблюдались проблемы
            if (Math.Sign(LineService.VectorProduct(
                    coordinates[i], coordinates[(i + halfOfLen) % coordinates.Length],
                    coordinates[i], coordinates[(i + quarterOfLen) % coordinates.Length]))
                == Math.Sign(LineService.VectorProduct(
                    coordinates[i], coordinates[(i + halfOfLen) % coordinates.Length],
                    coordinates[i], coordinates[(i - quarterOfLen + coordinates.Length) % coordinates.Length]))
               )
            {
                continue;
            }
            
            
            double currentConv = Math.Min(
                CalculateConvexity(
                    coordinates[(i + quarterOfLen) % coordinates.Length],
                    coordinates[i],
                    coordinates[(i + halfOfLen) % coordinates.Length]),
                CalculateConvexity(
                    coordinates[(i - quarterOfLen + coordinates.Length) % coordinates.Length],
                    coordinates[i],
                    coordinates[(i + halfOfLen) % coordinates.Length]));
            if (currentConv > maxConvexity)
            {
                maxConvexity = currentConv;
                betterIndex = i;
            }

        }
        
        // todo Понять, возможна ли такая ситуация. Удалить иф к релизу
        if (betterIndex == -1)
        {
            throw new NotImplementedException("Не нашел подходящий индекс. Передайте ошибку Максиму");
        }

        return betterIndex;
    }
}