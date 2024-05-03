using System.Collections.Generic;
using GeoSlicer.GridSlicer.Helpers;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.GridTests;

public class Tests
{
    private static readonly double Epsilon = 1E-9;

    private static readonly LineService LineService = new(Epsilon);

    private static readonly GridSlicerHelper SlicerHelper =
        new(new LinesIntersector(new EpsilonCoordinateComparator(Epsilon), LineService, Epsilon), LineService,
            new EpsilonCoordinateComparator(), new ContainsChecker(LineService, Epsilon));

    [Fact]
    public void First()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2, 4), new(0, 0), new(3, 0), new(5, -4), new(-2, -4), new(-2, 4)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(0, 0), new(3, 0), new(6, 4), new(6, -5), new(0, -5), new(0, 0)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(3, 0), new(5, -4), new(0, -4), new(0, 0), new(3, 0)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void Second()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2, 4), new(0, 0), new(3, 0), new(5, -4), new(-2, -4), new(-2, 4)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(0, 0), new(3, 0), new(5, 4), new(-3, 6), new(-3, -6), new(0, -6), new(0, 0)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(0, -4), new(-2, -4), new(-2, 4), new(0, 0), new(0, -4)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void Third()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2, 4), new(0, 0), new(6, 0), new(3, 4), new(-2, 4)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(0, 0), new(-2, -2), new(-2, 1), new(3, 6), new(3, 0), new(0, 0)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(1, 4), new(3, 4), new(3, 0), new(0, 0), new(-1, 2), new(1, 4)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void Forth()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2, 4), new(0, 0), new(6, 0), new(6, -3), new(-2, -3), new(-2, 4)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(0, 0), new(4, 0), new(4, -4), new(5, -4), new(5, 5), new(-2, 5), new(-4, 2), new(0, 0)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(4, 0), new(5, 0), new(5, -3), new(4, -3), new(4, 0)
            }),
            new LinearRing(new Coordinate[]
            {
                new(-2, 1), new(-2, 4), new(0, 0), new(-2, 1)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void Fifth()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2, 4), new(0, 0), new(4, 0), new(4, -2), new(-2, -2), new(-2, 4)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(0, 0), new(-3, -3), new(-3, 2), new(2, 2), new(2, 0), new(0, 0)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(-1, 2), new(0, 0), new(-2, -2), new(-2, 2), new(-1, 2)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
    }
}