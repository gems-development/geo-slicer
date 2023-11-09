using GeoSlicer.HoleDeleters;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.HoleDeletersTests;

public class BruteForceHoleDeleterTests
{
    [Theory]
    [InlineData("GeoSlicer.Tests/HoleDeletersTests/TestData/sample.geojson")]
    private void SavesAllPoints(string fileName)
    {
        Polygon polygon = GeoJsonFileService.GeoJsonFileService.ReadGeometryFromFile<Polygon>(fileName);
        BruteForceHoleDeleter holeDeleter = new BruteForceHoleDeleter();
        HashSet<Coordinate> expected = new HashSet<Coordinate>(polygon.Coordinates);
        foreach (LinearRing hole in polygon.Holes)
        {
            expected.UnionWith(hole.Coordinates);
        }

        LinearRing result = holeDeleter.DeleteHoles(polygon);
        HashSet<Coordinate> actual = new HashSet<Coordinate>(result.Coordinates);
        
        Assert.Equal(expected, actual);
    }
}