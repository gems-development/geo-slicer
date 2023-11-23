using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.IntersectorsTests;

public class GetSegmentIntersectionTests
{
    // a1b1 a2b2, a1b2 a2b1, a2b1 a1b2, a2b2 a1b1

    private const double Delta = 1E-5;
    private readonly Intersector _intersector = new Intersector(new EpsilonCoordinateComparator(Delta), Delta);

    [Theory]
    [InlineData(-3, 2, -1, 1.5, -2, 1.5, 0, 1.45)]
    [InlineData(-3, 2, -1, 1.5, 0, 1.45, -2, 1.5)]
    [InlineData(-1, 1.5, -3, 2, -2, 1.5, 0, 1.45)]
    [InlineData(-1, 1.5, -3, 2, 0, 1.45, -2, 1.5)]
    private void NoIntersectionTest(double a1x, double a1y, double a2x, double a2y, double b1x, double b1y, double b2x,
        double b2y)
    {
        Coordinate a1 = new Coordinate(a1x, a1y);
        Coordinate a2 = new Coordinate(a2x, a2y);
        Coordinate b1 = new Coordinate(b1x, b1y);
        Coordinate b2 = new Coordinate(b2x, b2y);

        Intersector.IntersectionType intersectionType =
            _intersector.GetSegmentIntersection(a1, a2, b1, b2).intersectionType;

        Assert.Equal(Intersector.IntersectionType.NoIntersection, intersectionType);
    }

    [Theory]
    [InlineData(-3, 2, -1, 1.5, -3, 2, -1, 1.5)]
    [InlineData(-3, 2, -1, 1.5, -1, 1.5, -3, 2)]
    private void EqualsTest(double a1x, double a1y, double a2x, double a2y, double b1x, double b1y, double b2x,
        double b2y)
    {
        Coordinate a1 = new Coordinate(a1x, a1y);
        Coordinate a2 = new Coordinate(a2x, a2y);
        Coordinate b1 = new Coordinate(b1x, b1y);
        Coordinate b2 = new Coordinate(b2x, b2y);

        Intersector.IntersectionType intersectionType =
            _intersector.GetSegmentIntersection(a1, a2, b1, b2).intersectionType;

        Assert.Equal(Intersector.IntersectionType.Equals, intersectionType);
    }

    [Theory]
    [InlineData(-2, 1, 0, 1.4, 0, 1.4, 1, 1)]
    [InlineData(-2, 1, 0, 1.4, 1, 1, 0, 1.4)]
    [InlineData(-2, 1, 0, 1.4, 0, 1.4, -1, 1)]
    [InlineData(-2, 1, 0, 1.4, -1, 1, 0, 1.4)]
    [InlineData(-2, 1, 0, 1.4, 0, 1.4, -1, 2)]
    [InlineData(-2, 1, 0, 1.4, -1, 2, 0, 1.4)]
    [InlineData(0, 1.4, -2, 1, 0, 1.4, 1, 1)]
    [InlineData(0, 1.4, -2, 1, 1, 1, 0, 1.4)]
    [InlineData(0, 1.4, -2, 1, 0, 1.4, -1, 1)]
    [InlineData(0, 1.4, -2, 1, -1, 1, 0, 1.4)]
    [InlineData(0, 1.4, -2, 1, 0, 1.4, -1, 2)]
    [InlineData(0, 1.4, -2, 1, -1, 2, 0, 1.4)]
    private void EndsIntersectionTest(double a1x, double a1y, double a2x, double a2y, double b1x, double b1y,
        double b2x,
        double b2y)
    {
        Coordinate a1 = new Coordinate(a1x, a1y);
        Coordinate a2 = new Coordinate(a2x, a2y);
        Coordinate b1 = new Coordinate(b1x, b1y);
        Coordinate b2 = new Coordinate(b2x, b2y);

        var res =
            _intersector.GetSegmentIntersection(a1, a2, b1, b2);

        Assert.Equal((Intersector.IntersectionType.EndsIntersection, new Coordinate(0, 1.4)), res);
    }


    [Theory]
    [InlineData(-2, 0.2, 1, 1.4, -0.5, 0.8, 0.8, 0.2)]
    [InlineData(-2, 0.2, 1, 1.4, -0.5, 0.8, -1, 1)]
    [InlineData(-2, 0.2, 1, 1.4, -0.5, 0.8, -1.5, 0.1)]
    [InlineData(-2, 0.2, 1, 1.4, 0.8, 0.2, -0.5, 0.8)]
    [InlineData(-2, 0.2, 1, 1.4, -1, 1, -0.5, 0.8)]
    [InlineData(-2, 0.2, 1, 1.4, -1.5, 0.1, -0.5, 0.8)]
    [InlineData(1, 1.4, -2, 0.2, -0.5, 0.8, 0.8, 0.2)]
    [InlineData(1, 1.4, -2, 0.2, -0.5, 0.8, -1, 1)]
    [InlineData(1, 1.4, -2, 0.2, -0.5, 0.8, -1.5, 0.1)]
    [InlineData(1, 1.4, -2, 0.2, 0.8, 0.2, -0.5, 0.8)]
    [InlineData(1, 1.4, -2, 0.2, -1, 1, -0.5, 0.8)]
    [InlineData(1, 1.4, -2, 0.2, -1.5, 0.1, -0.5, 0.8)]
    private void TangentIntersectionTest(double a1x, double a1y, double a2x, double a2y, double b1x, double b1y,
        double b2x,
        double b2y)
    {
        Coordinate a1 = new Coordinate(a1x, a1y);
        Coordinate a2 = new Coordinate(a2x, a2y);
        Coordinate b1 = new Coordinate(b1x, b1y);
        Coordinate b2 = new Coordinate(b2x, b2y);

        var res =
            _intersector.GetSegmentIntersection(a1, a2, b1, b2);

        Assert.Equal((Intersector.IntersectionType.TangentIntersection, new Coordinate(-0.5, 0.8)), res);
    }

    [Theory]
    [InlineData(-2, 1, 1, 2, 0, 1, -1, 2)]
    [InlineData(-2, 1, 1, 2, -1, 2, 0, 1)]
    [InlineData(-2, 1, 1, 2, -1.5, 0.5, 0.5, 2.5)]
    [InlineData(-2, 1, 1, 2, 0.5, 2.5, -1.5, 0.5)]
    [InlineData(1, 2, -2, 1, 0, 1, -1, 2)]
    [InlineData(1, 2, -2, 1, -1, 2, 0, 1)]
    [InlineData(1, 2, -2, 1, -1.5, 0.5, 0.5, 2.5)]
    [InlineData(1, 2, -2, 1, 0.5, 2.5, -1.5, 0.5)]
    private void InnerIntersectionTest(double a1x, double a1y, double a2x, double a2y, double b1x, double b1y,
        double b2x,
        double b2y)
    {
        Coordinate a1 = new Coordinate(a1x, a1y);
        Coordinate a2 = new Coordinate(a2x, a2y);
        Coordinate b1 = new Coordinate(b1x, b1y);
        Coordinate b2 = new Coordinate(b2x, b2y);

        var res =
            _intersector.GetSegmentIntersection(a1, a2, b1, b2);

        Assert.Equal((Intersector.IntersectionType.InnerIntersection, new Coordinate(-0.5, 1.5)), res);
    }
}