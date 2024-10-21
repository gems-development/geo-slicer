using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.PolygonClippingAlghorithm;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.GridTests;

public class HolesTests
{
    private static readonly double Epsilon = 1E-9;
 
    private static readonly LineService LineService = new(Epsilon);
 
    private static readonly WeilerAthertonAlghorithm SlicerHelper =
        new(new LinesIntersector(new EpsilonCoordinateComparator(Epsilon), LineService, Epsilon), LineService,
            new EpsilonCoordinateComparator(), new ContainsChecker(LineService, Epsilon), Epsilon);

    [Fact]
    public void TwoHolesTest()
    {
        Coordinate[] coordinatesShellClipped =
        {
            new(-2, -2), new(-3, 0), new(-5, 5), new(6, 5), new(6, -2), new(-2, -2)
        };

        Coordinate[] coordinatesFirstHoleClipped =
        {
            new(-2, 0), new(0, 0), new(0, 3), new(-2, 3), new(-2, 0)
        };

        Coordinate[] coordinatesSecondHoleClipped =
        {
            new(3, 0), new(2, -1), new(4, -1), new(4, 2), new(2, 2), new(3, 0)
        };

        Coordinate[] coordinatesShellCutting =
        {
            new(-4, -3), new(-4, 0), new(3, 0), new(3, -3), new(-4, -3)
        };

        LinearRing ringShellClipped = new LinearRing(coordinatesShellClipped);
        LinearRing[] ringsHolesClipped =
            { new(coordinatesFirstHoleClipped), new(coordinatesSecondHoleClipped) };
        Polygon clippedPolygon = new Polygon(ringShellClipped, ringsHolesClipped);

        LinearRing ringShellCutting = new LinearRing(coordinatesShellCutting);
        Polygon cuttingPolygon = new Polygon(ringShellCutting);

        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingPolygon);

        Polygon[] expected = new Polygon[] { new Polygon(new LinearRing(new Coordinate[]
        {
            new(3,-2), new(-2,-2), new(-3,0), new(3,0), new(2,-1), new(3,-1), new(3,-2)
        }))};

        Assert.Equal(expected,actual);
    }
}