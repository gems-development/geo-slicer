using System.Collections.Generic;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.GridTests;

public class FactTestsOverlayWeilerAtherton
{
    private static readonly double Epsilon = 1E-9;

    private static readonly LineService LineService = new LineService(Epsilon);

    private static readonly GridSlicer.GridSlicerHelper SlicerHelper =
        new(new LinesIntersector(new EpsilonCoordinateComparator(Epsilon), LineService, Epsilon), Epsilon, LineService,
            new EpsilonCoordinateComparator());

    [Fact]
    public void CuttingInClippedByClock()
    {
        //Arrange
        List<Coordinate> clipped = new List<Coordinate>()
        {
            new (-4, 2), new (0, 2), new (4, 4), new (2, 0),
            new (2, -4), new (-2, -4), new (-2, 0), new (-4, 2)
        };
        List<Coordinate> cutting = new List<Coordinate>()
        {
            new (-2, 2), new (2, 2), new (2, -2), new (-2, -2), new (-2, 2)
        };
        List<List<Coordinate>> expected = new List<List<Coordinate>>()
        {
            new List<Coordinate>(){
                new (-2, 2), new (0, 2), new (2, 2), new (2, 0),
                new (2, -2), new (-2, -2), new (-2, 0), new (-2, 2)
            }
        };
        
        //Act
        var actual = SlicerHelper.WeilerAtherton(clipped, cutting);

        //Assert
        Assert.Equal(expected,actual);
        Assert.Single(actual);
       // Assert.True(actual[0].IsEqualsRing(expected));
    }

    [Fact]
    public void ClippedInCuttingByClock()
    {
        //Arrange
        List<Coordinate> clipped = new List<Coordinate>()
        {
            new (-2, 2), new (2, 2), new (2, -2), new (-2, -2), new (-2, 2)
        };
        List<Coordinate> cutting = new List<Coordinate>()
        {
            new (-4, 2), new (0, 2), new (4, 4), new (2, 0),
            new (2, -4), new (-2, -4), new (-2, 0), new (-4, 2)
        };
        List<List<Coordinate>> expected = new List<List<Coordinate>>()
        {
            new List<Coordinate>(){
                new (-2, 2), new (0, 2), new (2, 2), new (2, 0),
                new (2, -2), new (-2, -2), new (-2, 0), new (-2, 2)
            }
        };
        
        //Act
        var actual = SlicerHelper.WeilerAtherton(clipped, cutting);

        //Assert
        Assert.Equal(expected,actual);
        Assert.Single(actual);
        //Assert.True(actual[0].IsEqualsRing(expected));
    }

    [Fact]
    public void CuttingInClippedNoByClock()
    {
        //Arrange
        List<Coordinate> clipped = new List<Coordinate>()
        {
            new (2, -4), new (2, 0), new (4, 4), new (0, 2),
            new (-4, 2), new (-4, -2), new (0, -2), new (2, -4),
        };
        List<Coordinate> cutting = new List<Coordinate>()
        {
            new (2, -2), new (2, 2), new (-2, 2), new (-2, -2), new (2, -2)
        };
        List<List<Coordinate>> expected = new List<List<Coordinate>>()
        {
            new List<Coordinate>(){
                new (2, -2), new (2, 0), new (2, 2), new (0, 2),
                new (-2, 2), new (-2, -2), new (0, -2), new (2, -2)
            }
        };
        
        //Act
        var actual = SlicerHelper.WeilerAtherton(clipped, cutting);

        //Assert
        Assert.Equal(expected,actual);
        Assert.Single(actual);
        //Assert.True(actual[0].IsEqualsRing(expected));
    }

    [Fact]
    public void ClippedInCuttingNoByClock()
    {
        //Arrange
        List<Coordinate> clipped = new List<Coordinate>()
        {
            new (2, -2), new (2, 2), new (-2, 2), new (-2, -2), new (2, -2)
        };
        List<Coordinate> cutting = new List<Coordinate>()
        {
            new (2, -4), new (2, 0), new (4, 4), new (0, 2),
            new (-4, 2), new (-4, -2), new (0, -2), new (2, -4),
        };
        List<List<Coordinate>> expected = new List<List<Coordinate>>()
        {
            new List<Coordinate>(){
                new (2, -2), new (2, 0), new (2, 2), new (0, 2),
                new (-2, 2), new (-2, -2), new (0, -2), new (2, -2)
            }
        };
        
        //Act
        var actual = SlicerHelper.WeilerAtherton(clipped, cutting);

        //Assert
        Assert.Equal(expected,actual);
        Assert.Single(actual);
        //Assert.True(actual[0].IsEqualsRing(expected));
    }

    [Fact]
    public void TangentClippedUnderAndRighterThanCutting()
    {
        //Arrange
        List<Coordinate> clipped = new List<Coordinate>()
        {
            new (-2, 1), new (-1, 2), new (0, 1), new (-2, 1)
        };
        List<Coordinate> cutting = new List<Coordinate>()
        {
            new (-1, 1), new (1, 1), new (0, 0), new (-1, 1)
        };
        List<List<Coordinate>> expected = new List<List<Coordinate>>()
        {
            new List<Coordinate>(){
                new (-1, 1), new (0, 1), new (-1, 1)
            }
        };
        
        //Act
        var actual = SlicerHelper.WeilerAtherton(clipped, cutting);

        //Assert
        //Assert.Equal(expected,actual);
        Assert.Single(actual);
        Assert.True(actual[0].IsEqualsRing(expected[0]));
    }

    [Fact]
    public void TangentCuttingUnderAndRighterThanClipped()
    {
        //Arrange
        List<Coordinate> clipped = new List<Coordinate>()
        {
            new (-1, 1), new (1, 1), new (0, 0), new (-1, 1)
        };
        List<Coordinate> cutting = new List<Coordinate>()
        {
            new (-2, 1), new (-1, 2), new (0, 1), new (-2, 1)
        };
        List<List<Coordinate>> expected = new List<List<Coordinate>>()
        {
            new List<Coordinate>(){
                new (-1, 1), new (0, 1), new (-1, 1)
            }
        };
        
        //Act
        var actual = SlicerHelper.WeilerAtherton(clipped, cutting);

        //Assert
        //Assert.Equal(expected,actual);
        Assert.Single(actual);
        Assert.True(actual[0].IsEqualsRing(expected[0]));
    }

    [Fact]
    public void TangentClippedUnderAndLefterThanCutting()
    {
        //Arrange
        LinearRing clipped = new LinearRing()
        {
            new (-1, 1), new (1, 1), new (0, 0), new (-1, 1)
        };
        List<Coordinate> cutting = new List<Coordinate>()
        {
            new (0, 1), new (1, 2), new (2, 1), new (0, 1)
        };
        List<List<Coordinate>> expected = new List<List<Coordinate>>()
        {
            new List<Coordinate>(){
                new (1, 1), new (0, 1), new (1, 1)
            }
        };
        
        //Act
        var actual = SlicerHelper.WeilerAtherton(clipped, cutting);

        //Assert
        //Assert.Equal(expected,actual);
        Assert.Single(actual);
        Assert.True(actual[0].IsEqualsRing(expected[0]));
    }

    [Fact]
    public void TangentCuttingUnderAndLefterThanClipped()
    {
        //Arrange
        List<Coordinate> clipped = new List<Coordinate>()
        {
            new (0, 1), new (1, 2), new (2, 1), new (0, 1)
        };
        List<Coordinate> cutting = new List<Coordinate>()
        {
            new (-1, 1), new (1, 1), new (0, 0), new (-1, 1)
        };
        List<List<Coordinate>> expected = new List<List<Coordinate>>()
        {
            new List<Coordinate>(){
                new (1, 1), new (0, 1), new (1, 1)
            }
        };
        
        //Act
        var actual = SlicerHelper.WeilerAtherton(clipped, cutting);

        //Assert
        //Assert.Equal(expected,actual);
        Assert.Single(actual);
        Assert.True(actual[0].IsEqualsRing(expected[0]));
    }
}