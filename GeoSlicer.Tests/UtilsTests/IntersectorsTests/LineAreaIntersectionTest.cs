using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.UtilsTests.IntersectorsTests;

public class LineAreaIntersectionTest
{
    private const double Epsilon = 1E-6;

    private static readonly LineAreaIntersector LineAreaIntersector =
        new(new LineService(1E-5, new EpsilonCoordinateComparator(Epsilon)), Epsilon);

    [Theory]
    [InlineData(0, 2, 2, 0, 0, 2, 2, 0, LineAreaIntersectionType.Inside)]
    [InlineData(0, 4, 4, 0, 1, 3, 3, 1, LineAreaIntersectionType.Inside)]
    [InlineData(0, 4, 4, 0, 4, 4, 4, 0, LineAreaIntersectionType.Inside)]
    private void TestInside(
        double a1X, double a1Y, double a2X, double a2Y,
        double b1X, double b1Y, double b2X, double b2Y,
        LineAreaIntersectionType expectedType)
    {
        Coordinate a1 = new Coordinate(a1X, a1Y);
        Coordinate a2 = new Coordinate(a2X, a2Y);
        Coordinate b1 = new Coordinate(b1X, b1Y);
        Coordinate b2 = new Coordinate(b2X, b2Y);
        Assert.True(LineAreaIntersector.CheckIntersection(expectedType, a1, a2, b1, b2));
    }

    [Theory]
    [InlineData(0, 2, 2, 0, 1, 1, 2, 5, LineAreaIntersectionType.PartlyInside)]
    [InlineData(0, 2, 2, 0, 1, 1, 3, 1, LineAreaIntersectionType.PartlyInside)]
    private void TestPartlyInside(
        double a1X, double a1Y, double a2X, double a2Y,
        double b1X, double b1Y, double b2X, double b2Y,
        LineAreaIntersectionType expectedType)
    {
        Coordinate a1 = new Coordinate(a1X, a1Y);
        Coordinate a2 = new Coordinate(a2X, a2Y);
        Coordinate b1 = new Coordinate(b1X, b1Y);
        Coordinate b2 = new Coordinate(b2X, b2Y);
        Assert.True(LineAreaIntersector.CheckIntersection(expectedType, a1, a2, b1, b2));
    }

    [Theory]
    [InlineData(0, 2, 2, 0, -1, 1, 3, 1, LineAreaIntersectionType.Overlay)]
    [InlineData(0, 3, 2, 0, -2, 4, 4, -1, LineAreaIntersectionType.Overlay)]
    [InlineData(0, 3, 2, 0, -2, 6, 4, -3, LineAreaIntersectionType.Overlay)]
    [InlineData(0, 3, 2, 0, -1, 6, 3, -3, LineAreaIntersectionType.Overlay)]
    [InlineData(0, 3, 2, 0, -1, 6, 3, 0, LineAreaIntersectionType.Overlay)]
    [InlineData(0, 3, 2, 0, -1, 6, 2, -1, LineAreaIntersectionType.Overlay)]
    [InlineData(0, 3, 2, 0, -1, 5, 3, 1, LineAreaIntersectionType.Overlay)]
    [InlineData(0, 3, 2, 0, -1, 5, 5, 1, LineAreaIntersectionType.Overlay)]
    [InlineData(0, 3, 2, 0, 2, 4, 2, -1, LineAreaIntersectionType.Overlay)]
    private void TestOverlay(
        double a1X, double a1Y, double a2X, double a2Y,
        double b1X, double b1Y, double b2X, double b2Y,
        LineAreaIntersectionType expectedType)
    {
        Coordinate a1 = new Coordinate(a1X, a1Y);
        Coordinate a2 = new Coordinate(a2X, a2Y);
        Coordinate b1 = new Coordinate(b1X, b1Y);
        Coordinate b2 = new Coordinate(b2X, b2Y);
        Assert.True(LineAreaIntersector.CheckIntersection(expectedType, a1, a2, b1, b2));
    }

    [Theory]
    [InlineData(0, 3, 2, 0, 3, 1, 4, 2, LineAreaIntersectionType.Outside)]
    [InlineData(0, 3, 2, 0, 1, 4, 3, 5, LineAreaIntersectionType.Outside)]
    [InlineData(0, 3, 2, 0, 1, 6, 4, -1, LineAreaIntersectionType.Outside)]
    [InlineData(0, 3, 2, 0, -1, 3, 3, 4, LineAreaIntersectionType.Outside)]
    [InlineData(0, 3, 2, 0, -2, 2, 1, 5, LineAreaIntersectionType.Outside)]
    private void TestOutside(
        double a1X, double a1Y, double a2X, double a2Y,
        double b1X, double b1Y, double b2X, double b2Y,
        LineAreaIntersectionType expectedType)
    {
        Coordinate a1 = new Coordinate(a1X, a1Y);
        Coordinate a2 = new Coordinate(a2X, a2Y);
        Coordinate b1 = new Coordinate(b1X, b1Y);
        Coordinate b2 = new Coordinate(b2X, b2Y);
        Assert.True(LineAreaIntersector.CheckIntersection(expectedType, a1, a2, b1, b2));
    }
}