using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils;

public static class LineStringExtensions
{
    public static int? GetIndex(this LineString ring, Coordinate x)
    {
        int ringLength = ring.Count;
        for (int i = 0; i < ringLength; ++i)
        {
            if (x.Equals2D(ring[i]))
            {
                return i;
            }
        }

        return null;
    }
    
    public static void GetMinAndMaxOrdinates(
        this LineString ring, out double xMin, out double yMin, out double xMax, out double yMax)
    {
        Coordinate[] coordinates = ring.Coordinates;

        xMin = coordinates[0].X;
        yMin = coordinates[0].Y;
        xMax = coordinates[0].X;
        yMax = coordinates[0].Y;

        for (var i = 1; i < coordinates.Length; i++)
        {
            if (coordinates[i].X < xMin)
            {
                xMin = coordinates[i].X;
            }

            if (coordinates[i].Y < yMin)
            {
                yMin = coordinates[i].Y;
            }

            if (coordinates[i].X > xMax)
            {
                xMax = coordinates[i].X;
            }

            if (coordinates[i].Y > yMax)
            {
                yMax = coordinates[i].Y;
            }
        }
    }
}