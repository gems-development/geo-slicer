﻿using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.PolygonClippingAlgorithm;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.WeilerAthertonTests;

public class HolesTests
{
    private static readonly double Epsilon = 1E-9;
 
    private static readonly LineService LineService = new(Epsilon, new EpsilonCoordinateComparator(Epsilon));
 
    private static readonly WeilerAthertonAlgorithm SlicerHelper =
        new(new LinesIntersector(new EpsilonCoordinateComparator(Epsilon), LineService, Epsilon), LineService,
            new EpsilonCoordinateComparator(), new ContainsChecker(LineService, Epsilon));

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
        LinearRing cuttingRing = new LinearRing(coordinatesShellCutting);

        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingRing);

        Polygon[] expected = new Polygon[] { new Polygon(new LinearRing(new Coordinate[]
        {
            new(3,-2), new(-2,-2), new(-3,0), new(-2,0), new(0,0), 
            new(3,0), new(2,-1), new(3,-1), new(3,-2)
        }))};

        Assert.True(expected.IsEqualsPolygonCollections(actual));
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
        LinearRing cuttingRing = new LinearRing(coordinatesShellCutting);

        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingRing);

        Polygon[] expected = new Polygon[] { new Polygon(new LinearRing(new Coordinate[]
        {
            new(-2,0), new(-2,-1), new(0,-1), new(0,0), new(2,0), new(2,-1),
            new(4,-1), new(4,0), new(5,0), new(5,-2), new(-3,-2), new(-3,0), new(-2,0)
        }))};

        Assert.True(expected.IsEqualsPolygonCollections(actual));
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

        LinearRing cuttingRing = new LinearRing(coordinatesShellCutting);

        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingRing);

        Polygon[] expected = new Polygon[] { new Polygon(new LinearRing(new Coordinate[]
        {
            new(0,2), new(-2,2), new(-2,0), new(0,0), new(0,-3), 
            new(-4,-3), new(-4,3), new(0,3), new(0,2)
        }))};

        Assert.True(expected.IsEqualsPolygonCollections(actual));
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

        LinearRing cuttingRing = new LinearRing(coordinatesShellCutting);

        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingRing);

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

        Assert.True(expected.IsEqualsPolygonCollections(actual));
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

        LinearRing cuttingRing = new LinearRing(coordinatesShellCutting);

        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingRing);

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

        Assert.True(expected.IsEqualsPolygonCollections(actual));
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

        LinearRing cuttingRing = new LinearRing(coordinatesShellCutting);

        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingRing);

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

        Assert.True(expected.IsEqualsPolygonCollections(actual));
    }
    
    [Fact]
    public void TwoHoles_TwoPolygons_Test()
    {
        Coordinate[] coordinatesShellClipped =
        {
            new(5,-2), new(-5,-2), new(-5,8), new(5,8), new(5,-2)
        };

        Coordinate[] coordinatesFirstHoleClipped =
        {
            new(4,-1), new(-4,-1), new(-4,2), new(4,2), new(4,-1)
        };

        Coordinate[] coordinatesSecondHoleClipped =
        {
            new(-1, 4), new(-4, 4), new(-4, 6), new(-1, 6), new(-1, 4)
        };

        Coordinate[] coordinatesShellCutting =
        {
            new(2,7), new(2,-4), new(-3,-4), new(-3,7), new(2,7)
        };

        LinearRing ringShellClipped = new LinearRing(coordinatesShellClipped);
        LinearRing[] ringsHolesClipped =
            { new(coordinatesFirstHoleClipped), new(coordinatesSecondHoleClipped) };
        Polygon clippedPolygon = new Polygon(ringShellClipped, ringsHolesClipped);

        LinearRing cuttingRing = new LinearRing(coordinatesShellCutting);

        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingRing);

        Polygon[] expected = { new Polygon(
                new LinearRing(new Coordinate[]
                {
                    new(2,-2), new(-3,-2), new(-3,-1), new(2,-1), new(2,-2)
                })
            ),
            new Polygon(new LinearRing(new Coordinate[]
                {
                    new(-3,6), new(-3,7), new(2,7), new(2,2), new(-3,2), new(-3, 4), new(-1, 4), new(-1, 6), new(-3, 6)
                })
            )
        };

        Assert.True(expected.IsEqualsPolygonCollections(actual));
    }
    [Fact]
    
    public void UFormHole_TwoResults_Test()
    {
        Coordinate[] coordinatesShellClipped =
        {
            new(-3, -2), new(-3, 5), new(4, 5), new(4, -2), new(-3, -2)
        };

        Coordinate[] coordinatesFirstHoleClipped =
        {
            new(3,-1), new(-2,-1), new(-2,3), new(1,3), new(1,2), new(-1, 2), new(-1, 0), new(3, 0), new(3, -1)
        };

        Coordinate[] coordinatesShellCutting =
        {
            new(0,4), new(2,4), new(2,-3), new(0,-3), new(0,4)
        };

        LinearRing ringShellClipped = new LinearRing(coordinatesShellClipped);
        LinearRing[] ringsHolesClipped =
            { new(coordinatesFirstHoleClipped) };
        Polygon clippedPolygon = new Polygon(ringShellClipped, ringsHolesClipped);

        LinearRing cuttingRing = new LinearRing(coordinatesShellCutting);

        var actual = SlicerHelper.WeilerAtherton(clippedPolygon, cuttingRing);

        Polygon[] expected = { new Polygon(
                new LinearRing(new Coordinate[]
                {
                    new(0, -2), new(0, -1), new(2, -1), new(2, -2), new(0, -2)
                })
            ),
            new Polygon(new LinearRing(new Coordinate[]
            {
                new(0, 0), new(0, 2), new(1, 2), new(1, 3), new(0, 3), new(0, 4), new(2, 4), new(2, 0), new(0, 0)
            }))
        };

        Assert.True(expected.IsEqualsPolygonCollections(actual));
    }
}