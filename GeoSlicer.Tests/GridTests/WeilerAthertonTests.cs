using NetTopologySuite.Geometries;
using System.Collections.Generic;
using GeoSlicer.GridSlicer.Helpers;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;

namespace GeoSlicer.Tests.GridTests;

public class WeilerAthertonTests
{
    private static readonly double Epsilon = 1E-9;

    private static readonly LineService LineService = new LineService(Epsilon);

    private static readonly GridSlicerHelper SlicerHelper =
        new(new LinesIntersector(new EpsilonCoordinateComparator(Epsilon), LineService, Epsilon), Epsilon, LineService,
            new EpsilonCoordinateComparator());

    [Fact]
    public void SimpleTest()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2, 1), new(-2, 7), new(4, 7), new(4, 1), new(-2, 1)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(3, 2), new(5, 6), new(8, 2), new(3, 2)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new List<LinearRing>()
        {
            new LinearRing(new Coordinate[]
            {
                new Coordinate(4, 4), new Coordinate(4, 2), new Coordinate(3, 2), new Coordinate(4, 4)
            })
        };

        //Act
        var figures = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, figures);
    }

    [Fact]
    public void TestWhereNodeToENextEqualsNull()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(8, 2), new(2, -2), new(-1, 2), new(1, 6), new(8, 6), new(8, 2)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(5, 4), new(10, 4), new(10, -2), new(5, -2), new(5, 4)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new List<LinearRing>()
        {
            new LinearRing(new Coordinate[]
            {
                new(8, 4), new(8, 2), new(5, 0), new(5, 4), new(8, 4)
            })
        };

        //Act
        var figures = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, figures);
    }

    [Fact]
    public void TestTwoFiguresInResult()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2, 3), new(-4, 3), new(-4, 6), new(-2, 9), new(6, 9), new(6, 4), new(1, 4), new(1, 6), new(-2, 6),
            new(-2, 3)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(-3, 2), new(-3, 5), new(2, 5), new(2, 2), new(-3, 2)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new List<LinearRing>()
        {
            new LinearRing(new Coordinate[]
            {
                new(2, 4), new(1, 4), new(1, 5), new(2, 5), new(2, 4)
            }),

            new LinearRing(new Coordinate[]
            {
                new(-2, 5), new(-2, 3), new(-3, 3), new(-3, 5), new(-2, 5)
            })
        };

        //Act
        var figures = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, figures);
    }

    [Fact]
    public void SimpleTestWithFirstPodkovirka()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(3, 3), new(3, -3), new(-3, -3), new(-3, 3), new(3, 3)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(-1, -4), new(4, 1), new(4, -4), new(-1, -4)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new List<LinearRing>()
        {
            new LinearRing(new Coordinate[]
            {
                new(3, 0), new(3, -3), new(0, -3), new(3, 0)
            })
        };

        //Act
        var figures = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, figures);
    }

    [Fact]
    public void SimpleTestWithUltraPodkovirka()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(4, 1), new(-1, -2), new(-1, 4), new(3, 4), new(4, 1)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(4, 1), new(2, 1), new(2, 4), new(4, 4), new(4, 1)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new List<LinearRing>()
        {
            new LinearRing(new Coordinate[]
            {
                new(2, 4), new(3, 4), new(4, 1), new(2, 1), new(2, 4)
            })
        };

        //Act
        var figures = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, figures);
    }

    [Fact]
    public void TestWithOnlyOneCommonPoints()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2, 2), new(0, 2), new(0, 0), new(-2, 0), new(-2, 2)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(0, 0), new(2, 0), new(2, -2), new(0, -2), new(0, 0)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new List<LinearRing>()
        {
            /* new List<Coordinate>() {
             }*/
        };

        //Act
        var figures = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, figures);
    }

    [Fact]
    public void TestWithoutCommonPoints()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2, 2), new(0, 2), new(0, 0), new(-2, 0), new(-2, 2)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(1, -1), new(2, -1), new(2, -2), new(1, -2), new(1, -1)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new List<LinearRing>()
        {
        };

        //Act
        var figures = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, figures);
    }

    [Fact]
    public void TestCuttingInClippedWithCommonSegments()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(0, 0), new(2, 0), new(2, -2), new(0, -2), new(0, 0)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(1, -1), new(2, -1), new(2, -2), new(1, -2), new(1, -1)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new List<LinearRing>()
        {
            new LinearRing(new Coordinate[]
            {
                new(2, -1), new(2, -2), new(1, -2), new(1, -1), new(2, -1)
            })
        };

        //Act
        var figures = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, figures);
    }

    [Fact]
    public void TestCuttingInClippedWithoutIntersections()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-1, -1), new(-1, 2), new(2, 2), new(2, -1), new(-1, -1)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(0, 0), new(0, 1), new(1, 1), new(1, 0), new(0, 0)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new List<LinearRing>()
        {
            new LinearRing(new Coordinate[]
            {
                new(0, 0), new(0, 1), new(1, 1), new(1, 0), new(0, 0)
            })
        };

        //Act
        var figures = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, figures);
    }

    [Fact]
    public void CornerCornerTest()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2, 0), new(0, 0), new(0, 3), new(2, 3), new(2, -4), new(-2, 0)
        };
        LinearRing clippedRing = new LinearRing(clipped);

        Coordinate[] cutting =
        {
            new(-2, -2), new(0, 0), new(-2, 2), new(3, 2), new(3, -2), new(-2, -2)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new List<LinearRing>()
        {
            new LinearRing(new Coordinate[]
            {
                new(0, 0), new(0, 2), new(2, 2), new(2, -2), new(-1, -1), new(0, 0)
            })
        };


        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
    }
}