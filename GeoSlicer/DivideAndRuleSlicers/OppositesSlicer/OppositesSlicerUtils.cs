using System;
using GeoSlicer.Utils;
using NetTopologySuite.Geometries;

namespace GeoSlicer.DivideAndRuleSlicers.OppositesSlicer;

public class OppositesSlicerUtils
{
    private readonly LineService _lineService;

    public OppositesSlicerUtils(LineService lineService)
    {
        _lineService = lineService;
    }

    /// <summary>
    /// Возвращает индекс одной точки из 2‑х противоположных по индексам.
    /// Лучшим вариантом разрезания считаются точки,
    /// что расположены друг к другу ближе по Манхеттенскому расстоянию
    /// </summary>
    public int GetNearestOpposites(LinearRing ring)
    {
        Coordinate[] coordinates = ring.Coordinates;
        int halfOfLen = coordinates.Length / 2;
        double minDistance = Double.MaxValue;
        int minDistanceIndex = -1;
        for (int i = 1; i < halfOfLen + 1; i++)
        {
            if (IsOuterLine(coordinates, i, halfOfLen))
            {
                continue;
            }

            double currentDistance = Math.Abs(coordinates[i].X - coordinates[(i + halfOfLen) % coordinates.Length].X)
                                     + Math.Abs(coordinates[i].Y - coordinates[(i + halfOfLen) % coordinates.Length].Y);
            if (currentDistance < minDistance)
            {
                minDistance = currentDistance;
                minDistanceIndex = i;
            }
        }
        
        // Скорее всего эта ситуация невозможна, но это не доказано
        if (minDistanceIndex == -1)
        {
            throw new NotImplementedException("Не нашел подходящий индекс");
        }

        return minDistanceIndex;
    }



    /// <summary>
    /// Возвращает индекс одной точки из 2‑х противоположных по индексам.
    /// Лучшим вариантом разрезания считаются точки, для которых больше коэффициент выпуклости 2-х треугольников,
    /// построенных на этих 2-х точках и точках на расстоянии по индексам в четверть длины.
    /// Convexity - выпуклость. Чем больше, тем ближе треугольник к равностороннему.
    /// Чем меньше, тем он ближе к отрезку.
    /// </summary>
    public int GetOppositesIndexByTriangles(LinearRing ring)
    {
        Coordinate[] coordinates = ring.Coordinates;
        int halfOfLen = coordinates.Length / 2;
        int quarterOfLen = coordinates.Length / 4;
        
        double maxConvexity = Double.MinValue;
        int betterIndex = -1;

        for (int i = 1; i < halfOfLen + 1; i++)
        {
            if (IsOuterLine(coordinates, i, halfOfLen))
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

        // Скорее всего эта ситуация невозможна, но это не доказано
        if (betterIndex == -1)
        {
            throw new NotImplementedException("Не нашел подходящий индекс");
        }

        return betterIndex;

        double CalculateConvexity(Coordinate a, Coordinate b, Coordinate c)
        {
            // min, mid и max - длины сторон треугольника 
            // Convexity = min+mid-max = min+(a+b+c-max-min)-max = a+b+c-2max
            // Div it at (a+b+c) -> Convexity = 1 - 2max / (a+b+c)
            double ab = a.Distance(b);
            double bc = b.Distance(c);
            double ca = c.Distance(a);
            return 1 - 2 * Math.Max(ab, Math.Max(bc, ca)) / (ab + bc + ca);
        }
    }

    private bool IsOuterLine(Coordinate[] coordinates, int i, int halfOfLen)
    {
        return !_lineService.InsideTheAngleWithoutBorders(
                   coordinates[i], coordinates[(i + halfOfLen) % coordinates.Length],
                   coordinates[i - 1], coordinates[i], coordinates[i + 1])
               || !_lineService.InsideTheAngleWithoutBorders(
                   coordinates[(i + halfOfLen) % coordinates.Length], coordinates[i],
                   coordinates[i + halfOfLen - 1],
                   coordinates[(i + halfOfLen) % coordinates.Length],
                   coordinates[(i + halfOfLen + 1) % coordinates.Length]);
    }

}