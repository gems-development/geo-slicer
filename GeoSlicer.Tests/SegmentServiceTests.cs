using GeoSlicer.NonConvexSlicer.Helpers;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests;

public class SegmentServiceTests
{
    [Fact]
    public void IntersectionOfSegmentsTest()
    {
        //Одинаковые отрезки не пересекаются
        Assert.False(SegmentService.IsIntersectionOfSegments(new Coordinate(0, 0),
            new Coordinate(2, 2),
            new Coordinate(0, 0),
            new Coordinate(2, 2)));
        //Частично совпадающие отрезки пересекаются
        Assert.True(SegmentService.IsIntersectionOfSegments(new Coordinate(0, 0), new Coordinate(2, 2),
            new Coordinate(1, 1),
            new Coordinate(3, 3)));
        //Скрещенные отрезки пересекаются
        Assert.True(SegmentService.IsIntersectionOfSegments(new Coordinate(0, 0), new Coordinate(2, 2),
            new Coordinate(0, 2),
            new Coordinate(2, 0)));
        //Отрезки с общей граничной точкой не пересекаются
        Assert.False(SegmentService.IsIntersectionOfSegments(new Coordinate(0, 0),
            new Coordinate(2, 2),
            new Coordinate(2, 2),
            new Coordinate(0, 4)));
        //Не имеющие общих точек отрезки не пересекаются
        Assert.False(SegmentService.IsIntersectionOfSegments(new Coordinate(0, 0),
            new Coordinate(0, 2),
            new Coordinate(2, 2),
            new Coordinate(4, 2)));
        //Скрещенные отрезки пересекаются
        Assert.True(SegmentService.IsIntersectionOfSegments(new Coordinate(0, 0), new Coordinate(2, 0),
            new Coordinate(2, 2),
            new Coordinate(2, -2)));
    }
}
