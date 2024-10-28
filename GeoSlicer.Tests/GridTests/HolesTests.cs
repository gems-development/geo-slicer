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
            new(3,-2), new(-2,-2), new(-3,0), new(-2,0), new(0,0), 
            new(3,0), new(2,-1), new(3,-1), new(3,-2)
        }))};

        Assert.Equal(expected,actual);
    }
    
    
    [Fact]
    public void TwoHoles_OneResult_Test()
    {
        Coordinate[] coordinatesShellClipped =
        {
            new(-3,-4), new(-3,4), new(5,4), new(5,-4), new(-3,-4)
        };

        Coordinate[] coordinatesFirstHoleClipped =
        {
            new(0,1), new(-2,1), new(-2,-1), new(0,-1), new(0,1)
        };

        Coordinate[] coordinatesSecondHoleClipped =
        {
            new(2,1), new(2,-1), new(4,-1), new(4,1), new(2,1)
        };

        Coordinate[] coordinatesShellCutting =
        {
            new(-3,0), new(5,0), new(5,-2), new(-3,-2), new(-3,0)
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
            new(-2,0), new(-2,-1), new(0,-1), new(0,0), new(2,0), new(2,-1),
            new(4,-1), new(4,0), new(5,0), new(5,-2), new(-3,-2), new(-3,0), new(-2,0)
        }))};

        Assert.Equal(expected,actual);
    }
    
    [Fact]
    public void TwoHoles_OneHoleTangentCutting_Test()
    {
        Coordinate[] coordinatesShellClipped =
        {
            new(-4,-3), new(-4,5), new(4,5), new(4,-3), new(-4,-3)
        };

        Coordinate[] coordinatesFirstHoleClipped =
        {
            new(-2,0), new(-2,2), new(0,2), new(0,0), new(-2,0)
        };

        Coordinate[] coordinatesSecondHoleClipped =
        {
            new(2,3), new(2,4), new(3,4), new(3,3), new(2,3)
        };

        Coordinate[] coordinatesShellCutting =
        {
            new(-4,-3), new(-4,3), new(0,3), new(0,-3), new(-4,-3)
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
            new(0,2), new(-2,2), new(-2,0), new(0,0), new(0,-3), 
            new(-4,-3), new(-4,3), new(0,3), new(0,2)
        }))};

        Assert.Equal(expected,actual);
    }
    
    [Fact]
    public void TwoHoles_OneHoleInCutting_Test()
    {
        Coordinate[] coordinatesShellClipped =
        {
            new(-4,-3), new(-4,5), new(4,5), new(4,-3), new(-4,-3)
        };

        Coordinate[] coordinatesFirstHoleClipped =
        {
            new(-2,0), new(-2,2), new(0,2), new(0,0), new(-2,0)
        };

        Coordinate[] coordinatesSecondHoleClipped =
        {
            new(2,3), new(2,4), new(3,4), new(3,3), new(2,3)
        };

        Coordinate[] coordinatesShellCutting =
        {
            new(-4,-3), new(-4,3), new(1,3), new(1,-3), new(-4,-3)
        };

        LinearRing ringShellClipped = new LinearRing(coordinatesShellClipped);
        LinearRing[] ringsHolesClipped =
            { new(coordinatesFirstHoleClipped), new(coordinatesSecondHoleClipped) };
        Polygon clippedPolygon = new Polygon(ringShellClipped, ringsHolesClipped);

        LinearRing ringShellCutting = new LinearRing(coordinatesShellCutting);
        Polygon cuttingPolygon = new Polygon(ringShellCutting);

        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingPolygon);

        Polygon[] expected = { new Polygon(
                new LinearRing(new Coordinate[]
                {
                    new(-4, -3), new(-4, 3), new(1, 3), new(1, -3), new(-4, -3)
                }),
                new LinearRing[]{ new LinearRing(new Coordinate[]
                    {
                        new(-2, 0), new(0, 0), new(0, 2), new(-2, 2), new(-2, 0)
                    })
                }
            )
        };

        Assert.Equal(expected,actual);
    }
    
    [Fact]
    public void TwoHoles_ThreeHoles_HoleInResult_Test()
    {
        Coordinate[] coordinatesShellClipped =
        {
            new(-4,-4), new(-4,5), new(5,5), new(5,-4), new(-4,-4)
        };

        Coordinate[] coordinatesFirstHoleClipped =
        {
            new(-3,-3), new(-3,2), new(-1,2), new(-1,-3), new(-3,-3)
        };

        Coordinate[] coordinatesSecondHoleClipped =
        {
            new(0,-3), new(0,-1), new(1,-1), new(1,-3), new(0,-3)
        };
        
        Coordinate[] coordinatesThirdHoleClipped =
        {
            new(2,2), new(4,2), new(4,-3), new(2,-3), new(2,2)
        };

        Coordinate[] coordinatesShellCutting =
        {
            new(-6,0), new(4,0), new(4,-4), new(-6,-4), new(-6,0)
        };

        LinearRing ringShellClipped = new LinearRing(coordinatesShellClipped);
        LinearRing[] ringsHolesClipped =
            { new(coordinatesFirstHoleClipped), new(coordinatesSecondHoleClipped), new(coordinatesThirdHoleClipped) };
        Polygon clippedPolygon = new Polygon(ringShellClipped, ringsHolesClipped);

        LinearRing ringShellCutting = new LinearRing(coordinatesShellCutting);
        Polygon cuttingPolygon = new Polygon(ringShellCutting);

        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingPolygon);

        Polygon[] expected = { new Polygon(
            new LinearRing(new Coordinate[]
                {
                    new(-4, -4), new(-4, 0), new(-3, 0), new(-3, -3), new(-1, -3), 
                    new(-1, 0), new(2, 0), new(2, -3), new(4, -3), new(4, -4), new(-4, -4)
                }),
            new LinearRing[]{ new LinearRing(new Coordinate[]
                    {
                        new(0, -3), new(1, -3), new(1, -1), new(0, -1), new(0, -3)
                    })
                }
            )
        };

        Assert.Equal(expected,actual);
    }
    
    [Fact]
    public void TwoHoles_ThreeHoles_HoleInResult_CuttingInClipped_Test()
    {
        Coordinate[] coordinatesShellClipped =
        {
            new(-4,-4), new(-4,5), new(5,5), new(5,-4), new(-4,-4)
        };

        Coordinate[] coordinatesFirstHoleClipped =
        {
            new(-3,-3), new(-3,2), new(-1,2), new(-1,-3), new(-3,-3)
        };

        Coordinate[] coordinatesSecondHoleClipped =
        {
            new(0,-3), new(0,-1), new(1,-1), new(1,-3), new(0,-3)
        };
        
        Coordinate[] coordinatesThirdHoleClipped =
        {
            new(2,2), new(4,2), new(4,-3), new(2,-3), new(2,2)
        };

        Coordinate[] coordinatesShellCutting =
        {
            new(-4,0), new(4,0), new(4,-4), new(-4,-4), new(-4,0)
        };

        LinearRing ringShellClipped = new LinearRing(coordinatesShellClipped);
        LinearRing[] ringsHolesClipped =
            { new(coordinatesFirstHoleClipped), new(coordinatesSecondHoleClipped), new(coordinatesThirdHoleClipped) };
        Polygon clippedPolygon = new Polygon(ringShellClipped, ringsHolesClipped);

        LinearRing ringShellCutting = new LinearRing(coordinatesShellCutting);
        Polygon cuttingPolygon = new Polygon(ringShellCutting);

        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingPolygon);

        Polygon[] expected = { new Polygon(
                new LinearRing(new Coordinate[]
                {
                    new(-3, 0), new(-3, -3), new(-1, -3), new(-1, 0), new(2, 0), 
                    new(2, -3), new(4, -3), new(4, -4), new(-4, -4), new(-4, 0), new(-3,0)
                }),
                new LinearRing[]{ new LinearRing(new Coordinate[]
                    {
                        new(0, -3), new(1, -3), new(1, -1), new(0, -1), new(0, -3)
                    })
                }
            )
        };

        Assert.Equal(expected,actual);
    }
}