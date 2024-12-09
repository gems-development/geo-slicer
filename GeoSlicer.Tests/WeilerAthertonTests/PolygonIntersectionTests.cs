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

    private static readonly LineService LineService = new(Epsilon, new EpsilonCoordinateComparator(Epsilon));

    private static readonly WeilerAthertonAlghorithm SlicerHelper =
        new(new LinesIntersector(new EpsilonCoordinateComparator(Epsilon), LineService, Epsilon), LineService,
            new EpsilonCoordinateComparator(), new ContainsChecker(LineService, Epsilon), Epsilon);

    [Fact]
    public void SimpleTest()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2, 1), new(-2, 7), new(4, 7), new(4, 1), new(-2, 1)
        };
        Polygon clippedPolygon = new Polygon(new LinearRing(clipped));
        Coordinate[] cutting =
        {
            new(3, 2), new(5, 6), new(8, 2), new(3, 2)
        };
        Polygon cuttingPolygon = new Polygon(new LinearRing(cutting));

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(4, 4), new(4, 2), new(3, 2), new(4, 4)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingPolygon).Select(polygon => polygon.Shell).ToList();

        //Assert
        Assert.True(expected.IsEqualsRingCollection(actual));
    }

    [Fact]
    public void TestWhereNodeToENextEqualsNull()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(8, 2), new(2, -2), new(-1, 2), new(1, 6), new(8, 6), new(8, 2)
        };
        Polygon clippedPolygon = new Polygon(new LinearRing(clipped));
        Coordinate[] cutting =
        {
            new(5, 4), new(10, 4), new(10, -2), new(5, -2), new(5, 4)
        };
        Polygon cuttingPolygon = new Polygon(new LinearRing(cutting));

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(8, 4), new(8, 2), new(5, 0), new(5, 4), new(8, 4)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingPolygon).Select(polygon => polygon.Shell).ToList();

        //Assert
        Assert.True(expected.IsEqualsRingCollection(actual));
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
        Polygon clippedPolygon = new Polygon(new LinearRing(clipped));
        Coordinate[] cutting =
        {
            new(-3, 2), new(-3, 5), new(2, 5), new(2, 2), new(-3, 2)
        };
        Polygon cuttingPolygon = new Polygon(new LinearRing(cutting));

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
        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingPolygon).Select(polygon => polygon.Shell).ToList();

        //Assert
        Assert.True(expected.IsEqualsRingCollection(actual));
    }

    [Fact]
    public void SimpleTestSpecialPointsInOneCuttingEdge()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(3, 3), new(3, -3), new(-3, -3), new(-3, 3), new(3, 3)
        };
        Polygon clippedPolygon = new Polygon(new LinearRing(clipped));
        Coordinate[] cutting =
        {
            new(-1, -4), new(4, 1), new(4, -4), new(-1, -4)
        };
        Polygon cuttingPolygon = new Polygon(new LinearRing(cutting));

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(3, 0), new(3, -3), new(0, -3), new(3, 0)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingPolygon).Select(polygon => polygon.Shell).ToList();

        //Assert
        Assert.True(expected.IsEqualsRingCollection(actual));
    }

    [Fact]
    public void SimpleTestOverlayingEdges()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(4, 1), new(-1, -2), new(-1, 4), new(3, 4), new(4, 1)
        };
        Polygon clippedPolygon = new Polygon(new LinearRing(clipped));
        Coordinate[] cutting =
        {
            new(4, 1), new(2, 1), new(2, 4), new(4, 4), new(4, 1)
        };
        Polygon cuttingPolygon = new Polygon(new LinearRing(cutting));

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(3, 4), new(4, 1), new(2, 1), new(2, 4), new(3, 4)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingPolygon).Select(polygon => polygon.Shell).ToList();

        //Assert
        Assert.True(expected.IsEqualsRingCollection(actual));
    }

    [Fact]
    public void OnlyOneCommonPoint()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2, 2), new(0, 2), new(0, 0), new(-2, 0), new(-2, 2)
        };
        Polygon clippedPolygon = new Polygon(new LinearRing(clipped));
        Coordinate[] cutting =
        {
            new(0, 0), new(2, 0), new(2, -2), new(0, -2), new(0, 0)
        };
        Polygon cuttingPolygon = new Polygon(new LinearRing(cutting));

        IEnumerable<LinearRing> expected = Enumerable.Empty<LinearRing>();

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingPolygon).Select(polygon => polygon.Shell).ToList();

        //Assert
        Assert.True(expected.IsEqualsRingCollection(actual));
    }

    [Fact]
    public void WithoutCommonPoints()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2, 2), new(0, 2), new(0, 0), new(-2, 0), new(-2, 2)
        };
        Polygon clippedPolygon = new Polygon(new LinearRing(clipped));
        Coordinate[] cutting =
        {
            new(1, -1), new(2, -1), new(2, -2), new(1, -2), new(1, -1)
        };
        Polygon cuttingPolygon = new Polygon(new LinearRing(cutting));

        IEnumerable<LinearRing> expected = Enumerable.Empty<LinearRing>();

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingPolygon).Select(polygon => polygon.Shell).ToList();

        //Assert
        Assert.True(expected.IsEqualsRingCollection(actual));
    }

    [Fact]
    public void CuttingInClippedWithCommonSegments()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(0, 0), new(2, 0), new(2, -2), new(0, -2), new(0, 0)
        };
        Polygon clippedPolygon = new Polygon(new LinearRing(clipped));
        Coordinate[] cutting =
        {
            new(1, -1), new(2, -1), new(2, -2), new(1, -2), new(1, -1)
        };
        Polygon cuttingPolygon = new Polygon(new LinearRing(cutting));

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(1, -1), new(2, -1), new(2, -2), new(1, -2), new(1, -1)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingPolygon).Select(polygon => polygon.Shell).ToList();

        //Assert
            Assert.True(expected.IsEqualsRingCollection(actual));
    }

    [Fact]
    public void CuttingInClippedWithoutIntersections()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-1, -1), new(-1, 2), new(2, 2), new(2, -1), new(-1, -1)
        };
        Polygon clippedPolygon = new Polygon(new LinearRing(clipped));
        Coordinate[] cutting =
        {
            new(0, 0), new(0, 1), new(1, 1), new(1, 0), new(0, 0)
        };
        Polygon cuttingPolygon = new Polygon(new LinearRing(cutting));

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(0, 0), new(0, 1), new(1, 1), new(1, 0), new(0, 0)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingPolygon).Select(polygon => polygon.Shell).ToList();

        //Assert
        Assert.True(expected.IsEqualsRingCollection(actual));
    }

    [Fact]
    public void CornerIntersection()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2, 0), new(0, 0), new(0, 3), new(2, 3), new(2, -4), new(-2, 0)
        };
        Polygon clippedPolygon = new Polygon(new LinearRing(clipped));

        Coordinate[] cutting =
        {
            new(-2, -2), new(0, 0), new(-2, 2), new(3, 2), new(3, -2), new(-2, -2)
        };
        Polygon cuttingPolygon = new Polygon(new LinearRing(cutting));

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(0, 0), new(0, 2), new(2, 2), new(2, -2), new(0, -2), new(-1, -1), new(0, 0)
            })
        };


        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingPolygon).Select(polygon => polygon.Shell).ToList();

        //Assert
        Assert.True(expected.IsEqualsRingCollection(actual));
    }

    [Fact]
    public void MultipleCommonEdges()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-4, 2), new(4, 2), new(4, -1), new(1, -1), new(1, -2), new(-4, -2), new(-4, 2)
        };
        Polygon clippedPolygon = new Polygon(new LinearRing(clipped));

        Coordinate[] cutting =
        {
            new(0, 1), new(0, 2), new(2, 2), new(2, 3), new(5, 3), new(5, 1), new(4, 1), new(4, 0),
            new(3, 0), new(2, 1), new(2, -2), new(-3, -2), new(-3, 3), new(-2, 3), new(0, 1)
        };
        Polygon cuttingPolygon = new Polygon(new LinearRing(cutting));

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
        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingPolygon).Select(polygon => polygon.Shell).ToList();

        //Assert
        Assert.True(expected.IsEqualsRingCollection(actual));
    }

    [Fact]
    public void ZeroTunnelInIntersection()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2, 2), new(0, 2), new(2, 4), new(3, 4), new(3, 0), new(-2, 0), new(-2, 2)
        };
        Polygon clippedPolygon = new Polygon(new LinearRing(clipped));

        Coordinate[] cutting =
        {
            new(1, 2), new(-2, 2), new(-2, 5), new(1, 5), new(1, 2)
        };
        Polygon cuttingPolygon = new Polygon(new LinearRing(cutting));

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(0, 2), new(1, 3), new(1, 2), new(0, 2)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingPolygon).Select(polygon => polygon.Shell).ToList();

        //Assert
        Assert.True(expected.IsEqualsRingCollection(actual));
    }

    [Fact]
    public void ClippedAngleInsideCuttingAngle()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2, 0), new(-2, 3), new(0, 3), new(0, 0), new(-2, 0)
        };
        Polygon clippedPolygon = new Polygon(new LinearRing(clipped));

        Coordinate[] cutting =
        {
            new(-3, 1), new(-2, 3), new(0, 4), new(1, 1), new(-3, 1)
        };
        Polygon cuttingPolygon = new Polygon(new LinearRing(cutting));

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(-2, 1), new(-2, 3), new(0, 3), new(0, 1), new(-2, 1)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingPolygon).Select(polygon => polygon.Shell).ToList();

        //Assert
        Assert.True(expected.IsEqualsRingCollection(actual));
    }
    
    [Fact]
    public void TwoResultFiguresTangentByOnePoint()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(2,0), new(-2,3), new(2,3), new(5,0), new(2,-2), new(-2,0), new(2,0)
        };
        Polygon clippedPolygon = new Polygon(new LinearRing(clipped));

        Coordinate[] cutting =
        {
            new(-2,5), new(2,5), new(2,-3), new(-2,-3), new(-2,5)
        };
        Polygon cuttingPolygon = new Polygon(new LinearRing(cutting));

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(2, 0), new(-2, 3), new(2, 3), new(2, 0)
            }),
            new LinearRing(new Coordinate[]
            {
                new(2, -2), new(-2, 0), new(2, 0), new(2, -2)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingPolygon).Select(polygon => polygon.Shell).ToList();
        //Assert
        Assert.True(expected.IsEqualsRingCollection(actual));
    }
    
    [Fact]
    public void FourResultFiguresTangentByThreePoints()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(0,0), new(-3,1), new(0,2), new(-3,3), new(0,4), new(4,0), new(3,-3), new(2,0), new(1,-3), new(0,0)
        };
        Polygon clippedPolygon = new Polygon(new LinearRing(clipped));

        Coordinate[] cutting =
        {
            new(-3,-3), new(-3,4), new(0,4), new(0,0), new(4,0), new(4,-3), new(-3,-3)
        };
        Polygon cuttingPolygon = new Polygon(new LinearRing(cutting));

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(0, 0), new(-3, 1), new(0, 2), new(0, 0)
            }),
            new LinearRing(new Coordinate[]
            {
                new(0, 2), new(-3, 3), new(0, 4), new(0, 2)
            }),
            new LinearRing(new Coordinate[]
            {
                new(4, 0), new(3, -3), new(2, 0), new(4, 0)
            }),
            new LinearRing(new Coordinate[]
            {
                new(2, 0), new(1, -3), new(0, 0), new(2, 0)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingPolygon).Select(polygon => polygon.Shell).ToList();
        //Assert
        Assert.True(expected.IsEqualsRingCollection(actual));
    }
    
    [Fact]
    public void ThreeResultFiguresTangentByTwoPoints()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-9,2), new(6,7), new(6,0), new(3,0), new(3,4), new(-9,2)
        };
        Polygon clippedPolygon = new Polygon(new LinearRing(clipped));

        Coordinate[] cutting =
        {
            new(9,2), new(-6,7), new(-6,0), new(-3,0), new(-3,4), new(9,2)
        };
        Polygon cuttingPolygon = new Polygon(new LinearRing(cutting));

        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(-6, 3), new(-3, 4), new(-3, 3), new(-6, 2.5), new(-6, 3)
            }),
            new LinearRing(new Coordinate[]
            {
                new(-3, 4), new(0, 5), new(3, 4), new(0, 3.5), new(-3, 4)
            }),
            new LinearRing(new Coordinate[]
            {
                new(6, 3), new(6, 2.5), new(3, 3), new(3, 4), new(6, 3)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingPolygon).Select(polygon => polygon.Shell).ToList();
        //Assert
        Assert.True(expected.IsEqualsRingCollection(actual));
    }
}