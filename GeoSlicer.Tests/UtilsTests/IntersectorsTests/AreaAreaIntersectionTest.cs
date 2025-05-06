using GeoSlicer.Utils.Intersectors;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.UtilsTests.IntersectorsTests;

public class AreaAreaIntersectionTest
{
    private static readonly AreasIntersector AreasIntersector = new();

    [Theory]
    [InlineData(0, 2, 2, 0, 0, 2, 2, 0)]
    [InlineData(0, 2, 2, 0, 1, 3, 3, 1)]
    [InlineData(0, 2, 2, 0, 0, 4, 2, 2)]
    [InlineData(0, 2, 2, 0, 2, 2, 4, 0)]
    [InlineData(0, 2, 2, 0, 1, 2, 3, 0)]
    [InlineData(0, 2, 2, 0, 2, 4, 4, 2)]
    [InlineData(0, 3, 3, 0, -1, 2, 4, 1)]
    private void TestInside(
        double a1X, double a1Y, double a2X, double a2Y,
        double b1X, double b1Y, double b2X, double b2Y)
    {
        Coordinate a1 = new Coordinate(a1X, a1Y);
        Coordinate a2 = new Coordinate(a2X, a2Y);
        Coordinate b1 = new Coordinate(b1X, b1Y);
        Coordinate b2 = new Coordinate(b2X, b2Y);
        Assert.True(AreasIntersector.IsIntersects(a1, a2, b1, b2));
    }

    [Theory]
    [InlineData(0, 2, 2, 0, 4, 2, 6, 0)]
    private void TestOutside(
        double a1X, double a1Y, double a2X, double a2Y,
        double b1X, double b1Y, double b2X, double b2Y)
    {
        Coordinate a1 = new Coordinate(a1X, a1Y);
        Coordinate a2 = new Coordinate(a2X, a2Y);
        Coordinate b1 = new Coordinate(b1X, b1Y);
        Coordinate b2 = new Coordinate(b2X, b2Y);
        Assert.False(AreasIntersector.IsIntersects(a1, a2, b1, b2));
    }
}