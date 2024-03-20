using System;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Slicers;

public class SpiralSlicer : ISlicer
{
    private readonly GeometryFactory _gf;

    public SpiralSlicer(GeometryFactory? gf = null)
    {
        _gf = gf ?? NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
    }

    public LinearRing[] Slice(LinearRing ring, int maxDotCount)
    {
        int numOfParts = (ring.Count - 6 + maxDotCount) / (maxDotCount - 2);
        var linearRings = new LinearRing[numOfParts];
        CoordinateM[] coordinateMs = new CoordinateM[ring.Coordinates.Length - 1];
        for (int i = 0; i < ring.Coordinates.Length - 1; ++i)
        {
            coordinateMs[i] = new CoordinateM(ring.Coordinates[i].X, ring.Coordinates[i].Y, 0);
        }

        int leftCoordsNum = coordinateMs.Length;
        int currentPoint = 0;
        for (int i = 0; i < numOfParts; ++i)
        {
            int maxDotInIteration = Math.Min(maxDotCount, leftCoordsNum);
            Coordinate[] coords = new Coordinate[maxDotInIteration + 1];
            for (int j = 0; j < maxDotInIteration; ++j)
            {
                while (coordinateMs[currentPoint].M.Equals(1))
                {
                    currentPoint = (currentPoint + 1) % coordinateMs.Length;
                }

                coords[j] = coordinateMs[currentPoint];
                if (j > 0 && j < maxDotInIteration - 1)
                {
                    coordinateMs[currentPoint].M = 1;
                    leftCoordsNum--;
                }

                currentPoint = (currentPoint + 1) % coordinateMs.Length;
            }

            coords[^1] = coords[0];
            linearRings[i] = _gf.CreateLinearRing(coords);
            currentPoint = (currentPoint - 1 + coordinateMs.Length) % coordinateMs.Length;
        }

        return linearRings;
    }
}