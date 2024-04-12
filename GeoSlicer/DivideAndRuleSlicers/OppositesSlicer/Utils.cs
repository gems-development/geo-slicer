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

    public static int GetNearestOppositesInner(LinearRing ring)
    {
        Coordinate[] coordinates = ring.Coordinates;
        int halfOfLen = coordinates.Length / 2;
        double minDistance = Double.MaxValue;
        int minDistanceIndex = -1;
        // todo Подумать над правильными индексами
        for (int i = 1; i < halfOfLen + 1; i++)
        {
            double currentDistance = Math.Abs(coordinates[i].X - coordinates[(i + halfOfLen) % coordinates.Length].X)
                                     + Math.Abs(coordinates[i].Y - coordinates[(i + halfOfLen) % coordinates.Length].Y);
            if (currentDistance < minDistance)
            {
                if (VectorService.InsideTheAngle(
                        coordinates[i],
                        coordinates[(i + halfOfLen) % coordinates.Length],
                        coordinates[i + 1],
                        coordinates[i],
                        coordinates[i - 1]))
                {
                    minDistance = currentDistance;
                    minDistanceIndex = i;
                }
            }
        }

        return minDistanceIndex;
    }
}