using System.Collections.Generic;
using GeoSlicer.GridSlicer.Helpers;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.GridTests;

public class EqualEdgeTests
{
    private static readonly double Epsilon = 1E-9;

    private static readonly LineService LineService = new(Epsilon);

    private static readonly GridSlicerHelper SlicerHelper =
        new(new LinesIntersector(new EpsilonCoordinateComparator(Epsilon), LineService, Epsilon), LineService,
            new EpsilonCoordinateComparator(), new ContainsChecker(LineService, Epsilon));
    
    [Fact]
    public void EdgesCuttingRighterLefterFirst()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(0,0), new(3,0), new(5,-4), new(-2,-4), new(-2,4), new(0,0)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(0, 0), new(3,0), new(3,-4), new(-3,-4), new(-3,2), new(0,2), new(0,0) 
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(-2,-4), new(-2,2), new(-1,2), new(0,0), new(3,0), new(3,-4), new(-2,-4)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void EdgesCuttingRighterLefterSecond()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(0,0), new(3,0), new(5,-4), new(-2,-4), new(-2,4), new(0,0)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(0, 0), new(0,3), new(5,3), new(5,-2), new(3,-2), new(3,0), new(0,0) 
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(3,0), new(4,-2), new(3,-2), new(3,0)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void EdgesCuttingRighterLefterThird()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(0,0), new(-2,4), new(4,4), new(4,-4), new(2,0), new(0,0)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(0, 0), new(0,4), new(3,4), new(3,-2), new(2,-2), new(2,0), new(0,0) 
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(3,-2), new(2,0), new(0,0), new(0,4), new(3,4), new(3,-2)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void EdgesCuttingRighterRighterFirst()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(0,0), new(2,0), new(4,-4), new(-2,-4), new(-2,4), new(0,0)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(0, 0), new(0,4), new(4,4), new(4,-2), new(2,0), new(0,0) 
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Empty(actual);
    }
    
    [Fact]
    public void EdgesCuttingRighterRighterSecond()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(0,0), new(-2,4), new(4,4), new(4,-4), new(2,0), new(0,0)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(0, 0), new(0,4), new(5,4), new(5,-1), new(3,-1), new(2,0), new(0,0) 
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(4,4), new(4,-1), new(3,-1), new(2,0), new(0,0), new(0,4), new(4,4)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
    }
    
}