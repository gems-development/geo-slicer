using System;
using NetTopologySuite.Geometries;

namespace GeoSlicer.DivideAndRuleSlicer;

public class Utils
{
    public static int GetNearestOpposites(LinearRing ring)
    {
        Coordinate[] coordinates = ring.Coordinates;
        int halfOfLen = coordinates.Length / 2;
        double minDistance = Double.MaxValue;
        int minDistanceIndex = -1;
        for (int i = 0; i < halfOfLen; i++)
        {
            double currentDistance = Math.Pow(coordinates[i].X - coordinates[i + halfOfLen].X, 2)
                                     + Math.Pow(coordinates[i].Y - coordinates[i + halfOfLen].Y, 2);
            if (currentDistance < minDistance)
            {
                minDistance = currentDistance;
                minDistanceIndex = i;
            }
        }

        return minDistanceIndex;
    }
    
}