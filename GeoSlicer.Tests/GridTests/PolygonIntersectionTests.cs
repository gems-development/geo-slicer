using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.PolygonClippingAlghorithm;

namespace GeoSlicer.Tests.GridTests;

public class PolygonIntersectionTests
{
    private static readonly double Epsilon = 1E-9;

    private static readonly LineService LineService = new(Epsilon);

    private static readonly WeilerAthertonAlghorithm SlicerHelper =
        new(new LinesIntersector(new EpsilonCoordinateComparator(Epsilon), LineService, Epsilon), LineService,
            new EpsilonCoordinateComparator(), new ContainsChecker(LineService, Epsilon));

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

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(4, 4), new(4, 2), new(3, 2), new(4, 4)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
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

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(8, 4), new(8, 2), new(5, 0), new(5, 4), new(8, 4)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
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

        List<LinearRing> expected = new()
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
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SimpleTestSpecialPointsInOneCuttingEdge()
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

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(3, 0), new(3, -3), new(0, -3), new(3, 0)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SimpleTestOverlayingEdges()
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

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(3, 4), new(4, 1), new(2, 1), new(2, 4), new(3, 4)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void OnlyOneCommonPoint()
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

        IEnumerable<LinearRing> expected = Enumerable.Empty<LinearRing>();

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void WithoutCommonPoints()
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

        IEnumerable<LinearRing> expected = Enumerable.Empty<LinearRing>();

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CuttingInClippedWithCommonSegments()
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

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(1, -1), new(2, -1), new(2, -2), new(1, -2), new(1, -1)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CuttingInClippedWithoutIntersections()
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

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(0, 0), new(0, 1), new(1, 1), new(1, 0), new(0, 0)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CornerIntersection()
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

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(0, 0), new(0, 2), new(2, 2), new(2, -2), new(0, -2), new(-1, -1), new(0, 0)
            })
        };


        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void MultipleCommonEdges()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-4, 2), new(4, 2), new(4, -1), new(1, -1), new(1, -2), new(-4, -2), new(-4, 2)
        };
        LinearRing clippedRing = new LinearRing(clipped);

        Coordinate[] cutting =
        {
            new(0, 1), new(0, 2), new(2, 2), new(2, 3), new(5, 3), new(5, 1), new(4, 1), new(4, 0),
            new(3, 0), new(2, 1), new(2, -2), new(-3, -2), new(-3, 3), new(-2, 3), new(0, 1)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(-3, 2), new(-1, 2), new(0, 1), new(0, 2), new(2, 2), new(4, 2), new(4, 1),
                new(4, 0), new(3, 0), new(2, 1), new(2, -1), new(1, -1),
                new(1, -2), new(-3, -2), new(-3, 2)
            })
        };


        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ZeroTunnelInIntersection()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2, 2), new(0, 2), new(2, 4), new(3, 4), new(3, 0), new(-2, 0), new(-2, 2)
        };
        LinearRing clippedRing = new LinearRing(clipped);

        Coordinate[] cutting =
        {
            new(1, 2), new(-2, 2), new(-2, 5), new(1, 5), new(1, 2)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(0, 2), new(1, 3), new(1, 2), new(0, 2)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ClippedAngleInsideCuttingAngle()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2, 0), new(-2, 3), new(0, 3), new(0, 0), new(-2, 0)
        };
        LinearRing clippedRing = new LinearRing(clipped);

        Coordinate[] cutting =
        {
            new(-3, 1), new(-2, 3), new(0, 4), new(1, 1), new(-3, 1)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(-2, 1), new(-2, 3), new(0, 3), new(0, 1), new(-2, 1)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Equal(expected, actual);
    }
}