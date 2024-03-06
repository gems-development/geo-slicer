using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace GeoSlicer.Tests.GridTests;

public class WeilerAthertonTests
{
    private readonly GeometryFactory _gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
    private readonly GridSlicer.GridSlicer slicer = new GridSlicer.GridSlicer();
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
            new List<Coordinate>() {
                new Coordinate(4, 4), new Coordinate(4, 2), new Coordinate(3, 2), new Coordinate(4, 4)
            }
        };

        //Act
        var figures = slicer.WeilerAtherton(clipped,cutting);

        //Assert
        Assert.Equal(expected, figures);
    }

    [Fact]
    public void TestWhereNodeToENextEqualsNull()
    {
        //Arrange
        List<Coordinate> clipped = new List<Coordinate>()
        {
            new(8,2), new(2,-2), new(-1,2), new(1,6), new(8,6), new(8,2)
        };

        List<Coordinate> cutting = new List<Coordinate>()
        {
            new(5,4), new(10, 4), new(10, -2), new(5, -2), new(5,4)
        };

        List<List<Coordinate>> expected = new List<List<Coordinate>>()
        {
            new List<Coordinate>() {
                new(8, 4), new(8, 2), new(5, 0), new(5,4), new(8, 4)
            }
        };

        //Act
        var figures = slicer.WeilerAtherton(clipped, cutting);

        //Assert
        Assert.Equal(expected, figures);
    }
}
