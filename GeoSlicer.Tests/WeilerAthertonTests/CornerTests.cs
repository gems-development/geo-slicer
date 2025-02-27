using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.PolygonClippingAlghorithm;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.WeilerAthertonTests;

public class CornerTests
{
    private static readonly double Epsilon = 1E-9;

    private static readonly LineService LineService = new(Epsilon, new EpsilonCoordinateComparator(Epsilon));

    private static readonly WeilerAthertonAlghorithm SlicerHelper =
        new(new LinesIntersector(new EpsilonCoordinateComparator(Epsilon), LineService, Epsilon), LineService,
            new EpsilonCoordinateComparator(), new ContainsChecker(LineService, Epsilon), Epsilon);

    [Fact]
    public void NonConvexNonConvex()
    {
        //Arrange
        Coordinate[] figureA =
        {
            new(-2, -1), new(-2, 2), new(2, 2), new(2, -2), new(0, 0), new(-2, -1)
        };
        LinearRing ringA = new LinearRing(figureA);
        Coordinate[] figureB =
        {
            new(0, 0), new(-4, 2), new(4, 5), new(4, -5), new(0, -5), new(0, 0)
        };
        LinearRing ringB = new LinearRing(figureB);

        List<LinearRing> expectedB = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(0, 0), new(-2, 1), new(-2, 2), new(2, 2), new(2, -2), new(0, 0)
            })
        };
        List<LinearRing> expectedA = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(-2, 1), new(-2, 2), new(2, 2), new(2, -2), new(0, 0), new(-2, 1)
            })
        };

        //Act
        var actualA = SlicerHelper.WeilerAtherton(new Polygon(ringA), ringB).Select(polygon => polygon.Shell);
        var actualB = SlicerHelper.WeilerAtherton(new Polygon(ringB), ringA).Select(polygon => polygon.Shell);

        //Assert
        Assert.Equal(expectedA, actualA);
        Assert.Equal(expectedB, actualB);
    }


    [Fact]
    public void NonConvexConvex()
    {
        //Arrange
        Coordinate[] figureA =
        {
            new(-3, 0), new(-3, 2), new(2, 2), new(2, -3), new(0, 0), new(-3, 0)
        };
        LinearRing ringA = new LinearRing(figureA);
        Coordinate[] figureB =
        {
            new(0, 0), new(-4, -5), new(-4, 4), new(0, 0)
        };
        LinearRing ringB = new LinearRing(figureB);

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(0, 0), new(-3, 0), new(-3, 2), new(-2, 2), new(0, 0)
            })
        };

        //Act
        var actualA = SlicerHelper.WeilerAtherton(new Polygon(ringA), ringB).Select(polygon => polygon.Shell);
        var actualB = SlicerHelper.WeilerAtherton(new Polygon(ringB), ringA).Select(polygon => polygon.Shell);

        //Assert
        Assert.True(expected.IsEqualsRingCollection(actualA));
        Assert.True(expected.IsEqualsRingCollection(actualB));
    }

    [Fact]
    public void ConvexNonConvex()
    {
        //Arrange
        Coordinate[] figureA =
        {
            new(0, 0), new(3, -3), new(-2, -3), new(0, 0)
        };
        LinearRing ringA = new LinearRing(figureA);
        Coordinate[] figureB =
        {
            new(-4, 0), new(4, 3), new(4, -6), new(0, -6), new(0, 0), new(-4, 0)
        };
        LinearRing ringB = new LinearRing(figureB);

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(0, 0), new(3, -3), new(0, -3), new(0, 0)
            })
        };

        //Act
        var actualA = SlicerHelper.WeilerAtherton(new Polygon(ringA), ringB).Select(polygon => polygon.Shell);
        var actualB = SlicerHelper.WeilerAtherton(new Polygon(ringB), ringA).Select(polygon => polygon.Shell);

        //Assert
        Assert.True(expected.IsEqualsRingCollection(actualA));
        Assert.True(expected.IsEqualsRingCollection(actualB));
    }

    [Fact]
    public void ConvexConvex()
    {
        //Arrange
        Coordinate[] figureA =
        {
            new(0, 0), new(-2, -3), new(-2, -1), new(0, 0)
        };
        LinearRing ringA = new LinearRing(figureA);
        Coordinate[] figureB =
        {
            new(0, 0), new(-3, -3), new(-3, 4), new(0, 0)
        };
        LinearRing ringB = new LinearRing(figureB);

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(-2, -2), new(-2, -1), new(0, 0), new(-2, -2)
            })
        };

        //Act
        var actualA = SlicerHelper.WeilerAtherton(new Polygon(ringA), ringB).Select(polygon => polygon.Shell);
        var actualB = SlicerHelper.WeilerAtherton(new Polygon(ringB), ringA).Select(polygon => polygon.Shell);
        
        //Assert
        Assert.True(expected.IsEqualsRingCollection(actualA));
        Assert.True(expected.IsEqualsRingCollection(actualB));
    }
}