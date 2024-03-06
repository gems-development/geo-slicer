using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.UtilsTests.IntersectorsTests;

public class GetSegmentIntersectionTests
{
    // a1b1 a2b2, a1b2 a2b1, a2b1 a1b2, a2b2 a1b1

    private const double Epsilon = 1E-6;
    private static readonly EpsilonCoordinateComparator EpsilonCoordinateComparator = new(Epsilon);

    private static readonly LineIntersector LineIntersector =
        new(EpsilonCoordinateComparator, new LineService(1E-5), Epsilon);


    [Theory]
    [InlineData(1, 1, -1, -1, 1, -1, -1, 1, 0, 0,
        LineIntersectionType.Inner)]
    [InlineData(0.4, 0, 0, 0.8, 0.2, 0, 0.2, 1, 0.2, 0.4,
        LineIntersectionType.Inner)]
    [InlineData(0.4, 0, 0, 0.8, 0.2, 1, 0.2, 0, 0.2, 0.4,
        LineIntersectionType.Inner)]
    [InlineData(1, 1, 0, 0, -1, 3, 1, 1, 1, 1,
        LineIntersectionType.Corner)]
    [InlineData(1, 1, 0.3, 0.3, 0.3, 0.3, 1, 2, 0.3, 0.3,
        LineIntersectionType.Corner)]
    [InlineData(0.1, 0.1, 0.3, 0.6, 0.2, 0.35, 0.15, 0.15, 0.2, 0.35,
        LineIntersectionType.TyShaped)]
    [InlineData(0.3, 0.6, 0.1, 0.1, 0.2, 0.35, 800, 34000, 0.2, 0.35,
        LineIntersectionType.TyShaped)]
    [InlineData(1, 1, 2, 3, 3, 0.5, 2.5, 1, 1.5, 2,
        LineIntersectionType.Outside)]
    [InlineData(1, 1, 0, -1, 3, 0.5, 2.5, 1, 1.5, 2,
        LineIntersectionType.Outside)]
    private void TestWithIntersection(
        double a1X, double a1Y, double a2X, double a2Y,
        double b1X, double b1Y, double b2X, double b2Y,
        double intersectionX, double intersectionY, LineIntersectionType expectedType)
    {
        Coordinate a1 = new Coordinate(a1X, a1Y);
        Coordinate a2 = new Coordinate(a2X, a2Y);
        Coordinate b1 = new Coordinate(b1X, b1Y);
        Coordinate b2 = new Coordinate(b2X, b2Y);
        Coordinate expectedIntersection = new Coordinate(intersectionX, intersectionY);

        (LineIntersectionType, Coordinate) result = LineIntersector.GetIntersection(a1, a2, b1, b2)!;

        Assert.Equal(expectedType, result.Item1);
        Assert.True(EpsilonCoordinateComparator.IsEquals(expectedIntersection, result.Item2));
    }


    [Theory]
    [InlineData(-1, 0, 0, 1, -2, 0, 1, 3, LineIntersectionType.NoIntersection)]
    [InlineData(-1, 0, 0, 1, 1, 3, -2, 0, LineIntersectionType.NoIntersection)]
    [InlineData(-1, 0, 0, 2, 0, 2, 1, 4, LineIntersectionType.Extension)]
    [InlineData(-1, 0, 0, 2, 1, 4, 0, 2, LineIntersectionType.Extension)]
    [InlineData(0, 2, -1, 0, 1, 4, 0, 2, LineIntersectionType.Extension)]
    [InlineData(0, 2, -1, 0, 0, 2, -1, 0, LineIntersectionType.Equals)]
    [InlineData(0, 2, -1, 0, -1, 0, 0, 2, LineIntersectionType.Equals)]
    [InlineData(-1, 0, 0, 2, -1, 0, 0, 2, LineIntersectionType.Equals)]
    [InlineData(0, 0, 0, 2, 0, 2, 0, 0, LineIntersectionType.Equals)]
    [InlineData(1, 1, 4, 3, 1, 1, 2.5, 2, LineIntersectionType.Part)]
    [InlineData(1, 1, 2.5, 2, 1, 1, 4, 3, LineIntersectionType.Part)]
    [InlineData(1, 1, 4, 3, 2.5, 2, 4, 3, LineIntersectionType.Part)]
    [InlineData(2.5, 2, 4, 3, 1, 1, 4, 3, LineIntersectionType.Part)]
    [InlineData(1, 1, 3, 2, 2, 1.5, 4, 2.5, LineIntersectionType.Overlay)]
    [InlineData(2, 1.5, 4, 2.5, 1, 1, 3, 2, LineIntersectionType.Overlay)]
    [InlineData(1, 1, 4, 2.5, 2, 1.5, 3, 2, LineIntersectionType.Contains)]
    [InlineData(2, 1.5, 3, 2, 1, 1, 4, 2.5, LineIntersectionType.Contains)]
    private void TestWithoutIntersection(
        double a1X, double a1Y, double a2X, double a2Y,
        double b1X, double b1Y, double b2X, double b2Y,
        LineIntersectionType expectedType)
    {
        Coordinate a1 = new Coordinate(a1X, a1Y);
        Coordinate a2 = new Coordinate(a2X, a2Y);
        Coordinate b1 = new Coordinate(b1X, b1Y);
        Coordinate b2 = new Coordinate(b2X, b2Y);

        (LineIntersectionType, Coordinate) result = LineIntersector.GetIntersection(a1, a2, b1, b2)!;

        Assert.Equal(expectedType, result.Item1);
    }
}