using System.Collections.Generic;
using System.Linq;
using GeoSlicer.DivideAndRuleSlicers.OppositesSlicer;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.PolygonClippingAlghorithm;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.WeilerAthertonTests;

public class MultiIntersects
{
    private const double Epsilon = 1E-15;

    private static readonly EpsilonCoordinateComparator CoordinateComparator = new EpsilonCoordinateComparator(1e-8);
    private static readonly LineService LineService = new LineService(Epsilon, CoordinateComparator);


    private static readonly WeilerAthertonForLine WeilerAtherton = new WeilerAthertonForLine(
        new LinesIntersector(CoordinateComparator, LineService, Epsilon), LineService,
        CoordinateComparator, new ContainsChecker(LineService, Epsilon), Epsilon);

    [Fact]
    public void Horizontal_IntersectsAtZero()
    {
        Polygon polygon = new Polygon(
            new LinearRing(new Coordinate[]
            {
                new(-4, -4), new(-4, 0), new(-4, 4), new(-1, 4), new(-1, -3), new(1, -3), new(1, 4), new(4, 4), new(4, 0), new(4, -4), new(-4, -4)
            }),
            new[]
            {
                new LinearRing(new Coordinate[]
                {
                    new(-2, 3), new(-3, 3), new(-3, -3), new(-2, -3), new(-2, 3)
                }),
                new LinearRing(new Coordinate[]
                {
                    new(3, 3), new(2, 3), new(2, -3), new(3, -3), new(3, 3)
                })
            });

        LineString lineA = new LineString(new Coordinate[] { new(-4, 0), new(4, 0) });
        LineString lineB = new LineString(new Coordinate[] { new(4, 0), new(-4, 0) });

        Polygon expectedA = new Polygon(
            new LinearRing(new Coordinate[]
            {
                new(-4, 0), new(-3, 0), new(-3, -3), new(-2, -3), new(-2, 0), new(-1, 0), new(-1, -3), new(1, -3), new(1, 0), new(2, 0), new(2, -3), new(3, -3), new(3, 0), new (4, 0), new(4, -4), new(-4, -4), new(-4, 0)
            }));
        Polygon[] expectedB = new Polygon[] {
            new Polygon(
                new LinearRing(new Coordinate[]
            {
                new(-4, 0), new(-4, 4), new(-1, 4), new(-1, 0), new(-2, 0), new(-2, 3), new(-3, 3), new(-3, 0), new(-4, 0)
            })),
            new Polygon(
                new LinearRing(new Coordinate[]
            {
                new(1, 0), new(1, 4), new(4, 4), new(4, 0), new(3, 0), new(3, 3), new(2, 3), new(2, 0), new(1, 0)
            })),
            
        };

        //Act
        Polygon[] actualA = WeilerAtherton.WeilerAtherton(polygon, lineA);
        Polygon[] actualB = WeilerAtherton.WeilerAtherton(polygon, lineB);

        //Assert
        Assert.Single(actualA);
        Assert.True(expectedA.IsEqualsPolygons(actualA[0]));
        Assert.True(expectedB.IsEqualsPolygonCollections(actualB));
    }
    
    [Fact]
    public void Horizontal_IntersectsAtNotZero()
    {
        Polygon polygon = new Polygon(
            new LinearRing(new Coordinate[]
            {
                new(-4, -6), new(-4, -2), new(-4, 2), new(-1, 2), new(-1, -5), new(1, -5), new(1, 2), new(4, 2), new(4, -2), new(4, -6), new(-4, -6)
            }),
            new[]
            {
                new LinearRing(new Coordinate[]
                {
                    new(-2, 1), new(-3, 1), new(-3, -5), new(-2, -5), new(-2, 1)
                }),
                new LinearRing(new Coordinate[]
                {
                    new(3, 1), new(2, 1), new(2, -5), new(3, -5), new(3, 1)
                })
            });

        LineString lineA = new LineString(new Coordinate[] { new(-4, -2), new(4, -2) });
        LineString lineB = new LineString(new Coordinate[] { new(4, -2), new(-4, -2) });

        Polygon expectedA = new Polygon(
            new LinearRing(new Coordinate[]
            {
                new(-4, -2), new(-3, -2), new(-3, -5), new(-2, -5), new(-2, -2), new(-1, -2), new(-1, -5), new(1, -5), new(1, -2), new(2, -2), new(2, -5), new(3, -5), new(3, -2), new (4, -2), new(4, -6), new(-4, -6), new(-4, -2)
            }));
        Polygon[] expectedB = new Polygon[] {
            new Polygon(
                new LinearRing(new Coordinate[]
            {
                new(-4, -2), new(-4, 2), new(-1, 2), new(-1, -2), new(-2, -2), new(-2, 1), new(-3, 1), new(-3, -2), new(-4, -2)
            })),
            new Polygon(
                new LinearRing(new Coordinate[]
            {
                new(1, -2), new(1, 2), new(4, 2), new(4, -2), new(3, -2), new(3, 1), new(2, 1), new(2, -2), new(1, -2)
            })),
            
        };

        //Act
        Polygon[] actualA = WeilerAtherton.WeilerAtherton(polygon, lineA);
        Polygon[] actualB = WeilerAtherton.WeilerAtherton(polygon, lineB);

        //Assert
        Assert.Single(actualA);
        Assert.True(expectedA.IsEqualsPolygons(actualA[0]));
        Assert.True(expectedB.IsEqualsPolygonCollections(actualB));
    }
}