using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Utils;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.UtilsTests;

public class HoleMatcherTests
{
    private static readonly double Epsilon = 1E-9;

    private static readonly LineService LineService = new(Epsilon);

    private static readonly HolesMatcher HolesMatcher = new(new ContainsChecker(LineService, Epsilon));

    private static readonly MultiPolygon MultiPolygon =
        GeoJsonFileService.ReadGeometryFromFile<MultiPolygon>("TestFiles\\ForHoleMatcher.geojson");
    
    [Fact]
    public void Test()
    {
        int totalHoles = MultiPolygon.Geometries.Sum(geometry => ((Polygon)geometry).NumInteriorRings);

        LinkedList<Polygon> result = HolesMatcher.MatchHoles(
            MultiPolygon.Select(geometry => ((Polygon)geometry).Shell),
            new LinkedList<LinearRing>(MultiPolygon.SelectMany(geometry => ((Polygon)geometry).Holes)));

        Assert.Equal(MultiPolygon.Count, result.Count);
        Assert.Equal(totalHoles, result.Sum(polygon => polygon.NumInteriorRings));
        // Во входном файле количество точек каждой дыры равно количеству точек оболочки её полигона
        Assert.True(result.All(polygon => polygon.Holes.All(hole => polygon.Shell.Count == hole.Count)));
    }
}