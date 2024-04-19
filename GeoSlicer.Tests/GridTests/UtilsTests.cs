using GeoSlicer.Utils;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.GridTests;

public class UtilsTests
{
    [Fact]
    public void TestInsideTheAngleLeft()
    {
        Coordinate vecBegin = new Coordinate(1, 1);
        Coordinate vecEnd = new Coordinate(0, 1);

        Coordinate angleA = new Coordinate(0,1);
        Coordinate angleB = new Coordinate(1,1);
        Coordinate angleC = new Coordinate(2,1);

        Assert.True(VectorService.InsideTheAngle(vecBegin, vecEnd,
            angleA, angleB, angleC));
    } 
    
    [Fact]
    public void TestInsideTheAngleRight()
    {
        Coordinate vecBegin = new Coordinate(1, 1);
        Coordinate vecEnd = new Coordinate(2, 1);

        Coordinate angleA = new Coordinate(0,1);
        Coordinate angleB = new Coordinate(1,1);
        Coordinate angleC = new Coordinate(2,1);

        Assert.True(VectorService.InsideTheAngle(vecBegin, vecEnd,
            angleA, angleB, angleC));
    } 
}