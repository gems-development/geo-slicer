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

    public static (double, double, double, double) GetMinAndMaxPoints(this LineString ring)
    {
        Coordinate[] coordinates = ring.Coordinates;

        var coordMinX = coordinates[0].X;
        var coordMinY = coordinates[0].Y;
        var coordMaxX = coordinates[0].X;
        var coordMaxY = coordinates[0].Y;

        for (var i = 1; i < coordinates.Length; i++)
        {
            if (coordinates[i].X < coordMinX)
            {
                coordMinX = coordinates[i].X;
            }
            if (coordinates[i].Y < coordMinY)
            {
                coordMinY = coordinates[i].Y;
            }
            if (coordinates[i].X > coordMaxX)
            {
                coordMaxX = coordinates[i].X;
            }
            if (coordinates[i].Y > coordMaxY)
            {
                coordMaxY = coordinates[i].Y;
            }
            
        }
        return (coordMinX, coordMinY, coordMaxX, coordMaxY);
    }
}