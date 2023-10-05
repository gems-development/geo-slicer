using NetTopologySuite.Geometries;

namespace GeoSlicer.Slicers;

public class SpiralConvexSlicer : ISlicer
{
    private readonly GeometryFactory _gf;

    public SpiralConvexSlicer()
    {
        _gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
    }

    public LinearRing[] Slice(LinearRing ring, int maxDotCount)
    {
        int numOfParts = (ring.Count - 6 + maxDotCount) / (maxDotCount - 2);
        var linearRings = new LinearRing[numOfParts];
        List<Coordinate> coords = new List<Coordinate>();
        List<Coordinate> polygonLeftCoords = new List<Coordinate>(ring.Coordinates);
        polygonLeftCoords.RemoveAt(polygonLeftCoords.Count - 1);
        int currentPoint = 0;
        for (int i = 0; i < numOfParts; ++i)
        {
            coords.Clear();
            for (int j = 0; j < Math.Min(maxDotCount, polygonLeftCoords.Count); ++j)
            {
                coords.Add(polygonLeftCoords[currentPoint % polygonLeftCoords.Count]);
                currentPoint++;
            }

            for (int j = 1; j < Math.Min(maxDotCount, polygonLeftCoords.Count) - 1; ++j)
            {
                polygonLeftCoords.Remove(coords[j]);
                currentPoint--;
            }

            coords.Add(coords[0]);
            linearRings[i] = _gf.CreateLinearRing(coords.ToArray());
            currentPoint--;
        }

        return linearRings;
    }
}