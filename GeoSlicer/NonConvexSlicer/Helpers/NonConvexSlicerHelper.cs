namespace GeoSlicer.NonConvexSlicer.Helpers;

public static class NonConvexSlicerHelper
{
    private static bool FirstPointCanSeeSecond(CoordinatePCN[] ring, CoordinatePCN pointA, CoordinatePCN pointB)
    {
        return pointA.Equals2D(pointB) ||
               SegmentService.InsideTheAngle(pointA, pointB, ring[pointA.NL],
                   pointA, ring[pointA.PL]) ||
               (ring[pointA.NL].Equals2D(pointB) && pointA.NL == pointB.C) ||
               (ring[pointA.PL].Equals2D(pointB) && pointA.PL == pointB.C);
    }

    public static bool CanSeeEachOther(CoordinatePCN[] ring, CoordinatePCN pointA, CoordinatePCN pointB)
    {
        return FirstPointCanSeeSecond(ring, pointA, pointB) && FirstPointCanSeeSecond(ring, pointB, pointA);
    }
}