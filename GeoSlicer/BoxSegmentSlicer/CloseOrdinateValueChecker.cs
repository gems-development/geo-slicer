using System;
using NetTopologySuite.Geometries;

namespace GeoSlicer.BoxSegmentSlicer;

internal static class CloseOrdinateValueChecker
{
    internal static void Fix(
        Envelope envelope,
        ref (double value, Ordinate ordinate) ordinateValue, double epsilon)
    {
        bool ordinateX = ordinateValue.ordinate == Ordinate.X;
        double minOrdinate = ordinateX ? envelope.MinX : envelope.MinY;
        double maxOrdinate = ordinateX ? envelope.MaxX : envelope.MaxY;

        if (AreEqual(minOrdinate, ordinateValue.value, epsilon) ||
            AreEqual(maxOrdinate, ordinateValue.value, epsilon))
        {
            Coordinate center = envelope.Centre;
            ordinateValue.value = ordinateX ? center.X : center.Y;
        }
    }
    
    private static bool AreEqual(double a, double b, double epsilon)
    {
        return Math.Abs(a - b) <= epsilon;
    }
}