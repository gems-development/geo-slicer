using System;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.NonConvexSlicer.Helpers;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests;

public class NonConvexSlicerTest
{
    private static readonly double Epsilon = 1E-5;

    private static readonly GeometryFactory Gf =
        NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);

    private static readonly LineService LineService = new LineService(Epsilon);
    private static readonly SegmentService SegmentService = new SegmentService(LineService);
    private static readonly TraverseDirection TraverseDirection = new TraverseDirection(LineService);

    private static readonly NonConvexSlicerHelper NonConvexSlicerHelper = new NonConvexSlicerHelper(
        new LineIntersector(new EpsilonCoordinateComparator(Epsilon), LineService, Epsilon),
        TraverseDirection, LineService);

    private readonly NonConvexSlicer.NonConvexSlicer _nonConvexSlicer =
        new(Gf, SegmentService, NonConvexSlicerHelper, TraverseDirection, LineService);

    [Fact]
    public void OneSpecialPoint_OptimalSlice()
    {
        //Arrange
        Coordinate[] coordinates =
        {
            new(3, 1), new(1, 7), new(3, 5), new(5, 7), new(3, 1)
        };
        var lnr = Gf.CreateLinearRing(coordinates);

        var firstGeometryCoordinatesExpected = new[]
        {
            new Coordinate(3, 5), new Coordinate(5, 7), new Coordinate(3, 1), new Coordinate(3, 5)
        };
        var secondGeometryCoordinatesExpected = new[]
        {
            new Coordinate(3, 1), new Coordinate(1, 7), new Coordinate(3, 5), new Coordinate(3, 1)
        };

        //Act
        var geometries = _nonConvexSlicer.SliceFigureWithMinNumberOfSpecialPoints(lnr);

        //Assert
        Assert.Equal(2, geometries.Count);
        Assert.Equal(firstGeometryCoordinatesExpected, geometries[0].Coordinates);
        Assert.Equal(secondGeometryCoordinatesExpected, geometries[1].Coordinates);
    }

    [Fact]
    public void OneSpecialPoint_MultiplePointsInSegment_OptimalSlice()
    {
        //Arrange
        Coordinate[] coordinates =
        {
            new(3, 1), new(2, 4), new(1, 7), new(2, 6), new(3, 5), new(4, 6), new(5, 7), new(4, 4), new(3, 1)
        };
        var lnr = Gf.CreateLinearRing(coordinates);

        var firstGeometryCoordinatesExpected = new[]
        {
            new Coordinate(3, 5), new Coordinate(5, 7), new Coordinate(3, 1), new Coordinate(3, 5)
        };
        var secondGeometryCoordinatesExpected = new[]
        {
            new Coordinate(3, 1), new Coordinate(1, 7), new Coordinate(3, 5), new Coordinate(3, 1)
        };

        //Act
        var geometries = _nonConvexSlicer.SliceFigureWithMinNumberOfSpecialPoints(lnr);

        //Assert
        Assert.Equal(2, geometries.Count);
        Assert.Equal(firstGeometryCoordinatesExpected, geometries[0].Coordinates);
        Assert.Equal(secondGeometryCoordinatesExpected, geometries[1].Coordinates);
    }

    [Fact]
    public void OneSpecialPoint_NonOptimalSlice()
    {
        //Arrange
        Coordinate[] coordinates =
        {
            new(1, 1), new(1, 14), new(7, 14), new(2, 9), new(10, 1), new(1, 1)
        };
        var lnr = Gf.CreateLinearRing(coordinates);

        var firstGeometryCoordinatesExpected = new[]
        {
            new Coordinate(2, 9), new Coordinate(10, 1), new Coordinate(1, 1), new Coordinate(2, 9)
        };
        var secondGeometryCoordinatesExpected = new[]
        {
            new Coordinate(2, 9), new Coordinate(1, 1), new Coordinate(1, 14), new Coordinate(2, 9)
        };
        var thirdGeometryCoordinatesExpected = new[]
        {
            new Coordinate(1, 14), new Coordinate(7, 14), new Coordinate(2, 9), new Coordinate(1, 14)
        };

        //Act
        var geometries = _nonConvexSlicer.SliceFigureWithMinNumberOfSpecialPoints(lnr);

        //Assert
        Assert.Equal(3, geometries.Count);
        Assert.Equal(firstGeometryCoordinatesExpected, geometries[0].Coordinates);
        Assert.Equal(secondGeometryCoordinatesExpected, geometries[1].Coordinates);
        Assert.Equal(thirdGeometryCoordinatesExpected, geometries[2].Coordinates);
    }

    [Fact]
    public void OneSpecialPoint_MultiplePointsInSegment_NonOptimalSlice()
    {
        //Arrange
        Coordinate[] coordinates =
        {
            new(1, 1), new(1, 14), new(2, 14), new(4, 14), new(7, 14), new(2, 9), new(3, 8), new(4, 7), new(5, 6),
            new(6, 5), new(7, 4), new(8, 3), new(9, 2), new(10, 1), new(9, 1), new(8, 1), new(7, 1), new(6, 1),
            new(5, 1), new(4, 1), new(3, 1), new(2, 1), new(1, 1)
        };
        var lnr = Gf.CreateLinearRing(coordinates);

        var firstGeometryCoordinatesExpected = new[]
        {
            new Coordinate(2, 9), new Coordinate(10, 1), new Coordinate(1, 1), new Coordinate(2, 9)
        };
        var secondGeometryCoordinatesExpected = new[]
        {
            new Coordinate(2, 9), new Coordinate(1, 1), new Coordinate(1, 14), new Coordinate(2, 9)
        };
        var thirdGeometryCoordinatesExpected = new[]
        {
            new Coordinate(1, 14), new Coordinate(7, 14), new Coordinate(2, 9), new Coordinate(1, 14)
        };

        //Act
        var geometries = _nonConvexSlicer.SliceFigureWithMinNumberOfSpecialPoints(lnr);

        //Assert
        Assert.Equal(3, geometries.Count);
        Assert.Equal(firstGeometryCoordinatesExpected, geometries[0].Coordinates);
        Assert.Equal(secondGeometryCoordinatesExpected, geometries[1].Coordinates);
        Assert.Equal(thirdGeometryCoordinatesExpected, geometries[2].Coordinates);
    }

    [Theory]
    [InlineData(9, 1, 1, 1, 8, 3, 7, 3, 8, 8, 6, 6, 5, 8, 4, 3, 1, 3, 2, 1, 1)]
    [InlineData(12, 1, 1, 2, 10, 6, 13, 8, 11, 10, 13, 14, 11, 10, 10, 9, 8, 15, 10, 10, 2, 14, 4, 9, 1, 1, 1)]
    [InlineData(17, 2, 2, 4, 4, 4, 7, 2, 9, 9, 9, 7, 7, 9, 5, 11, 7, 10, 9, 14, 9, 14, 7, 16, 8, 16, 2, 14, 4, 12, 5,
        10, 4, 10, 2, 2, 2)]
    [InlineData(22, 1, 1, 1, 14, 3, 14, 1, 16, 9, 16, 6, 14, 7, 13, 9, 13, 10, 15, 13, 10, 10, 12, 5, 9, 9, 6, 10, 7,
        11,
        4, 10, 5, 7, 4, 8, 3, 10, 4, 10, 3, 3, 1, 3, 3, 1, 1)]
    public void Slicer_GeneralProperties(int n, params int[] arr)
    {
        //Arrange
        var coordinates = new Coordinate[n + 1];
        for (var i = 0; i < arr.Length; i += 2)
        {
            coordinates[(int)Math.Ceiling(i * 1.0 / 2)] = new Coordinate(arr[i], arr[i + 1]);
        }

        var lnr = Gf.CreateLinearRing(coordinates);

        //Act
        var geometries = _nonConvexSlicer.Slice(lnr);

        //Assert
        //Проверка того, что в каждой части меньше точек, чем в изначальной фигуре
        foreach (var elem in geometries)
        {
            Assert.True(elem.Count < lnr.Count);
        }

        //Проверка того, что set точек всех частей совпадает с set точек изначальной фигуры
        var partsSet = new HashSet<Coordinate>(lnr.Coordinates);
        foreach (var elem in geometries)
        {
            partsSet.ExceptWith(new HashSet<Coordinate>(elem.Coordinates));
        }

        Assert.False(partsSet.Any());


        //Проверка отсутствия особых точек в получившихся кольцах
        foreach (var ring in geometries)
        {
            Assert.True(!NonConvexSlicerHelper.GetSpecialPoints(ring).Any());
        }
    }

    [Fact]
    public void LastIteration_ALotOfSpecialPoints()
    {
        //Arrange
        Coordinate[] coordinates =
        {
            new(1, 1), new(1, 14), new(3, 14), new(1, 16), new(9, 16),
            new(6, 14), new(7, 13), new(9, 13), new(10, 15), new(13, 10),
            new(10, 12), new(5, 9), new(9, 6), new(10, 7), new(11, 4), new(10, 5),
            new(7, 4), new(8, 3), new(10, 4), new(10, 3), new(3, 1), new(3, 3), new(1, 1)
        };
        var lnr = Gf.CreateLinearRing(coordinates);

        var firstGeometryCoordinatesExpected = new[]
        {
            new Coordinate(3, 14), new Coordinate(1, 16),
            new Coordinate(9, 16), new Coordinate(6, 14), new Coordinate(3, 14)
        };
        var secondGeometryCoordinatesExpected = new[]
        {
            new Coordinate(9, 13), new Coordinate(10, 15),
            new Coordinate(13, 10), new Coordinate(10, 12), new Coordinate(9, 13)
        };
        var thirdGeometryCoordinatesExpected = new[]
        {
            new Coordinate(9, 6), new Coordinate(10, 7),
            new Coordinate(11, 4), new Coordinate(9, 6)
        };
        var fourthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(8, 3), new Coordinate(10, 4), new Coordinate(10, 3),
            new Coordinate(8, 3)
        };
        var fifthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(10, 3), new Coordinate(3, 1), new Coordinate(3, 3),
            new Coordinate(10, 3)
        };
        var sixthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(3, 3), new Coordinate(1, 1), new Coordinate(1, 14),
            new Coordinate(3, 14), new Coordinate(3, 3)
        };
        var seventhGeometryCoordinatesExpected = new[]
        {
            new Coordinate(7, 13), new Coordinate(9, 13), new Coordinate(10, 12),
            new Coordinate(5, 9), new Coordinate(7, 13)
        };
        var eighthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(5, 9), new Coordinate(9, 6), new Coordinate(10, 5),
            new Coordinate(7, 4), new Coordinate(5, 9)
        };
        var ninthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(7, 4), new Coordinate(8, 3), new Coordinate(3, 3),
            new Coordinate(7, 4)
        };
        var tenthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(3, 3), new Coordinate(3, 14), new Coordinate(6, 14),
            new Coordinate(3, 3)
        };
        var eleventhGeometryCoordinatesExpected = new[]
        {
            new Coordinate(5, 9), new Coordinate(7, 4), new Coordinate(3, 3),
            new Coordinate(5, 9)
        };
        var twelfthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(5, 9), new Coordinate(3, 3), new Coordinate(6, 14),
            new Coordinate(5, 9)
        };
        var thirteenthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(6, 14), new Coordinate(7, 13), new Coordinate(5, 9), new Coordinate(6, 14)
        };

        //Act
        var geometries = _nonConvexSlicer.Slice(lnr);

        //Assert
        Assert.Equal(13, geometries.Count);
        Assert.Equal(firstGeometryCoordinatesExpected, geometries[0].Coordinates);
        Assert.Equal(secondGeometryCoordinatesExpected, geometries[1].Coordinates);
        Assert.Equal(thirdGeometryCoordinatesExpected, geometries[2].Coordinates);
        Assert.Equal(fourthGeometryCoordinatesExpected, geometries[3].Coordinates);
        Assert.Equal(fifthGeometryCoordinatesExpected, geometries[4].Coordinates);
        Assert.Equal(sixthGeometryCoordinatesExpected, geometries[5].Coordinates);
        Assert.Equal(seventhGeometryCoordinatesExpected, geometries[6].Coordinates);
        Assert.Equal(eighthGeometryCoordinatesExpected, geometries[7].Coordinates);
        Assert.Equal(ninthGeometryCoordinatesExpected, geometries[8].Coordinates);
        Assert.Equal(tenthGeometryCoordinatesExpected, geometries[9].Coordinates);
        Assert.Equal(eleventhGeometryCoordinatesExpected, geometries[10].Coordinates);
        Assert.Equal(twelfthGeometryCoordinatesExpected, geometries[11].Coordinates);
        Assert.Equal(thirteenthGeometryCoordinatesExpected, geometries[12].Coordinates);
    }

    [Fact]
    public void LastIteration_ZeroSpecialPoints()
    {
        //Arrange
        Coordinate[] coordinates =
        {
            new(1, 1), new(1, 8), new(3, 7), new(3, 8), new(8, 6), new(6, 5), new(8, 4), new(3, 1), new(3, 2), new(1, 1)
        };
        var lnr = Gf.CreateLinearRing(coordinates);

        var firstGeometryCoordinatesExpected = new[]
        {
            new Coordinate(3, 7), new Coordinate(3, 8), new Coordinate(8, 6), new Coordinate(6, 5), new Coordinate(3, 7)
        };
        var secondGeometryCoordinatesExpected = new[]
        {
            new Coordinate(6, 5), new Coordinate(8, 4), new Coordinate(3, 1), new Coordinate(3, 2), new Coordinate(6, 5)
        };
        var thirdGeometryCoordinatesExpected = new[]
        {
            new Coordinate(3, 2), new Coordinate(1, 1), new Coordinate(1, 8), new Coordinate(3, 7), new Coordinate(3, 2)
        };
        var fourthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(3, 7), new Coordinate(6, 5), new Coordinate(3, 2), new Coordinate(3, 7)
        };

        //Act
        var geometries = _nonConvexSlicer.Slice(lnr);

        //Assert
        Assert.Equal(4, geometries.Count);
        Assert.Equal(firstGeometryCoordinatesExpected, geometries[0].Coordinates);
        Assert.Equal(secondGeometryCoordinatesExpected, geometries[1].Coordinates);
        Assert.Equal(thirdGeometryCoordinatesExpected, geometries[2].Coordinates);
        Assert.Equal(fourthGeometryCoordinatesExpected, geometries[3].Coordinates);
    }

    [Fact]
    public void LastIteration_OneSpecialPoint()
    {
        //Arrange
        Coordinate[] coordinates =
        {
            new(1, 1), new(2, 10), new(6, 13), new(8, 11),
            new(10, 13), new(14, 11), new(10, 10), new(9, 8),
            new(15, 10), new(10, 2), new(14, 4), new(9, 1),
            new(1, 1)
        };
        var lnr = Gf.CreateLinearRing(coordinates);

        var firstGeometryCoordinatesExpected = new[]
        {
            new Coordinate(8, 11),
            new Coordinate(10, 13), new Coordinate(14, 11), new Coordinate(10, 10), new Coordinate(8, 11)
        };
        var secondGeometryCoordinatesExpected = new[]
        {
            new Coordinate(9, 8),
            new Coordinate(15, 10), new Coordinate(10, 2), new Coordinate(9, 8)
        };
        var thirdGeometryCoordinatesExpected = new[]
        {
            new Coordinate(10, 2), new Coordinate(14, 4), new Coordinate(9, 1), new Coordinate(10, 2)
        };
        var fourthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(9, 1), new Coordinate(1, 1), new Coordinate(2, 10),
            new Coordinate(6, 13), new Coordinate(8, 11), new Coordinate(10, 2), new Coordinate(9, 1)
        };
        var fifthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(9, 8), new Coordinate(10, 2), new Coordinate(8, 11), new Coordinate(9, 8)
        };
        var sixthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(8, 11), new Coordinate(10, 10), new Coordinate(9, 8), new Coordinate(8, 11)
        };

        //Act
        var geometries = _nonConvexSlicer.Slice(lnr);

        //Assert
        Assert.Equal(6, geometries.Count);
        Assert.Equal(firstGeometryCoordinatesExpected, geometries[0].Coordinates);
        Assert.Equal(secondGeometryCoordinatesExpected, geometries[1].Coordinates);
        Assert.Equal(thirdGeometryCoordinatesExpected, geometries[2].Coordinates);
        Assert.Equal(fourthGeometryCoordinatesExpected, geometries[3].Coordinates);
        Assert.Equal(fifthGeometryCoordinatesExpected, geometries[4].Coordinates);
        Assert.Equal(sixthGeometryCoordinatesExpected, geometries[5].Coordinates);
    }

    [Fact]
    public void LastIteration_TwoSpecialPoints()
    {
        //Arrange
        Coordinate[] coordinates =
        {
            new(2, 2), new(4, 4), new(4, 7), new(2, 9), new(9, 9), new(7, 7), new(9, 5), new(11, 7), new(10, 9),
            new(14, 9), new(14, 7), new(16, 8), new(16, 2), new(14, 4), new(12, 5),
            new(10, 4), new(10, 2), new(2, 2)
        };
        var lnr = Gf.CreateLinearRing(coordinates);

        var firstGeometryCoordinatesExpected = new[]
        {
            new Coordinate(4, 7), new Coordinate(2, 9), new Coordinate(9, 9), new Coordinate(7, 7), new Coordinate(4, 7)
        };
        var secondGeometryCoordinatesExpected = new[]
        {
            new Coordinate(11, 7), new Coordinate(10, 9), new Coordinate(14, 9), new Coordinate(14, 7),
            new Coordinate(11, 7)
        };
        var thirdGeometryCoordinatesExpected = new[]
        {
            new Coordinate(14, 7), new Coordinate(16, 8), new Coordinate(16, 2), new Coordinate(14, 4),
            new Coordinate(14, 7)
        };
        var fourthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(10, 4), new Coordinate(10, 2), new Coordinate(2, 2), new Coordinate(4, 4),
            new Coordinate(10, 4)
        };
        var fifthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(12, 5), new Coordinate(9, 5), new Coordinate(11, 7), new Coordinate(14, 7),
            new Coordinate(12, 5)
        };
        var sixthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(14, 7), new Coordinate(14, 4), new Coordinate(12, 5), new Coordinate(14, 7)
        };
        var seventhGeometryCoordinatesExpected = new[]
        {
            new Coordinate(9, 5), new Coordinate(12, 5), new Coordinate(10, 4), new Coordinate(4, 4),
            new Coordinate(9, 5)
        };
        var eighthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(4, 4), new Coordinate(4, 7), new Coordinate(7, 7), new Coordinate(9, 5), new Coordinate(4, 4)
        };

        //Act
        var geometries = _nonConvexSlicer.Slice(lnr);

        //Assert
        Assert.Equal(8, geometries.Count);
        Assert.Equal(firstGeometryCoordinatesExpected, geometries[0].Coordinates);
        Assert.Equal(secondGeometryCoordinatesExpected, geometries[1].Coordinates);
        Assert.Equal(thirdGeometryCoordinatesExpected, geometries[2].Coordinates);
        Assert.Equal(fourthGeometryCoordinatesExpected, geometries[3].Coordinates);
        Assert.Equal(fifthGeometryCoordinatesExpected, geometries[4].Coordinates);
        Assert.Equal(sixthGeometryCoordinatesExpected, geometries[5].Coordinates);
        Assert.Equal(seventhGeometryCoordinatesExpected, geometries[6].Coordinates);
        Assert.Equal(eighthGeometryCoordinatesExpected, geometries[7].Coordinates);
    }

    [Fact]
    public void TwoSpecialPointsInRow()
    {
        //Arrange.
        Coordinate[] coordinates =
        {
            new(2, 1), new(2, 4), new(5, 4), new(5, 1), new(4, 1), new(4, 3), new(3, 3), new(3, 1), new(2, 1)
        };
        var lnr = Gf.CreateLinearRing(coordinates);
        var firstGeometryCoordinatesExpected = new[]
        {
            new Coordinate(3, 3), new Coordinate(3, 1), new Coordinate(2, 1), new Coordinate(2, 4), new Coordinate(3, 3)
        };
        var secondGeometryCoordinatesExpected = new[]
        {
            new Coordinate(2, 4), new Coordinate(5, 4), new Coordinate(3, 3), new Coordinate(2, 4)
        };
        var thirdGeometryCoordinatesExpected = new[]
        {
            new Coordinate(5, 4), new Coordinate(5, 1), new Coordinate(4, 1), new Coordinate(4, 3), new Coordinate(5, 4)
        };
        var fourthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(4, 3), new Coordinate(3, 3), new Coordinate(5, 4), new Coordinate(4, 3)
        };

        //Act.
        var geometries = _nonConvexSlicer.Slice(lnr);

        //Assert.
        Assert.Equal(4, geometries.Count);
        Assert.Equal(firstGeometryCoordinatesExpected, geometries[0].Coordinates);
        Assert.Equal(secondGeometryCoordinatesExpected, geometries[1].Coordinates);
        Assert.Equal(thirdGeometryCoordinatesExpected, geometries[2].Coordinates);
        Assert.Equal(fourthGeometryCoordinatesExpected, geometries[3].Coordinates);
    }

    [Fact]
    public void ZeroTunnels_AfterHoleDeleter_OneHole()
    {
        //Arrange.
        Coordinate[] coordinates =
        {
            new(3, 2), new(1, 10), new(6, 14), new(18, 12), new(15, 1), new(7, 1), new(7, 4), new(10, 5), new(10, 7),
            new(8, 8), new(5, 6), new(7, 4), new(7, 1), new(3, 2)
        };
        var lnr = Gf.CreateLinearRing(coordinates);
        var firstGeometryCoordinatesExpected = new[]
        {
            new Coordinate(7, 4), new Coordinate(7, 1), new Coordinate(3, 2), new Coordinate(1, 10),
            new Coordinate(7, 4)
        };
        var secondGeometryCoordinatesExpected = new[]
        {
            new Coordinate(1, 10), new Coordinate(6, 14), new Coordinate(5, 6), new Coordinate(1, 10)
        };
        var thirdGeometryCoordinatesExpected = new[]
        {
            new Coordinate(6, 14), new Coordinate(18, 12), new Coordinate(15, 1), new Coordinate(6, 14)
        };
        var fourthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(15, 1), new Coordinate(7, 1), new Coordinate(7, 4), new Coordinate(10, 5),
            new Coordinate(15, 1)
        };
        var fifthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(8, 8), new Coordinate(5, 6), new Coordinate(6, 14), new Coordinate(8, 8)
        };
        var sixthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(10, 7), new Coordinate(6, 14), new Coordinate(15, 1), new Coordinate(10, 7)
        };
        var seventhGeometryCoordinatesExpected = new[]
        {
            new Coordinate(15, 1), new Coordinate(10, 5), new Coordinate(10, 7), new Coordinate(15, 1)
        };
        var eighthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(10, 7), new Coordinate(8, 8), new Coordinate(6, 14), new Coordinate(10, 7)
        };

        //Act.
        var geometries = _nonConvexSlicer.Slice(lnr);

        //Arrange.
        Assert.Equal(8, geometries.Count);
        Assert.Equal(firstGeometryCoordinatesExpected, geometries[0].Coordinates);
        Assert.Equal(secondGeometryCoordinatesExpected, geometries[1].Coordinates);
        Assert.Equal(thirdGeometryCoordinatesExpected, geometries[2].Coordinates);
        Assert.Equal(fourthGeometryCoordinatesExpected, geometries[3].Coordinates);
        Assert.Equal(fifthGeometryCoordinatesExpected, geometries[4].Coordinates);
        Assert.Equal(sixthGeometryCoordinatesExpected, geometries[5].Coordinates);
        Assert.Equal(seventhGeometryCoordinatesExpected, geometries[6].Coordinates);
        Assert.Equal(eighthGeometryCoordinatesExpected, geometries[7].Coordinates);
    }

    [Fact]
    public void ZeroTunnels_AfterHoleDeleter_TwoHoles()
    {
        //Arrange.
        Coordinate[] coordinates =
        {
            new(3, 2), new(1, 10), new(6, 14), new(18, 12), new(14, 11), new(12, 12), new(14, 9),
            new(14, 11), new(18, 12), new(15, 1), new(7, 1), new(7, 4), new(10, 5), new(10, 7),
            new(8, 8), new(5, 6), new(7, 4), new(7, 1), new(3, 2)
        };
        var lnr = Gf.CreateLinearRing(coordinates);
        var firstGeometryCoordinatesExpected = new[]
        {
            new Coordinate(14, 9), new Coordinate(14, 11), new Coordinate(18, 12), new Coordinate(15, 1),
            new Coordinate(14, 9)
        };
        var secondGeometryCoordinatesExpected = new[]
        {
            new Coordinate(10, 5), new Coordinate(14, 9), new Coordinate(15, 1), new Coordinate(7, 1),
            new Coordinate(10, 5)
        };
        var thirdGeometryCoordinatesExpected = new[]
        {
            new Coordinate(7, 1), new Coordinate(7, 4), new Coordinate(10, 5), new Coordinate(7, 1)
        };
        var fourthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(7, 4), new Coordinate(7, 1), new Coordinate(3, 2),
            new Coordinate(1, 10), new Coordinate(7, 4)
        };
        var fifthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(12, 12), new Coordinate(5, 6), new Coordinate(1, 10), new Coordinate(6, 14),
            new Coordinate(12, 12)
        };
        var sixthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(12, 12), new Coordinate(6, 14), new Coordinate(18, 12), new Coordinate(12, 12)
        };
        var seventhGeometryCoordinatesExpected = new[]
        {
            new Coordinate(18, 12), new Coordinate(14, 11), new Coordinate(12, 12), new Coordinate(18, 12)
        };
        var eighthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(8, 8), new Coordinate(5, 6), new Coordinate(12, 12), new Coordinate(8, 8)
        };
        var ninthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(12, 12), new Coordinate(14, 9), new Coordinate(8, 8), new Coordinate(12, 12)
        };
        var tenthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(14, 9), new Coordinate(10, 5), new Coordinate(10, 7), new Coordinate(14, 9)
        };
        var eleventhGeometryCoordinatesExpected = new[]
        {
            new Coordinate(10, 7), new Coordinate(8, 8), new Coordinate(14, 9), new Coordinate(10, 7)
        };

        //Act.
        var geometries = _nonConvexSlicer.Slice(lnr);

        //Assert.
        Assert.Equal(11, geometries.Count);
        Assert.Equal(firstGeometryCoordinatesExpected, geometries[0].Coordinates);
        Assert.Equal(secondGeometryCoordinatesExpected, geometries[1].Coordinates);
        Assert.Equal(thirdGeometryCoordinatesExpected, geometries[2].Coordinates);
        Assert.Equal(fourthGeometryCoordinatesExpected, geometries[3].Coordinates);
        Assert.Equal(fifthGeometryCoordinatesExpected, geometries[4].Coordinates);
        Assert.Equal(sixthGeometryCoordinatesExpected, geometries[5].Coordinates);
        Assert.Equal(seventhGeometryCoordinatesExpected, geometries[6].Coordinates);
        Assert.Equal(eighthGeometryCoordinatesExpected, geometries[7].Coordinates);
        Assert.Equal(ninthGeometryCoordinatesExpected, geometries[8].Coordinates);
        Assert.Equal(tenthGeometryCoordinatesExpected, geometries[9].Coordinates);
        Assert.Equal(eleventhGeometryCoordinatesExpected, geometries[10].Coordinates);
    }

    [Fact]
    public void ZeroTunnels_AfterHoleDeleter_TwoHolesFromOnePoint()
    {
        //Arrange.
        Coordinate[] coordinates =
        {
            new(1, 1), new(2, 11), new(4, 14), new(18, 12), new(18, 2), new(10, 1), new(11, 8),
            new(13, 5), new(16, 5), new(15, 10), new(11, 8), new(10, 1), new(4, 3), new(7, 4),
            new(7, 7), new(4, 7), new(4, 3), new(10, 1), new(1, 1)
        };
        var lnr = Gf.CreateLinearRing(coordinates);
        var firstGeometryCoordinatesExpected = new[]
            { new Coordinate(7, 4), new Coordinate(11, 8), new Coordinate(10, 1), new Coordinate(7, 4) };
        var secondGeometryCoordinatesExpected = new[]
            { new Coordinate(10, 1), new Coordinate(4, 3), new Coordinate(7, 4), new Coordinate(10, 1) };
        var thirdGeometryCoordinatesExpected = new[]
            { new Coordinate(4, 3), new Coordinate(10, 1), new Coordinate(1, 1), new Coordinate(4, 3) };
        var fourthGeometryCoordinatesExpected = new[]
            { new Coordinate(1, 1), new Coordinate(2, 11), new Coordinate(4, 3), new Coordinate(1, 1) };
        var fifthGeometryCoordinatesExpected = new[]
            { new Coordinate(2, 11), new Coordinate(4, 14), new Coordinate(18, 12), new Coordinate(2, 11) };
        var sixthGeometryCoordinatesExpected = new[]
            { new Coordinate(13, 5), new Coordinate(18, 2), new Coordinate(10, 1), new Coordinate(13, 5) };
        var seventhGeometryCoordinatesExpected = new[]
            { new Coordinate(10, 1), new Coordinate(11, 8), new Coordinate(13, 5), new Coordinate(10, 1) };
        var eighthGeometryCoordinatesExpected = new[]
            { new Coordinate(11, 8), new Coordinate(7, 4), new Coordinate(7, 7), new Coordinate(11, 8) };
        var ninthGeometryCoordinatesExpected = new[]
            { new Coordinate(4, 7), new Coordinate(4, 3), new Coordinate(2, 11), new Coordinate(4, 7) };
        var tenthGeometryCoordinatesExpected = new[]
            { new Coordinate(2, 11), new Coordinate(18, 12), new Coordinate(4, 7), new Coordinate(2, 11) };
        var eleventhGeometryCoordinatesExpected = new[]
            { new Coordinate(16, 5), new Coordinate(18, 12), new Coordinate(18, 2), new Coordinate(16, 5) };
        var twelfthGeometryCoordinatesExpected = new[]
            { new Coordinate(18, 2), new Coordinate(13, 5), new Coordinate(16, 5), new Coordinate(18, 2) };
        var thirteenthGeometryCoordinatesExpected = new[]
        {
            new Coordinate(15, 10), new Coordinate(11, 8), new Coordinate(7, 7), new Coordinate(4, 7),
            new Coordinate(18, 12), new Coordinate(15, 10)
        };
        var fourteenthGeometryCoordinatesExpected = new[]
            { new Coordinate(18, 12), new Coordinate(16, 5), new Coordinate(15, 10), new Coordinate(18, 12) };

        //Act.
        var geometries = _nonConvexSlicer.Slice(lnr);

        //Assert.
        Assert.Equal(14, geometries.Count);
        Assert.Equal(firstGeometryCoordinatesExpected, geometries[0].Coordinates);
        Assert.Equal(secondGeometryCoordinatesExpected, geometries[1].Coordinates);
        Assert.Equal(thirdGeometryCoordinatesExpected, geometries[2].Coordinates);
        Assert.Equal(fourthGeometryCoordinatesExpected, geometries[3].Coordinates);
        Assert.Equal(fifthGeometryCoordinatesExpected, geometries[4].Coordinates);
        Assert.Equal(sixthGeometryCoordinatesExpected, geometries[5].Coordinates);
        Assert.Equal(seventhGeometryCoordinatesExpected, geometries[6].Coordinates);
        Assert.Equal(eighthGeometryCoordinatesExpected, geometries[7].Coordinates);
        Assert.Equal(ninthGeometryCoordinatesExpected, geometries[8].Coordinates);
        Assert.Equal(tenthGeometryCoordinatesExpected, geometries[9].Coordinates);
        Assert.Equal(eleventhGeometryCoordinatesExpected, geometries[10].Coordinates);
        Assert.Equal(twelfthGeometryCoordinatesExpected, geometries[11].Coordinates);
        Assert.Equal(thirteenthGeometryCoordinatesExpected, geometries[12].Coordinates);
        Assert.Equal(fourteenthGeometryCoordinatesExpected, geometries[13].Coordinates);
    }
}