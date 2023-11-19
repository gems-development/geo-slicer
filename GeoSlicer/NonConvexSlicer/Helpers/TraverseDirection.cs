using NetTopologySuite.Geometries;

namespace GeoSlicer.NonConvexSlicer.Helpers;

public static class TraverseDirection
{
    public static bool IsClockwiseBypass(LinearRing ring)
    {
        Coordinate[] coordinates = ring.Coordinates;

        //точка с минимальным х среди минимальных у
        var coordB = coordinates[0];

        var coordA = coordinates[^2];
        var coordC = coordinates[1];

        for (var i = 1; i < coordinates.Length; i++)
        {
            if (coordinates[i].Y < coordB.Y)
            {
                coordB = coordinates[i];
            }
        }

        for (var i = 1; i < coordinates.Length; i++)
        {
            if (!(Math.Abs(coordinates[i].Y - coordB.Y) < 1e-9) || !(coordinates[i].X <= coordB.X)) continue;
            coordB = coordinates[i];
            coordA = coordinates[i - 1];
            coordC = i == coordinates.Length - 1 ? coordinates[1] : coordinates[i + 1];
        }

        var vecAB = new Coordinate(coordB.X - coordA.X, coordB.Y - coordA.Y);
        var vecBC = new Coordinate(coordC.X - coordB.X, coordC.Y - coordB.Y);

        return SegmentService.VectorProduct(vecAB, vecBC) < 0;
    }

    public static void ChangeDirection(LinearRing ring)
    {
        Coordinate[] coordinates = ring.Coordinates;
        for (var i = 1; i < (coordinates.Length - 2) / 2 + 1; i++)
        {
            (coordinates[i], coordinates[coordinates.Length - 1 - i]) = (coordinates[coordinates.Length - 1 - i], coordinates[i]);
        }
    }
}