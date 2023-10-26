using NetTopologySuite.Geometries;

namespace GeoSlicer.Slicers;

public class StepConvexSlicer : ISlicer
{
    private readonly GeometryFactory _gf;

    public StepConvexSlicer(GeometryFactory? gf = null)
    {
        _gf = gf ?? NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
    }

    public LinearRing[] Slice(LinearRing ring, int maxDotCount)
    {
        Coordinate[] coordinates = ring.Coordinates;
        int ringsCount = (coordinates.Length - 6 + maxDotCount) / (maxDotCount - 2);
        LinearRing[] result = new LinearRing[ringsCount];

        int left = 0;
        int right = coordinates.Length - 2;
        Coordinate[] newRingCoordinates;

        int ringNumber = 0;
        while (right - left > maxDotCount - 2)
        {
            newRingCoordinates = new Coordinate[maxDotCount + 1];
            newRingCoordinates[0] = coordinates[left];
            for (int j = 1; j < maxDotCount - 1; j++)
            {
                newRingCoordinates[j] = coordinates[left + j];
            }

            newRingCoordinates[^2] = coordinates[right];
            newRingCoordinates[^1] = coordinates[left];
            left += maxDotCount - 2;

            result[ringNumber] = _gf.CreateLinearRing(newRingCoordinates);
            ringNumber++;

            if (right - left <= maxDotCount - 2)
                break;

            newRingCoordinates = new Coordinate[maxDotCount + 1];
            newRingCoordinates[0] = coordinates[right];
            for (int j = 1; j < maxDotCount - 1; j++)
            {
                newRingCoordinates[j] = coordinates[right - j];
            }

            newRingCoordinates[^2] = coordinates[left];
            newRingCoordinates[^1] = coordinates[right];
            right -= maxDotCount - 2;

            result[ringNumber] = _gf.CreateLinearRing(newRingCoordinates);
            ringNumber++;
        }

        if (ringNumber < ringsCount)
        {
            newRingCoordinates = new Coordinate[right - left + 2];
            for (int i = 0; i <= right - left + 1; i++)
            {
                newRingCoordinates[i] = coordinates[left + i];
            }

            newRingCoordinates[^1] = coordinates[left];
            result[ringNumber] = _gf.CreateLinearRing(newRingCoordinates);
        }

        return result;
    }
}