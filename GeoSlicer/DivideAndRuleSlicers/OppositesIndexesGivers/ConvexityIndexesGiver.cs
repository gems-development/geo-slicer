using System;
using System.Linq;
using GeoSlicer.Utils;
using NetTopologySuite.Geometries;

namespace GeoSlicer.DivideAndRuleSlicers.OppositesIndexesGivers;

/// <summary>
/// Возвращает индексы точек для разрезания по следующим правилам:
/// Если есть внутренние разрезания по точкам, противоположным по индексам, то разрезание будет по таким точкам.
/// Лучшим вариантом разрезания считаются точки, для которых больше коэффициент выпуклости 2-х треугольников,
/// построенных на этих 2-х точках и точках на расстоянии по индексам в четверть длины.
/// Convexity - выпуклость. Чем больше, тем ближе треугольник к равностороннему.
/// Чем меньше, тем он ближе к отрезку.
/// Разрезание идет всегда по линии, что пересекает фигуру более чем в 2-х точках.
/// </summary>
public class ConvexityIndexesGiver : OppositesIndexesGiver
{
    public ConvexityIndexesGiver(LineService lineService) : base(lineService)
    {
    }

    public override void GetIndexes(LinearRing ring, out int first, out int second)
    {
        Coordinate[] coordinates = ring.Coordinates;
        coordinates = coordinates.Take(coordinates.Length - 1).ToArray();

        int halfOfLen = coordinates.Length / 2;
        int quarterOfLen = coordinates.Length / 4;

        double maxConvexity = Double.MinValue;
        first = -1;

        for (int i = 0; i < halfOfLen; i++)
        {
            if (!IsInnerLine(coordinates, i, (i + halfOfLen) % coordinates.Length))
            {
                continue;
            }

            double currentConv =
                CalculateConvexity(
                    coordinates[i + quarterOfLen],
                    coordinates[i],
                    coordinates[i + halfOfLen]) +
                CalculateConvexity(
                    coordinates[(i - quarterOfLen + coordinates.Length) % coordinates.Length],
                    coordinates[i],
                    coordinates[i + halfOfLen]);

            if (currentConv > maxConvexity)
            {
                maxConvexity = currentConv;
                first = i;
            }
        }

        if (first == -1)
        {
            FindAnyInnerIndex(coordinates, ref first, out second);
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
}