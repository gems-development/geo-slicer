using System.Collections.Generic;
using NetTopologySuite.Geometries;
using static GeoSlicer.NonConvexSlicer.Helpers.SegmentService;

namespace GeoSlicer.NonConvexSlicer.Helpers;

public static class NonConvexSlicerHelper
{
    public static List<CoordinatePCN> GetSpecialPoints(LinearRing ring)
    {
        var list = new List<CoordinatePCN>();
        for (var i = 0; i < ring.Coordinates.Length - 1; ++i)
        {
            var clockwise = TraverseDirection.IsClockwiseBypass(ring);
            if (VectorProduct(
                    new Coordinate(
                        ring.Coordinates[i].X -
                        ring.Coordinates[(i - 1 + ring.Coordinates.Length - 1) % (ring.Coordinates.Length - 1)].X,
                        ring.Coordinates[i].Y -
                        ring.Coordinates[(i - 1 + ring.Coordinates.Length - 1) % (ring.Coordinates.Length - 1)].Y),
                    new Coordinate(ring.Coordinates[(i + 1) % (ring.Coordinates.Length - 1)].X - ring.Coordinates[i].X,
                        ring.Coordinates[(i + 1) % (ring.Coordinates.Length - 1)].Y - ring.Coordinates[i].Y)
                ) >= 0 == clockwise)
            {
                list.Add(new CoordinatePCN(ring.Coordinates[i].X, ring.Coordinates[i].Y, c: i));
            }
        }

        return list;
    }
    
    private static bool FirstPointCanSeeSecond(CoordinatePCN[] ring, CoordinatePCN pointA, CoordinatePCN pointB)
    {
        return pointA.Equals2D(pointB) ||
               InsideTheAngle(pointA, pointB, ring[pointA.NL],
                   pointA, ring[pointA.PL]) ||
               (ring[pointA.NL].Equals2D(pointB) && pointA.NL == pointB.C) ||
               (ring[pointA.PL].Equals2D(pointB) && pointA.PL == pointB.C);
    }

    public static bool CanSeeEachOther(CoordinatePCN[] ring, CoordinatePCN pointA, CoordinatePCN pointB)
    {
        return FirstPointCanSeeSecond(ring, pointA, pointB) && FirstPointCanSeeSecond(ring, pointB, pointA);
    }

    public static bool HasIntersection(CoordinatePCN[] ring, CoordinatePCN coordCurrent, CoordinatePCN coordNext)
    {
        if (coordCurrent.Equals2D(coordNext)) return false;
        if (coordCurrent.PL == coordNext.C) return true;
        var index = coordCurrent.C;
        while (ring[index].NL != coordCurrent.C)
        {
            var firstCoord = ring[index];
            var secondCoord = ring[firstCoord.NL];
            if (IsIntersectionOfSegments(coordCurrent, coordNext, firstCoord, secondCoord))
            {
                return true;
            }

            index = secondCoord.C;
        }

        return IsIntersectionOfSegments(coordCurrent, coordNext, ring[index], coordCurrent);
    }
}