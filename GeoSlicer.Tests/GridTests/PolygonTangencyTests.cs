using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.PolygonClippingAlghorithm;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.GridTests;

public class PolygonTangencyTests
{
    private static readonly double Epsilon = 1E-9;

    private static readonly LineService LineService = new(Epsilon, new EpsilonCoordinateComparator(Epsilon));

    private static readonly WeilerAthertonAlghorithm SlicerHelper =
        new(new LinesIntersector(new EpsilonCoordinateComparator(Epsilon), LineService, Epsilon), LineService,
            new EpsilonCoordinateComparator(), new ContainsChecker(LineService, Epsilon), Epsilon);

    [Fact]
    public void CuttingInClippedByClock()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-4, 2), new(0, 2), new(4, 4), new(2, 0),
            new(2, -4), new(-2, -4), new(-2, 0), new(-4, 2)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(-2, 2), new(2, 2), new(2, -2), new(-2, -2), new(-2, 2)
        };
        LinearRing cuttingRing = new LinearRing(cutting);
        List<LinearRing> expected = new List<LinearRing>()
        {
            new LinearRing(new Coordinate[]
                {
                    new(-2, 2), new(2, 2), new(2, -2), new(-2, -2), new(-2, 2)
                }
            )
        };


        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing).ToList();

        //Assert
        Assert.Equal(expected, actual);
        Assert.Single(actual);
    }

    [Fact]
    public void ClippedInCuttingByClock()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2, 2), new(2, 2), new(2, -2), new(-2, -2), new(-2, 2)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(-4, 2), new(0, 2), new(4, 4), new(2, 0),
            new(2, -4), new(-2, -4), new(-2, 0), new(-4, 2)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        List<LinearRing> expected = new List<LinearRing>()
        {
            new LinearRing(new Coordinate[]
            {
                new(0, 2), new(2, 2), new(2, 0), new(2, -2),
                new(-2, -2), new(-2, 0), new(-2, 2), new(0, 2)
            })
        };

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing).ToList();

        //Assert
        Assert.Equal(expected, actual);
        Assert.Single(actual);
    }

    [Fact]
    public void TangentCuttingUnderAndRighterThanClipped()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2, 1), new(-1, 2), new(0, 1), new(-2, 1)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(-1, 1), new(1, 1), new(0, 0), new(-1, 1)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Empty(actual);
    }

    [Fact]
    public void TangentClippedUnderAndRighterThanCutting()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-1, 1), new(1, 1), new(0, 0), new(-1, 1)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(-2, 1), new(-1, 2), new(0, 1), new(-2, 1)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Empty(actual);
    }

    [Fact]
    public void TangentClippedUnderAndLefterThanCutting()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-1, 1), new(1, 1), new(0, 0), new(-1, 1)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(0, 1), new(1, 2), new(2, 1), new(0, 1)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Empty(actual);
    }

    [Fact]
    public void TangentCuttingUnderAndLefterThanClipped()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(0, 1), new(1, 2), new(2, 1), new(0, 1)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(-1, 1), new(1, 1), new(0, 0), new(-1, 1)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Empty(actual);
    }
    
    [Fact]
    public void TangentRectangles()
    {
        //Arrange
        Coordinate[] clipped =
        {
            new(-2, 4), new(2, 4), new(2, 0), new(-2, 0), new(-2, 4)
        };
        LinearRing clippedRing = new LinearRing(clipped);
        Coordinate[] cutting =
        {
            new(2, 4), new(-2, 4), new(-2, 6), new(2, 6), new(2, 4)
        };
        LinearRing cuttingRing = new LinearRing(cutting);

        //Act
        var actual = SlicerHelper.WeilerAtherton(clippedRing, cuttingRing);

        //Assert
        Assert.Empty(actual);
    }
}