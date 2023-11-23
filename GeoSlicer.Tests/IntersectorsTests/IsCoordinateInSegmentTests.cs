using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.IntersectorsTests;

public class IsCoordinateInSegmentTests
{
    private const double Delta = 1E-9;
    private readonly Intersector _intersector = new Intersector(new EpsilonCoordinateComparator(Delta), Delta);

    [Theory]
    [InlineData(-1, 0, -1, 1, 0, 2)]
    private void FalseTest(double ax, double ay, double bx, double by, double cx, double cy)
    {
        Coordinate a = new Coordinate(ax, ay);
        Coordinate b = new Coordinate(bx, by);
        Coordinate c = new Coordinate(cx, cy);

        bool actual = _intersector.IsCoordinatesAtOneLine(a, b, c);
        Assert.False(actual);
    }

    [Theory]
    [InlineData(0, 1, 0, 2, 0, 3)]
    [InlineData(1, 0, 2, 0, 3, 0)]
    [InlineData(1, 1, 2, 2, 3, 3)]
    private void TrueTest(double ax, double ay, double bx, double by, double cx, double cy)
    {
        Coordinate a = new Coordinate(ax, ay);
        Coordinate b = new Coordinate(bx, by);
        Coordinate c = new Coordinate(cx, cy);

        bool actual = _intersector.IsCoordinatesAtOneLine(a, b, c);
        
        Assert.True(actual);
    }

}