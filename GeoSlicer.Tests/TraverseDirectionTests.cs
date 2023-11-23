using NetTopologySuite.Geometries;
using GeoSlicer.NonConvexSlicer.Helpers;
using GeoSlicer.Utils;

namespace GeoSlicer.Tests;

public class TraverseDirectionTests
{
    [Fact]
    public void IsClockwiseBypassTriangleTest()
    {
        Coordinate[] coordinates = new Coordinate[] {
            new (3, 3), new (4, 1), new (2, 1), new (3, 3)};

        LinearRing ring = new LinearRing(coordinates);

        Assert.True(TraverseDirection.IsClockwiseBypass(ring));
    }

    [Fact]
    public void IsNotClockwiseBypassTriangleTest()
    {
        Coordinate[] coordinates = new Coordinate[] {
            new (4, 1), new (3, 3), new (2, 1), new (4, 1)};

        LinearRing ring = new LinearRing(coordinates);

        Assert.False(TraverseDirection.IsClockwiseBypass(ring));
    }

    [Fact]
    public void IsClockwiseBypassNineAngleTest()
    {
        Coordinate[] coordinates = new Coordinate[] {
            new (14, -1), new (9, -1), new (11, -3), new (2, -3), new (7, 2), new (-1, 1),
            new (-1, 2), new (1, 2), new (-1, 7), new (14, -1)};

        LinearRing ring = new LinearRing(coordinates);

        Assert.True(TraverseDirection.IsClockwiseBypass(ring));
    }

    [Fact]
    public void IsNotClockwiseBypassNineAngleTest()
    {
        Coordinate[] coordinates = new Coordinate[] {
            new (14, -1), new (-1, 7), new (1, 2), new (-1, 2), new (-1, 1),
            new (7, 2), new (2, -3), new (11, -3), new (9, -1), new (14, -1)};

        LinearRing ring = new LinearRing(coordinates);

        Assert.False(TraverseDirection.IsClockwiseBypass(ring));
    }

    [Fact]
    public void ChangeDirectionWithOddNumberOfPointsTest()
    {
        Coordinate[] coordinates = new Coordinate[] {
            new (14, -1), new (9, -1), new (11, -3), new (2, -3), new (7, 2), new (-1, 1),
            new (-1, 2), new (1, 2), new (-1, 7), new (14, -1)};

        LinearRing ring = new LinearRing(coordinates);

        Coordinate[] expectedCoordinates = new Coordinate[] {
            new (14, -1), new (-1, 7), new (1, 2), new (-1, 2), new (-1, 1),
            new (7, 2), new (2, -3), new (11, -3), new (9, -1), new (14, -1)};

        LinearRing expectedRing = new LinearRing(expectedCoordinates);

        TraverseDirection.ChangeDirection(ring);

        Assert.Equal(expectedRing, ring);
    }

    [Fact]
    public void ChangeDirectionWithEvenNumberOfPointsTest()
    {
        Coordinate[] coordinates = new Coordinate[] {
            new (14, -1), new (9, -1), new (11, -3), new (2, -3), new (7, 2), new (-1, 1),
            new (-1, 2), new (1, 2), new (14, -1)};

        LinearRing ring = new LinearRing(coordinates);

        Coordinate[] expectedCoordinates = new Coordinate[] {
            new (14, -1), new (1, 2), new (-1, 2), new (-1, 1),
            new (7, 2), new (2, -3), new (11, -3), new (9, -1), new (14, -1)};

        LinearRing expectedRing = new LinearRing(expectedCoordinates);

        TraverseDirection.ChangeDirection(ring);

        Assert.Equal(expectedRing, ring);
    }
}
        LinearRing ring = new LinearRing(coordinates);

        Coordinate[] expectedCoordinates =
        {
            new(14, -1), new(1, 2), new(-1, 2), new(-1, 1),
            new(7, 2), new(2, -3), new(11, -3), new(9, -1), new(14, -1)
        };

        LinearRing expectedRing = new LinearRing(expectedCoordinates);

        TraverseDirection.ChangeDirection(ring);

        Assert.Equal(expectedRing, ring);
    }
}