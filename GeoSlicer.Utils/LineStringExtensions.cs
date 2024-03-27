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
}