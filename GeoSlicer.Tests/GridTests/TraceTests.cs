using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoSlicer.GridSlicer;
using NetTopologySuite.Algorithm;

namespace GeoSlicer.Tests.GridTests;

public class TraceTests
{
    private readonly GeometryFactory _gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);

    [Fact]
    public void TestIsPointInPolygon()
    {
        Coordinate[] polygon1
              = { new Coordinate(2, 1), new Coordinate(1, 4), new Coordinate(4, 4), new Coordinate(2, 1) };

        var slicer = new GridSlicer.GridSlicer();

        Coordinate pointTrue1 = new Coordinate(2, 1);
        Coordinate pointTrue2 = new Coordinate(2, 2);
        Coordinate pointTrue3 = new Coordinate(1, 4);
        Coordinate pointTrue4 = new Coordinate(2, 4);
        Coordinate pointTrue5 = new Coordinate(4, 4);

        Coordinate pointFalse1 = new Coordinate(1, 1);
        Coordinate pointFalse2 = new Coordinate(1, 2);
        Coordinate pointFalse3 = new Coordinate(-1, 4);

        Assert.True(slicer.IsPointInPolygon(pointTrue1, polygon1.ToList()));
        Assert.True(slicer.IsPointInPolygon(pointTrue2, polygon1.ToList()));
        Assert.True(slicer.IsPointInPolygon(pointTrue3, polygon1.ToList()));
        Assert.True(slicer.IsPointInPolygon(pointTrue4, polygon1.ToList()));
        Assert.True(slicer.IsPointInPolygon(pointTrue5, polygon1.ToList()));

        Assert.False(slicer.IsPointInPolygon(pointFalse1, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse2, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse3, polygon1.ToList()));
    }

    [Fact]
    public void Test1IsPointInPolygon()
    {
        Coordinate[] polygon1
              = { new Coordinate(1, 4), new Coordinate(3, 7), new Coordinate(5, 4), new Coordinate(3, 4), new Coordinate(3, 2), new Coordinate(1, 4) };

        var slicer = new GridSlicer.GridSlicer();

        Coordinate pointTrue1 = new Coordinate(2, 4);
        Coordinate pointTrue2 = new Coordinate(2, 3);
        Coordinate pointTrue3 = new Coordinate(5, 4);
        Coordinate pointTrue4 = new Coordinate(3, 6);
        Coordinate pointTrue5 = new Coordinate(3, 3);
        Coordinate pointTrue6 = new Coordinate(2.5, 3.5);

        Coordinate pointFalse1 = new Coordinate(2, 7);
        Coordinate pointFalse2 = new Coordinate(1, 6);
        Coordinate pointFalse3 = new Coordinate(4, 3);
        Coordinate pointFalse4 = new Coordinate(2, 2);
        Coordinate pointFalse5 = new Coordinate(-1, 4);
        Coordinate pointFalse6 = new Coordinate(4, 7);
        Coordinate pointFalse7 = new Coordinate(6, 4);
        Coordinate pointFalse8 = new Coordinate(3, 1);

        Assert.True(slicer.IsPointInPolygon(pointTrue1, polygon1.ToList()));
        Assert.True(slicer.IsPointInPolygon(pointTrue2, polygon1.ToList()));
        Assert.True(slicer.IsPointInPolygon(pointTrue3, polygon1.ToList()));
        Assert.True(slicer.IsPointInPolygon(pointTrue4, polygon1.ToList()));
        Assert.True(slicer.IsPointInPolygon(pointTrue5, polygon1.ToList()));
        Assert.True(slicer.IsPointInPolygon(pointTrue6, polygon1.ToList()));

        Assert.False(slicer.IsPointInPolygon(pointFalse1, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse2, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse3, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse4, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse5, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse6, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse7, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse8, polygon1.ToList()));
    }

    [Fact]
    public void Test2IsPointInPolygon()
    {
        Coordinate[] polygon1
              = { new Coordinate(1,1), new Coordinate(1,6), new Coordinate(2,6), new Coordinate(3,6), new Coordinate(4,5), new Coordinate(6,6),
                  new Coordinate(7,0), new Coordinate(5,3), new Coordinate(4,1), new Coordinate(3,3), new Coordinate(2,1),
                  new Coordinate(1,1) };

        var slicer = new GridSlicer.GridSlicer();

        Coordinate pointTrue1 = new Coordinate(1, 6);
        Coordinate pointTrue2 = new Coordinate(3, 6);
        Coordinate pointTrue3 = new Coordinate(6, 6);
        Coordinate pointTrue4 = new Coordinate(3, 5);
        Coordinate pointTrue5 = new Coordinate(2, 3);
        Coordinate pointTrue6 = new Coordinate(3, 3);
        Coordinate pointTrue7 = new Coordinate(5, 3);
        Coordinate pointTrue8 = new Coordinate(2, 2);
        Coordinate pointTrue9 = new Coordinate(4, 2);
        Coordinate pointTrue10 = new Coordinate(6, 2);
        Coordinate pointTrue11 = new Coordinate(4, 4);
        Coordinate pointTrue12 = new Coordinate(2, 6);

        Coordinate pointFalse1 = new Coordinate(2, 7);
        Coordinate pointFalse2 = new Coordinate(0, 6);
        Coordinate pointFalse3 = new Coordinate(4, 6);
        Coordinate pointFalse4 = new Coordinate(0, 5);
        Coordinate pointFalse5 = new Coordinate(0, 4);
        Coordinate pointFalse6 = new Coordinate(8, 4);
        Coordinate pointFalse7 = new Coordinate(0, 3);
        Coordinate pointFalse8 = new Coordinate(8, 3);
        Coordinate pointFalse9 = new Coordinate(0, 2);
        Coordinate pointFalse10 = new Coordinate(3, 2);
        Coordinate pointFalse11 = new Coordinate(5, 2);
        Coordinate pointFalse12 = new Coordinate(8, 2);
        Coordinate pointFalse13 = new Coordinate(0, 1);
        Coordinate pointFalse14 = new Coordinate(3, 1);
        Coordinate pointFalse15 = new Coordinate(5, 1);
        Coordinate pointFalse16 = new Coordinate(8, 1);
        Coordinate pointFalse17 = new Coordinate(1, -1);
        Coordinate pointFalse18 = new Coordinate(7, -1);

        Assert.True(slicer.IsPointInPolygon(pointTrue1, polygon1.ToList()));
        Assert.True(slicer.IsPointInPolygon(pointTrue2, polygon1.ToList()));
        Assert.True(slicer.IsPointInPolygon(pointTrue3, polygon1.ToList()));
        Assert.True(slicer.IsPointInPolygon(pointTrue4, polygon1.ToList()));
        Assert.True(slicer.IsPointInPolygon(pointTrue5, polygon1.ToList()));
        Assert.True(slicer.IsPointInPolygon(pointTrue6, polygon1.ToList()));
        Assert.True(slicer.IsPointInPolygon(pointTrue7, polygon1.ToList()));
        Assert.True(slicer.IsPointInPolygon(pointTrue8, polygon1.ToList()));
        Assert.True(slicer.IsPointInPolygon(pointTrue9, polygon1.ToList()));
        Assert.True(slicer.IsPointInPolygon(pointTrue10, polygon1.ToList()));
        Assert.True(slicer.IsPointInPolygon(pointTrue11, polygon1.ToList()));
        Assert.True(slicer.IsPointInPolygon(pointTrue12, polygon1.ToList()));

        Assert.False(slicer.IsPointInPolygon(pointFalse1, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse2, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse3, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse4, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse5, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse6, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse7, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse8, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse9, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse10, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse11, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse12, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse13, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse14, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse15, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse16, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse17, polygon1.ToList()));
        Assert.False(slicer.IsPointInPolygon(pointFalse18, polygon1.ToList()));
    }
}
