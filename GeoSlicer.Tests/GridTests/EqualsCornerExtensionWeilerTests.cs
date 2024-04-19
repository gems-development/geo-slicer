using System.Collections.Generic;
using GeoSlicer.GridSlicer.Helpers;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.GridTests;

public class EqualsCornerExtensionWeilerTests
{
    private static readonly double Epsilon = 1E-9;

    private static readonly LineService LineService = new LineService(Epsilon);

    private static readonly GridSlicerHelper SlicerHelper =
        new(new LinesIntersector(new EpsilonCoordinateComparator(Epsilon), LineService, Epsilon), Epsilon, LineService,
            new EpsilonCoordinateComparator(), new ContainsChecker(LineService, Epsilon));

    [Fact]
    public void RectangleEdgeRectangle()
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
        // Assert.True(actual[0].IsEqualsRing(expected));
    }
}