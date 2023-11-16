using NetTopologySuite.Geometries;

namespace GeoSlicer.NonConvexSlicer;

public class TraverseDirection
{
    public static bool IsClockwiseBypass(LinearRing ring)
    {
        Coordinate[] coordinates = ring.Coordinates;
            
        //точка с минимальным х среди минимальных у
        Coordinate coordB = coordinates[0];

        Coordinate coordA = coordinates[coordinates.Length-2];
        Coordinate coordC = coordinates[1];

        for(int i = 1; i < coordinates.Length; i++)
        {
            if (coordinates[i].Y < coordB.Y)
            {
                coordB = coordinates[i];
            }
        }

        for(int i = 1; i < coordinates.Length; i++)
        {
            if (coordinates[i].Y == coordB.Y && coordinates[i].X <= coordB.X)
            {
                coordB = coordinates[i];
                coordA = coordinates[i - 1];
                if (i == coordinates.Length - 1)
                {
                    coordC = coordinates[1];
                }
                else
                {
                    coordC = coordinates[i + 1];
                }
            }
        }

        Coordinate vecAB = new Coordinate(coordB.X - coordA.X, coordB.Y - coordB.Y);
        Coordinate vecBC = new Coordinate(coordC.X - coordB.X, coordC.Y - coordB.Y);

        return SegmentService.VectorProduct(vecAB,vecBC) < 0;
    }

    public static void ChangeDirection(LinearRing ring)
    {
       //ring.Coordinates.Reverse
        Coordinate[] coordinates = ring.Coordinates;

        for(int i = 1; i < (coordinates.Length - 2) / 2 + 1; i++)
        {
            Coordinate coord = coordinates[i];
            coordinates[i] = coordinates[coordinates.Length - 1 - i];
            coordinates[coordinates.Length - 1 - i] = coord;
        }
    }
}

