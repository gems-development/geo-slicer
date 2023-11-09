using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests;

public class NonConvexSlicerTest
{
    [Fact]
    public void OneSpecialPoint_OptimalSlice()
    {
        var gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
        Coordinate[] coordinates =
        {
            new(3, 1), new(1, 7), new(3, 5), new(5, 7), new(3, 1)
        };
        var lnr = gf.CreateLinearRing(coordinates);
        var slicer = new NonConvexSlicer.NonConvexSlicer(true);
        var geometries = slicer.SliceFigureWithOneSpecialPoint(lnr);
        Assert.Equal(2, geometries.Count);
        Assert.Equal(new[]
        {
            new Coordinate(3, 1), new Coordinate(1, 7), new Coordinate(3, 5), new Coordinate(3, 1)
        }, geometries[0].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(3, 5), new Coordinate(5, 7), new Coordinate(3, 1), new Coordinate(3, 5)
        }, geometries[1].Coordinates);
    }

    [Fact]
    public void OneSpecialPoint_MultiplePointsInSegment_OptimalSlice()
    {
        var gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
        Coordinate[] coordinates =
        {
            new(3, 1), new(2, 4), new(1, 7), new(2, 6), new(3, 5), new(4, 6), new(5, 7), new(4, 4), new(3, 1)
        };
        var lnr = gf.CreateLinearRing(coordinates);
        var slicer = new NonConvexSlicer.NonConvexSlicer(true);
        var geometries = slicer.SliceFigureWithOneSpecialPoint(lnr);
        Assert.Equal(2, geometries.Count);
        Assert.Equal(new[]
        {
            new Coordinate(3, 1), new Coordinate(1, 7), new Coordinate(3, 5), new Coordinate(3, 1)
        }, geometries[0].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(3, 5), new Coordinate(5, 7), new Coordinate(3, 1), new Coordinate(3, 5)
        }, geometries[1].Coordinates);
    }

    [Fact]
    public void OneSpecialPoint_NonOptimalSlice()
    {
        var gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
        Coordinate[] coordinates =
        {
            new(1, 1), new(1, 14), new(7, 14), new(2, 9), new(10, 1), new(1, 1)
        };
        var lnr = gf.CreateLinearRing(coordinates);
        var slicer = new NonConvexSlicer.NonConvexSlicer(true);
        var geometries = slicer.SliceFigureWithOneSpecialPoint(lnr);
        Assert.Equal(3, geometries.Count);
        Assert.Equal(new[]
        {
            new Coordinate(2, 9), new Coordinate(10, 1), new Coordinate(1, 1), new Coordinate(2, 9)
        }, geometries[0].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(2, 9), new Coordinate(1, 1), new Coordinate(1, 14), new Coordinate(2, 9)
        }, geometries[1].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(2, 9), new Coordinate(1, 14), new Coordinate(7, 14), new Coordinate(2, 9)
        }, geometries[2].Coordinates);
    }

    [Fact]
    public void OneSpecialPoint_MultiplePointsInSegment_NonOptimalSlice()
    {
        var gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
        Coordinate[] coordinates =
        {
            new(1, 1), new(1, 14), new(2, 14), new(4, 14), new(7, 14), new(2, 9), new(3, 8), new(4, 7), new(5, 6),
            new(6, 5), new(7, 4), new(8, 3), new(9, 2), new(10, 1), new(9, 1), new(8, 1), new(7, 1), new(6, 1),
            new(5, 1), new(4, 1), new(3, 1), new(2, 1), new(1, 1)
        };
        var lnr = gf.CreateLinearRing(coordinates);
        var slicer = new NonConvexSlicer.NonConvexSlicer(true);
        var geometries = slicer.SliceFigureWithOneSpecialPoint(lnr);
        Assert.Equal(3, geometries.Count);
        Assert.Equal(new[]
        {
            new Coordinate(2, 9), new Coordinate(10, 1), new Coordinate(1, 1), new Coordinate(2, 9)
        }, geometries[0].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(2, 9), new Coordinate(1, 1), new Coordinate(1, 14), new Coordinate(2, 9)
        }, geometries[1].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(2, 9), new Coordinate(1, 14), new Coordinate(7, 14), new Coordinate(2, 9)
        }, geometries[2].Coordinates);
    }

    [Fact]
    public void IntersectionOfSegmentsTest()
    {
        //Одинаковые отрезки не пересекаются
        Assert.False(NonConvexSlicer.NonConvexSlicer.IsIntersectionOfSegments(new Coordinate(0, 0),
            new Coordinate(2, 2),
            new Coordinate(0, 0),
            new Coordinate(2, 2)));
        //Частично совпадающие отрезки пересекаются
        Assert.True(NonConvexSlicer.NonConvexSlicer.IsIntersectionOfSegments(new Coordinate(0, 0), new Coordinate(2, 2),
            new Coordinate(1, 1),
            new Coordinate(3, 3)));
        //Скрещенные отрезки пересекаются
        Assert.True(NonConvexSlicer.NonConvexSlicer.IsIntersectionOfSegments(new Coordinate(0, 0), new Coordinate(2, 2),
            new Coordinate(0, 2),
            new Coordinate(2, 0)));
        //Отрезки с общеё граничной точкой не пересекаются
        Assert.False(NonConvexSlicer.NonConvexSlicer.IsIntersectionOfSegments(new Coordinate(0, 0),
            new Coordinate(2, 2),
            new Coordinate(2, 2),
            new Coordinate(0, 4)));
        //Не имеющие общих точек отрезки не пересекаются
        Assert.False(NonConvexSlicer.NonConvexSlicer.IsIntersectionOfSegments(new Coordinate(0, 0),
            new Coordinate(0, 2),
            new Coordinate(2, 2),
            new Coordinate(4, 2)));
        //Скрещенные отрезки пересекаются
        Assert.True(NonConvexSlicer.NonConvexSlicer.IsIntersectionOfSegments(new Coordinate(0, 0), new Coordinate(2, 0),
            new Coordinate(2, 2),
            new Coordinate(2, -2)));
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
        var gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
        var coordinates = new Coordinate[n + 1];
        for (var i = 0; i < arr.Length; i += 2)
        {
            coordinates[(int)Math.Ceiling(i * 1.0 / 2)] = new Coordinate(arr[i], arr[i + 1]);
        }

        var lnr = gf.CreateLinearRing(coordinates);

        var slicer = new NonConvexSlicer.NonConvexSlicer(true);
        var geometries = slicer.Slice(lnr);

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
            Assert.True(!slicer.GetSpecialPoints(ring).Any());
        }
    }

    [Fact]
    public void LastIteration_ALotOfSpecialPoints()
    {
        var gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
        Coordinate[] coordinates =
        {
            new(1, 1), new(1, 14), new(3, 14), new(1, 16), new(9, 16),
            new(6, 14), new(7, 13), new(9, 13), new(10, 15), new(13, 10),
            new(10, 12), new(5, 9), new(9, 6), new(10, 7), new(11, 4), new(10, 5),
            new(7, 4), new(8, 3), new(10, 4), new(10, 3), new(3, 1), new(3, 3), new(1, 1)
        };
        var lnr = gf.CreateLinearRing(coordinates);
        var slicer = new NonConvexSlicer.NonConvexSlicer(true);
        var geometries = slicer.Slice(lnr);

        Assert.Equal(13, geometries.Count);
        Assert.Equal(new[]
        {
            new Coordinate(3, 14), new Coordinate(1, 16),
            new Coordinate(9, 16), new Coordinate(6, 14), new Coordinate(3, 14)
        }, geometries[0].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(9, 13), new Coordinate(10, 15),
            new Coordinate(13, 10), new Coordinate(10, 12), new Coordinate(9, 13)
        }, geometries[1].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(9, 6), new Coordinate(10, 7),
            new Coordinate(11, 4), new Coordinate(9, 6)
        }, geometries[2].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(10, 3), new Coordinate(3, 1), new Coordinate(3, 3),
            new Coordinate(10, 3)
        }, geometries[3].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(8, 3), new Coordinate(10, 4), new Coordinate(10, 3),
            new Coordinate(8, 3)
        }, geometries[4].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(3, 3), new Coordinate(1, 1), new Coordinate(1, 14),
            new Coordinate(3, 14), new Coordinate(3, 3)
        }, geometries[5].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(7, 13), new Coordinate(9, 13), new Coordinate(10, 12),
            new Coordinate(5, 9), new Coordinate(7, 13)
        }, geometries[6].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(5, 9), new Coordinate(9, 6), new Coordinate(10, 5),
            new Coordinate(7, 4), new Coordinate(5, 9)
        }, geometries[7].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(7, 4), new Coordinate(8, 3), new Coordinate(3, 3),
            new Coordinate(7, 4)
        }, geometries[8].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(3, 3), new Coordinate(3, 14), new Coordinate(6, 14),
            new Coordinate(3, 3)
        }, geometries[9].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(5, 9), new Coordinate(7, 4), new Coordinate(3, 3),
            new Coordinate(5, 9)
        }, geometries[10].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(5, 9), new Coordinate(3, 3), new Coordinate(6, 14),
            new Coordinate(5, 9)
        }, geometries[11].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(5, 9), new Coordinate(6, 14), new Coordinate(7, 13),
            new Coordinate(5, 9)
        }, geometries[12].Coordinates);
    }

    [Fact]
    public void LastIteration_ZeroSpecialPoints()
    {
        var gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
        Coordinate[] coordinates =
        {
            new(1, 1), new(1, 8), new(3, 7), new(3, 8), new(8, 6), new(6, 5), new(8, 4), new(3, 1), new(3, 2), new(1, 1)
        };
        var lnr = gf.CreateLinearRing(coordinates);
        var slicer = new NonConvexSlicer.NonConvexSlicer(true);
        var geometries = slicer.Slice(lnr);

        Assert.Equal(4, geometries.Count);
        Assert.Equal(new[]
        {
            new Coordinate(3, 7), new Coordinate(3, 8), new Coordinate(8, 6), new Coordinate(6, 5), new Coordinate(3, 7)
        }, geometries[0].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(6, 5), new Coordinate(8, 4), new Coordinate(3, 1), new Coordinate(3, 2), new Coordinate(6, 5)
        }, geometries[1].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(3, 2), new Coordinate(1, 1), new Coordinate(1, 8), new Coordinate(3, 7), new Coordinate(3, 2)
        }, geometries[2].Coordinates);
        Assert.Equal(new[] { new Coordinate(3, 7), new Coordinate(6, 5), new Coordinate(3, 2), new Coordinate(3, 7) },
            geometries[3].Coordinates);
    }

    [Fact]
    public void LastIteration_OneSpecialPoint()
    {
        var gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
        Coordinate[] coordinates =
        {
            new(1, 1), new(2, 10), new(6, 13), new(8, 11),
            new(10, 13), new(14, 11), new(10, 10), new(9, 8),
            new(15, 10), new(10, 2), new(14, 4), new(9, 1),
            new(1, 1)
        };
        var lnr = gf.CreateLinearRing(coordinates);
        var slicer = new NonConvexSlicer.NonConvexSlicer(true);
        var geometries = slicer.Slice(lnr);

        Assert.Equal(6, geometries.Count);
        Assert.Equal(new[]
        {
            new Coordinate(8, 11),
            new Coordinate(10, 13), new Coordinate(14, 11), new Coordinate(10, 10), new Coordinate(8, 11)
        }, geometries[0].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(9, 8),
            new Coordinate(15, 10), new Coordinate(10, 2), new Coordinate(9, 8)
        }, geometries[1].Coordinates);
        Assert.Equal(
            new[]
            {
                new Coordinate(9, 1), new Coordinate(1, 1), new Coordinate(2, 10),
                new Coordinate(6, 13), new Coordinate(8, 11), new Coordinate(10, 2), new Coordinate(9, 1)
            }, geometries[2].Coordinates);
        Assert.Equal(
            new[] { new Coordinate(10, 2), new Coordinate(14, 4), new Coordinate(9, 1), new Coordinate(10, 2) },
            geometries[3].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(8, 11), new Coordinate(10, 10), new Coordinate(9, 8), new Coordinate(8, 11)
        }, geometries[4].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(9, 8), new Coordinate(10, 2), new Coordinate(8, 11), new Coordinate(9, 8)
        }, geometries[5].Coordinates);
    }

    [Fact]
    public void LastIteration_TwoSpecialPoints()
    {
        var gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
        Coordinate[] coordinates =
        {
            new(2, 2), new(4, 4), new(4, 7), new(2, 9), new(9, 9), new(7, 7), new(9, 5), new(11, 7), new(10, 9),
            new(14, 9), new(14, 7), new(16, 8), new(16, 2), new(14, 4), new(12, 5),
            new(10, 4), new(10, 2), new(2, 2)
        };
        var lnr = gf.CreateLinearRing(coordinates);
        var slicer = new NonConvexSlicer.NonConvexSlicer(true);
        var geometries = slicer.Slice(lnr);

        Assert.Equal(8, geometries.Count);
        Assert.Equal(new[]
        {
            new Coordinate(4, 7), new Coordinate(2, 9), new Coordinate(9, 9), new Coordinate(7, 7), new Coordinate(4, 7)
        }, geometries[0].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(11, 7), new Coordinate(10, 9), new Coordinate(14, 9), new Coordinate(14, 7),
            new Coordinate(11, 7)
        }, geometries[1].Coordinates);
        Assert.Equal(
            new[]
            {
                new Coordinate(14, 7), new Coordinate(16, 8), new Coordinate(16, 2), new Coordinate(14, 4),
                new Coordinate(14, 7)
            }, geometries[2].Coordinates);
        Assert.Equal(
            new[]
            {
                new Coordinate(10, 4), new Coordinate(10, 2), new Coordinate(2, 2), new Coordinate(4, 4),
                new Coordinate(10, 4)
            },
            geometries[3].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(11, 7), new Coordinate(14, 7), new Coordinate(14, 4), new Coordinate(12, 5),
            new Coordinate(11, 7)
        }, geometries[4].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(12, 5), new Coordinate(9, 5), new Coordinate(11, 7), new Coordinate(12, 5)
        }, geometries[5].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(4, 4), new Coordinate(4, 7), new Coordinate(7, 7), new Coordinate(9, 5), new Coordinate(4, 4)
        }, geometries[6].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(9, 5), new Coordinate(12, 5), new Coordinate(10, 4), new Coordinate(4, 4),
            new Coordinate(9, 5)
        }, geometries[7].Coordinates);
    }

    [Fact]
    public void ZeroTunnel_Type_V()
    {
        var gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
        Coordinate[] coordinates =
        {
            new(2, 1), new(2, 4), new(4, 4), new(4, 1), new(3, 1), new(3, 3), new(3, 1), new(2, 1)
        };
        var lnr = gf.CreateLinearRing(coordinates);
        var slicer = new NonConvexSlicer.NonConvexSlicer(true);
        var geometries = slicer.Slice(lnr);

        Assert.Equal(5, geometries.Count);
        Assert.Equal(new[]
        {
            new Coordinate(3, 3), new Coordinate(3, 1), new Coordinate(2, 1), new Coordinate(3, 3)
        }, geometries[0].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(3, 3), new Coordinate(2, 1), new Coordinate(2, 4), new Coordinate(3, 3)
        }, geometries[1].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(3, 3), new Coordinate(2, 4), new Coordinate(4, 4), new Coordinate(3, 3)
        }, geometries[2].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(3, 3), new Coordinate(4, 4), new Coordinate(4, 1), new Coordinate(3, 3)
        }, geometries[3].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(3, 3), new Coordinate(4, 1), new Coordinate(3, 1), new Coordinate(3, 3)
        }, geometries[4].Coordinates);
    }

    [Fact]
    public void TwoSpecialPointsInRow()
    {
        var gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
        Coordinate[] coordinates =
        {
            new(2, 1), new(2, 4), new(5, 4), new(5, 1), new(4, 1), new(4, 3), new(3, 3), new(3, 1), new(2, 1)
        };
        var lnr = gf.CreateLinearRing(coordinates);
        var slicer = new NonConvexSlicer.NonConvexSlicer(true);
        var geometries = slicer.Slice(lnr);

        Assert.Equal(4, geometries.Count);
        Assert.Equal(new[]
        {
            new Coordinate(3, 3), new Coordinate(3, 1), new Coordinate(2, 1), new Coordinate(2, 4), new Coordinate(3, 3)
        }, geometries[0].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(2, 4), new Coordinate(5, 4), new Coordinate(3, 3), new Coordinate(2, 4)
        }, geometries[1].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(5, 4), new Coordinate(5, 1), new Coordinate(4, 1), new Coordinate(4, 3), new Coordinate(5, 4)
        }, geometries[2].Coordinates);
        Assert.Equal(new[]
        {
            new Coordinate(3, 3), new Coordinate(5, 4), new Coordinate(4, 3), new Coordinate(3, 3)
        }, geometries[3].Coordinates);
    }
}