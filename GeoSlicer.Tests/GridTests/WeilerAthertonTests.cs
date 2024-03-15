using NetTopologySuite.Geometries;
using System.Collections.Generic;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;

namespace GeoSlicer.Tests.GridTests;

public class WeilerAthertonTests
{
    private static readonly double Epsilon = 1E-9;

    private static readonly LineService LineService = new LineService(Epsilon);

    private static readonly GridSlicer.GridSlicer Slicer =
        new(new LineIntersector(new EpsilonCoordinateComparator(Epsilon), LineService, Epsilon), Epsilon, LineService);

    [Fact]
    public void SimpleTest()
    {
        //Arrange
        List<Coordinate> clipped = new List<Coordinate>()
        {
            new(-2, 1), new(-2, 7), new(4, 7), new(4, 1), new(-2, 1)
        };

        List<Coordinate> cutting = new List<Coordinate>()
        {
            new(3, 2), new(5, 6), new(8, 2), new(3, 2)
        };

        List<List<Coordinate>> expected = new List<List<Coordinate>>()
        {
            new List<Coordinate>()
            {
                new Coordinate(4, 4), new Coordinate(4, 2), new Coordinate(3, 2), new Coordinate(4, 4)
            }
        };

        //Act
        var figures = Slicer.WeilerAtherton(clipped, cutting);

        //Assert
        Assert.Equal(expected, figures);
    }

    [Fact]
    public void TestWhereNodeToENextEqualsNull()
    {
        //Arrange
        List<Coordinate> clipped = new List<Coordinate>()
        {
            new(8, 2), new(2, -2), new(-1, 2), new(1, 6), new(8, 6), new(8, 2)
        };

        List<Coordinate> cutting = new List<Coordinate>()
        {
            new(5, 4), new(10, 4), new(10, -2), new(5, -2), new(5, 4)
        };

        List<List<Coordinate>> expected = new List<List<Coordinate>>()
        {
            new List<Coordinate>()
            {
                new(8, 4), new(8, 2), new(5, 0), new(5, 4), new(8, 4)
            }
        };

        //Act
        var figures = Slicer.WeilerAtherton(clipped, cutting);

        //Assert
        Assert.Equal(expected, figures);
    }

    [Fact]
    public void TestTwoFiguresInResult()
    {
        //Arrange
        List<Coordinate> clipped = new List<Coordinate>()
        {
            new(-2,3), new(-4,3), new(-4,6), new(-2,9), new(6,9), new(6,4), new(1,4), new(1,6), new(-2,6), new(-2,3)
        };

        List<Coordinate> cutting = new List<Coordinate>()
        {
            new(-3,2), new(-3, 5), new(2, 5), new(2, 2), new(-3,2)
        };

        List<List<Coordinate>> expected = new List<List<Coordinate>>()
        {
            new List<Coordinate>() {
                new(2, 4), new(1, 4), new(1, 5), new(2,5), new(2, 4)
            },

            new List<Coordinate>()
            {
                new(-2,5), new(-2,3), new(-3,3), new(-3,5), new(-2,5)
            }
        };

        //Act
        var figures = Slicer.WeilerAtherton(clipped, cutting);

        //Assert
        Assert.Equal(expected, figures);
    }

    [Fact]
    public void SimpleTestWithFirstPodkovirka()
    {
        //Arrange
        List<Coordinate> clipped = new List<Coordinate>()
        {
            new(3, 3), new(3, -3), new(-3, -3), new(-3, 3), new(3, 3)
        };

        List<Coordinate> cutting = new List<Coordinate>()
        {
            new(-1, -4), new(4, 1), new(4, -4), new(-1, -4)
        };

        List<List<Coordinate>> expected = new List<List<Coordinate>>()
        {
            new List<Coordinate>() {
                new(3, 0), new(3, -3), new(0, -3), new(3, 0)
            }
        };

        //Act
        var figures = Slicer.WeilerAtherton(clipped, cutting);

        //Assert
        Assert.Equal(expected, figures);
    }

    [Fact]
    public void SimpleTestWithUltraPodkovirka()
    {
        //Arrange
        List<Coordinate> clipped = new List<Coordinate>()
        {
            new(4, 1), new(-1, -2), new(-1, 4), new(3, 4), new(4, 1)
        };

        List<Coordinate> cutting = new List<Coordinate>()
        {
            new(4, 1), new(2, 1), new(2, 4), new(4, 4), new(4, 1)
        };

        List<List<Coordinate>> expected = new List<List<Coordinate>>()
        {
            new List<Coordinate>() {
                new(2,4), new(3,4), new(4,1), new(2,1), new(2,4)
            }
        };

        //Act
        var figures = Slicer.WeilerAtherton(clipped, cutting);

        //Assert
        Assert.Equal(expected, figures);
    }

    [Fact]
    public void TestWithOnlyOneCommonPoints()
    {
        //Arrange
        List<Coordinate> clipped = new List<Coordinate>()
        {
            new(-2, 2), new(0, 2), new(0, 0), new(-2, 0), new(-2, 2)
        };

        List<Coordinate> cutting = new List<Coordinate>()
        {
            new(0, 0), new(2, 0), new(2, -2), new(0, -2), new(0, 0)
        };

        List<List<Coordinate>> expected = new List<List<Coordinate>>()
        {
           /* new List<Coordinate>() {
            }*/
        };

        //Act
        var figures = Slicer.WeilerAtherton(clipped, cutting);

        //Assert
        Assert.Equal(expected, figures);
    }

    [Fact]
    public void TestWithoutCommonPoints()
    {
        //Arrange
        List<Coordinate> clipped = new List<Coordinate>()
        {
            new(-2, 2), new(0, 2), new(0, 0), new(-2, 0), new(-2, 2)
        };

        List<Coordinate> cutting = new List<Coordinate>()
        {
            new(1, -1), new(2, -1), new(2, -2), new(1, -2), new(1, -1)
        };

        List<List<Coordinate>> expected = new List<List<Coordinate>>()
        {
            
        };

        //Act
        var figures = Slicer.WeilerAtherton(clipped, cutting);

        //Assert
        Assert.Equal(expected, figures);
    }

    [Fact]
    public void TestCuttingInClippedWithCommonSegments()
    {
        //Arrange
        List<Coordinate> clipped = new List<Coordinate>()
        {
            new(0, 0), new(2, 0), new(2, -2), new(0, -2), new(0, 0)
        };

        List<Coordinate> cutting = new List<Coordinate>()
        {
            new(1, -1), new(2, -1), new(2, -2), new(1, -2), new(1, -1)
        };

        List<List<Coordinate>> expected = new List<List<Coordinate>>()
        {
            new List<Coordinate>() {
                new(2, -1), new(2, -2), new(1, -2), new(1, -1), new(2, -1)
            }
        };

        //Act
        var figures = Slicer.WeilerAtherton(clipped, cutting);

        //Assert
        Assert.Equal(expected, figures);
    }

    [Fact]
    public void TestCuttingInClippedWothoutIntersections()
    {
        //Arrange
        List<Coordinate> clipped = new List<Coordinate>()
        {
            new(-1, -1), new(-1, 2), new(2, 2), new(2, -1), new(-1, -1)
        };

        List<Coordinate> cutting = new List<Coordinate>()
        {
            new(0, 0), new(0, 1), new(1, 1), new(1, 0), new(0, 0)
        };

        List<List<Coordinate>> expected = new List<List<Coordinate>>()
        {
            new List<Coordinate>() {
                new(0, 0), new(0, 1), new(1, 1), new(1, 0), new(0, 0)
            }
        };

        //Act
        var figures = Slicer.WeilerAtherton(clipped, cutting);

        //Assert
        Assert.Equal(expected, figures);
    }
}