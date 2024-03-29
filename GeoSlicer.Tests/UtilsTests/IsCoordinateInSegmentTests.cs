using GeoSlicer.Utils;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.UtilsTests;

public class IsCoordinateInSegmentTests
{
    private const double Epsilon = 1E-9;
    private readonly LineService _lineService = new LineService(Epsilon);

    [Theory]
    [InlineData(0, 0, -1, -1, 1, 1)]
    [InlineData(-0.2, 1.6, -1, 2, 1, 1)]
    private void InSegment(double ax, double ay, double bx, double by, double cx, double cy)
    {
        Coordinate a = new Coordinate(ax, ay);
        Coordinate b = new Coordinate(bx, by);
        Coordinate c = new Coordinate(cx, cy);

        bool actual = _lineService.IsCoordinateInSegment(a, b, c);
        Assert.True(actual);
    }

    [Theory]
    [InlineData(0, 0, -1.5, -1, 1, 1)]
    [InlineData(2, 2, -1, -1, 1, 1)]
    [InlineData(1.2, 0.9, -1, 2, 1, 1)]
    private void NotInSegment(double ax, double ay, double bx, double by, double cx, double cy)
    {
        Coordinate a = new Coordinate(ax, ay);
        Coordinate b = new Coordinate(bx, by);
        Coordinate c = new Coordinate(cx, cy);

        bool actual = _lineService.IsCoordinateInSegment(a, b, c);
        Assert.False(actual);
    }

    [Theory]
    [InlineData(2, 2, -1, -1, 1, 1)]
    [InlineData(1.2, 0.9, -1, 2, 1, 1)]
    [InlineData(0, 0, -1, -1, 1, 1)]
    private void AtLine(double ax, double ay, double bx, double by, double cx, double cy)
    {
        Coordinate a = new Coordinate(ax, ay);
        Coordinate b = new Coordinate(bx, by);
        Coordinate c = new Coordinate(cx, cy);

        bool actual = _lineService.IsCoordinateAtLine(a, b, c);
        Assert.True(actual);
    }

    [Theory]
    [InlineData(2, 1, -1, -1, 1, 1)]
    [InlineData(1.2, 0.8, -1, 2, 1.1, 1)]
    [InlineData(0, 0, -1.1, -1, 1, 1)]
    private void NotAtLine(double ax, double ay, double bx, double by, double cx, double cy)
    {
        Coordinate a = new Coordinate(ax, ay);
        Coordinate b = new Coordinate(bx, by);
        Coordinate c = new Coordinate(cx, cy);

        bool actual = _lineService.IsCoordinateAtLine(a, b, c);
        Assert.False(actual);
    }
}