using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoSlicer.GridSlicer;

namespace GeoSlicer.Tests.GridTests;

public class WeilerAthertonTests
{
    private readonly GeometryFactory _gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);

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

        //var lnr = _gf.CreateLinearRing(coordinates);
        var slicer = new GridSlicer.GridSlicer();

        List<List<Coordinate>> expected = new List<List<Coordinate>>()
        {
            new List<Coordinate>() {
                new Coordinate(4, 2), new Coordinate(3, 2), new Coordinate(4, 4), new Coordinate(4, 2)
            }
        };

        //Act
        var figures = slicer.WeilerAtherton(clipped,cutting);

        //Assert
        Assert.Equal(expected, figures);
    }
}
