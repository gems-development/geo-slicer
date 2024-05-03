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
            new(0, 0), new(0,3), new(5,3), new(5,-1), new(3,-1), new(2,0), new(0,0) 
        };
        LinearRing cuttingRing = new LinearRing(cutting);
 
        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(4,3), new(4,-1), new(3,-1), new(2,0), new(0,0), new(0,3), new(4,3)
            })
        };
 
        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);
 
        //Assert
        Assert.Equal(expected, actual);
    }
 
    [Fact]
    public void EdgesCuttingLefterLefterFirst()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2,-2), new(-2,4), new(0,0), new(3,0), new(5,-2), new(-2,-2)
        };
        LinearRing clippedRing = new LinearRing(clipped);
 
        Coordinate[] cutting =
        {
            new(0, 0), new(3,0), new(3,-4), new(-3,-4), new(-3,3), new(0,0) 
        };
        LinearRing cuttingRing = new LinearRing(cutting);
 
        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(3,-2), new(-2,-2), new(-2,2), new(0,0), new(3,0), new(3,-2)
            })
        };
 
        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);
 
        //Assert
        Assert.Equal(expected, actual);
    }
 
    [Fact]
    public void EdgesCuttingLefterLefterSecond()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2,0), new(-2,4), new(4,4), new(4,-4), new(2,0), new(-2,0)
        };
        LinearRing clippedRing = new LinearRing(clipped);
 
        Coordinate[] cutting =
        {
            new(-2,0), new(-4,4), new(5,4), new(5,-4), new(2,-4), new(2,0), new(-2,0) 
        };
        LinearRing cuttingRing = new LinearRing(cutting);
 
        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(-2,0), new(-2,4), new(4,4), new(4,-4), new(2,0), new(-2,0)
            })
        };
 
        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);
 
        //Assert
        Assert.Equal(expected, actual);
    }
 
    [Fact]
    public void EdgesCuttingLefterLefterThird()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-1,4), new(0,0), new(2,0), new(4,-2), new(-1,-2), new(-1,4)
        };
        LinearRing clippedRing = new LinearRing(clipped);
 
        Coordinate[] cutting =
        {
            new(0,0), new(-2,4), new(4,4), new(4,-4), new(2,0), new(0,0)
        };
        LinearRing cuttingRing = new LinearRing(cutting);
 
        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(2,0), new(4,-2), new(3,-2), new(2,0) 
            }),
 
            new LinearRing(new Coordinate[]
            {
                new(-1,2), new(-1,4), new(0,0), new(-1,2)
            })
        };
 
        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);
 
        //Assert
        Assert.Equal(expected, actual);
    }
 
    [Fact]
    public void EdgesCuttingLefterRighterFirst()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2,0), new(2,0), new(2,-3), new(-3,-3), new(-3,4), new(-2,0)
        };
        LinearRing clippedRing = new LinearRing(clipped);
 
        Coordinate[] cutting =
        {
            new(-2,0), new(2,0), new(4,-3), new(-4,-3), new(-4,2), new(-2,0) 
        };
        LinearRing cuttingRing = new LinearRing(cutting);
 
        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(2,0), new(2,-3), new(-3,-3), new(-3,1), new(-2,0), new(2,0)
            })
        };
 
        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);
 
        //Assert
        Assert.Equal(expected, actual);
    }
 
    [Fact]
    public void EdgesCuttingLefterRighterSecond()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-5,-2), new(-2,4), new(-2,0), new(2,0), new(2,-2), new(-5,-2)
        };
        LinearRing clippedRing = new LinearRing(clipped);
 
        Coordinate[] cutting =
        {
            new(-2,0), new(-4,4), new(4,4), new(4,-4), new(2,0), new(-2,0)
        };
        LinearRing cuttingRing = new LinearRing(cutting);
 
        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(-3,2), new(-2,4), new(-2,0), new(-3,2)
            })
        };
 
        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);
 
        //Assert
        Assert.Equal(expected, actual);
    }
 
    [Fact]
    public void EdgeOnLineFirst()
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
    public void EdgeOnLineSecond()
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
    public void EdgeOnLineThird()
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
    public void EdgeOnLineFourth()
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
    public void EdgeOnLineFifth()
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
 
    [Fact]
    public void EdgeOnLineSixth()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(0,0), new(-2,4), new(3,4), new(6,0), new(0,0)
        };
        LinearRing clippedRing = new LinearRing(clipped);
 
        Coordinate[] cutting =
        {
            new(4,-2), new(3,0), new(0,0), new(-2,2), new(4,2), new(4,-2)
        };
        LinearRing cuttingRing = new LinearRing(cutting);
 
        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(0,0), new(-1,2), new(4,2), new(4,0) ,new(3,0), new(0,0)
            })
        };
 
        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);
 
        //Assert
        Assert.Equal(expected, actual);
    }
 
    [Fact]
    public void EdgeOnLineSeventh()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-4,0), new(-4,6), new(3,6), new(3,0), new(-4,0)
        };
        LinearRing clippedRing = new LinearRing(clipped);
 
        Coordinate[] cutting =
        {
            new(1,0), new(2,4), new(2,-2), new(-4,-2), new(-4,0), new(1,0)
        };
        LinearRing cuttingRing = new LinearRing(cutting);
 
        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(2,0), new(1,0), new(2,4), new(2,0)
            })
        };
 
        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);
 
        //Assert
        Assert.Equal(expected, actual);
    }
 
    [Fact]
    public void EdgeOnLineEighth()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-1,-1), new(-1,5), new(0,0), new(5,0), new(5,-1), new(-1,-1)
        };
        LinearRing clippedRing = new LinearRing(clipped);
 
        Coordinate[] cutting =
        {
            new(-2,-2), new(-2,4), new(0,0), new(3,0), new(3,-2), new(-2,-2)
        };
        LinearRing cuttingRing = new LinearRing(cutting);
 
        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(3,-1), new(-1,-1), new(-1,2), new(0,0), new(3,0), new(3,-1)
            })
        };
 
        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);
 
        //Assert
        Assert.Equal(expected, actual);
    }
 
    [Fact]
    public void EdgeOnLineNinth()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2,0), new(4,0), new(4,-3), new(-4,-3), new(-4,4), new(-2,0)
        };
        LinearRing clippedRing = new LinearRing(clipped);
 
        Coordinate[] cutting =
        {
            new(-2,0), new(2,0), new(3,4), new(3,-2), new(-2,-2), new(-2,0) 
        };
        LinearRing cuttingRing = new LinearRing(cutting);
 
        List<LinearRing> expected = new()
        {
            new LinearRing(new Coordinate[]
            {
                new(2,0), new(3,0), new(3,-2), new(-2,-2), new(-2,0), new(2,0)
            })
        };
 
        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);
 
        //Assert
        Assert.Equal(expected, actual);
    }
 
    [Fact]
    public void EdgeOnLineTenth()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(0,0), new(-2,4), new(4,4), new(5,0), new(0,0)
        };
        LinearRing clippedRing = new LinearRing(clipped);
 
        Coordinate[] cutting =
        {
            new(0,0), new(3,0), new(3,-2), new(-2,-2), new(-2,2), new(0,0)
        };
        LinearRing cuttingRing = new LinearRing(cutting);
 
        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);
 
        //Assert
        Assert.Empty(actual);
    }
}
