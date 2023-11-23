using System;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests;

public class SegmentServiceTests
{
    [Theory]
    [InlineData(false, 0, 0, 2, 2, 0, 0, 2, 2)]
    [InlineData(true, 0, 0, 2, 2, 1, 1, 3, 3)]
    [InlineData(true, 0, 0, 2, 2, 0, 2, 2, 0)]
    [InlineData(false, 0, 0, 2, 2, 2, 2, 0, 4)]
    [InlineData(false, 0, 0, 0, 2, 2, 2, 4, 2)]
    [InlineData(true, 0, 0, 2, 0, 2, 2, 2, -2)]
    public void IntersectionOfSegmentsTest(bool intersectionAnswer, params int[] arr)
    {
        var coordinates = new Coordinate[4];
        for (var i = 0; i < arr.Length; i += 2)
        {
            coordinates[(int)Math.Ceiling(i * 1.0 / 2)] = new Coordinate(arr[i], arr[i + 1]);
        }

        Assert.Equal(intersectionAnswer,
            NonConvexSlicer.Helpers.SegmentService.IsIntersectionOfSegments(
                coordinates[0], coordinates[1],
                coordinates[2], coordinates[3]));
    }
}