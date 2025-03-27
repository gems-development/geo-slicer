using System;
using System.Linq;
using GeoSlicer.Utils;
using NetTopologySuite.Geometries;

namespace GeoSlicer.DivideAndRuleSlicers.OppositesIndexesGivers;

/// <summary>
/// Возвращает индексы точек для разрезания по следующим правилам:
/// Если есть внутренние разрезания по точкам, противоположным по индексам, то разрезание будет по таким точкам.
/// Лучшим вариантом разрезания считаются точки,
/// что расположены друг к другу ближе по Манхеттенскому расстоянию.
/// Разрезание идет всегда по линии, что пересекает фигуру более чем в 2-х точках.
/// </summary>
public class NearestIndexesGiver : OppositesIndexesGiver
{
    public NearestIndexesGiver(LineService lineService) : base(lineService)
    {
    }
    
    public override void GetIndexes(LinearRing ring, out int first, out int second)
    {
        Coordinate[] coordinates = ring.Coordinates;
        coordinates = coordinates.Take(coordinates.Length - 1).ToArray();

        int halfOfLen = coordinates.Length / 2;
        double minDistance = Double.MaxValue;
        first = -1;
        for (int i = 0; i < halfOfLen; i++)
        {
            if (!IsInnerLine(coordinates, i, (i + halfOfLen) % coordinates.Length))
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
}