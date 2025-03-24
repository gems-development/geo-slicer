using System;
using System.Linq;
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
    public void GetNearestOpposites(LinearRing ring, out int first, out int second)
    {
        Coordinate[] coordinates = ring.Coordinates;
        coordinates = coordinates.Take(coordinates.Length - 1).ToArray();

        int halfOfLen = coordinates.Length / 2;
        double minDistance = Double.MaxValue;
        first = -1;
        for (int i = 0; i < halfOfLen; i++)
        {
            if (!IsInnerLine(coordinates, i, i + halfOfLen))
            {
                continue;
            }

            double currentDistance = Math.Abs(coordinates[i].X - coordinates[i + halfOfLen].X)
                                     + Math.Abs(coordinates[i].Y - coordinates[i + halfOfLen].Y);

            if (currentDistance < minDistance)
            {
                minDistance = currentDistance;
                first = i;
            }
        }

        if (first == -1)
        {
            FindAnyInnerIndex(coordinates, out first, out second);
        }
        else
        {
            second = first + halfOfLen;
        }
    }


    /// <summary>
    /// Возвращает индекс одной точки из 2‑х противоположных по индексам.
    /// Лучшим вариантом разрезания считаются точки, для которых больше коэффициент выпуклости 2-х треугольников,
    /// построенных на этих 2-х точках и точках на расстоянии по индексам в четверть длины.
    /// Convexity - выпуклость. Чем больше, тем ближе треугольник к равностороннему.
    /// Чем меньше, тем он ближе к отрезку.
    /// </summary>
    public void GetOppositesIndexByTriangles(LinearRing ring, out int first, out int second)
    {
        Coordinate[] coordinates = ring.Coordinates;
        coordinates = coordinates.Take(coordinates.Length - 1).ToArray();

        int halfOfLen = coordinates.Length / 2;
        int quarterOfLen = coordinates.Length / 4;

        double maxConvexity = Double.MinValue;
        first = -1;

        for (int i = 0; i < halfOfLen; i++)
        {
            if (!IsInnerLine(coordinates, i, i + halfOfLen))
            {
                continue;
            }

            double currentConv = Math.Min(
                CalculateConvexity(
                    coordinates[i + quarterOfLen],
                    coordinates[i],
                    coordinates[i + halfOfLen]),
                CalculateConvexity(
                    coordinates[(i - quarterOfLen + coordinates.Length) % coordinates.Length],
                    coordinates[i],
                    coordinates[i + halfOfLen]));

            if (currentConv > maxConvexity)
            {
                maxConvexity = currentConv;
                first = i;
            }
        }

        if (first == -1)
        {
            FindAnyInnerIndex(coordinates, out first, out second);
        }
        else
        {
            second = first + halfOfLen;
        }

        return;

        double CalculateConvexity(Coordinate a, Coordinate b, Coordinate c)
        {
            // min, mid и max - длины сторон треугольника 
            // Convexity = (min+mid) / max = (a+b+c-max) / max = (a+b+c) / max - 1
            // Упустим "-1" -> Convexity = (a+b+c) / max
            double ab = a.Distance(b);
            double bc = b.Distance(c);
            double ca = c.Distance(a);
            return (ab + bc + ca) / Math.Max(ab, Math.Max(bc, ca));
        }
    }

    private void FindAnyInnerIndex(Coordinate[] coordinates, out int first, out int second)
    {
        int halfOfLen = coordinates.Length / 2;
        int shift = 1;

        while (true)
        {
            for (int i = 0; i < coordinates.Length; i++)
            {
                if (IsInnerLine(
                        coordinates, i, (i + halfOfLen + shift) % coordinates.Length))
                {
                    first = i;
                    second = (i + halfOfLen + shift) % coordinates.Length;
                    return;
                }
            }

            shift++;
        }
    }

    private bool IsInnerLine(Coordinate[] coordinates, int first, int second)
    {
        return _lineService.InsideTheAngleWithoutBorders(
                   coordinates[first],
                   coordinates[second],
                   coordinates[(first + 1) % coordinates.Length],
                   coordinates[first],
                   coordinates[(first - 1 + coordinates.Length) % coordinates.Length])
               && _lineService.InsideTheAngleWithoutBorders(
                   coordinates[second],
                   coordinates[first],
                   coordinates[(second + 1) % coordinates.Length],
                   coordinates[second],
                   coordinates[(second - 1 + coordinates.Length) % coordinates.Length]);
    }
}