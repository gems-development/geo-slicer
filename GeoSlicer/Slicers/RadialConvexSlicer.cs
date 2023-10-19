using NetTopologySuite.Geometries;

namespace GeoSlicer.Slicers;

public class RadialConvexSlicer : ISlicer
{
    private readonly GeometryFactory _gf;

    public RadialConvexSlicer()
    {
        _gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
    }

    public LinearRing[] Slice(LinearRing ring, int maxDotCount)
    {
        Coordinate[] coordinates = ring.Coordinates;
        int ringsCount = (coordinates.Length - 6 + maxDotCount) / (maxDotCount - 2);
        LinearRing[] result = new LinearRing[ringsCount];

        int index = 2;
        Coordinate[] newRingCoordinates;
        Coordinate first = coordinates[0];
        Coordinate last = coordinates[1];

        for (int i = 0; i < ringsCount - 1; i++)
        {
            newRingCoordinates = new Coordinate[maxDotCount + 1];
            newRingCoordinates[0] = first;
            newRingCoordinates[1] = last;
            for (int j = 2; j < maxDotCount; j++)
            {
                newRingCoordinates[j] = coordinates[index];
                index++;
            }

            newRingCoordinates[maxDotCount] = first;
            last = newRingCoordinates[maxDotCount - 1];
            result[i] = _gf.CreateLinearRing(newRingCoordinates);
        }

        newRingCoordinates = new Coordinate[coordinates.Length - index + 2];
        newRingCoordinates[0] = first;
        newRingCoordinates[1] = last;
        for (int i = 2; i < newRingCoordinates.Length - 1; i++)
        {
            newRingCoordinates[i] = coordinates[index + i - 2];
        }

        newRingCoordinates[^1] = first;
        result[ringsCount - 1] = _gf.CreateLinearRing(newRingCoordinates);
        return result;
    }
}