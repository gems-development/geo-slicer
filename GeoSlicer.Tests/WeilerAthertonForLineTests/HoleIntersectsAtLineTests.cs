using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.PolygonClippingAlghorithm;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.WeilerAthertonForLineTests;

public class HoleIntersectsAtLineTests
{
    private const double Epsilon = 1E-15;

    private static readonly EpsilonCoordinateComparator CoordinateComparator = new EpsilonCoordinateComparator(1e-8);
    private static readonly LineService LineService = new LineService(Epsilon, CoordinateComparator);


    private static readonly WeilerAthertonForLine WeilerAtherton = new WeilerAthertonForLine(
        new LinesIntersector(CoordinateComparator, LineService, Epsilon), LineService,
        CoordinateComparator, new ContainsChecker(LineService, Epsilon), Epsilon);


    [Fact]
    // Дыры пересекаются с линией по одному из своих отрезков, не заходя за нее
    public void Diagonal_OnOneSide()
    {
        Polygon polygon = new Polygon(
            new LinearRing(new Coordinate[]
            {
                new(-4, 4), new(4, 4), new(4, -4), new(-4, -4), new(-4, 4)
            }),
            new[]
            {
                new LinearRing(new Coordinate[]
                {
                    new(-2, -2), new(-1, -1), new(-3, 0), new(-2, -2)
                }),
                new LinearRing(new Coordinate[]
                {
                    new(0, 0), new(2, -1), new(1, 1), new(0, 0)
                })
            });

        LineString lineA = new LineString(new Coordinate[] { new(-4, -4), new(4, 4) });
        LineString lineB = new LineString(new Coordinate[] { new(4, 4), new(-4, -4) });

        Polygon expectedA = new Polygon(
            new LinearRing(new Coordinate[]
            {
                new(4, 4), new(4, -4), new(-4, -4), new(0, 0), new(2, -1), new(1, 1), new(4, 4)
            }));
        Polygon expectedB = new Polygon(
            new LinearRing(new Coordinate[]
            {
                new(-4, 4), new(4, 4), new(-1, -1), new(-3, 0), new(-2, -2), new(-4, -4), new(-4, 4)
            }));

        //Act
        Polygon[] actualA = WeilerAtherton.WeilerAtherton(polygon, lineA);
        Polygon[] actualB = WeilerAtherton.WeilerAtherton(polygon, lineB);

        //Assert
        Assert.Single(actualA);
        Assert.Single(actualB);

        Assert.True(expectedA.IsEqualsPolygons(actualA[0]));
        Assert.True(expectedB.IsEqualsPolygons(actualB[0]));
    }

    [Fact]
    // Дыры пересекаются с линией, заходя за нее
    public void Diagonal_OnTwoSide()
    {
        Polygon polygon = new Polygon(
            new LinearRing(new Coordinate[]
            {
                new(-4, 4), new(4, 4), new(4, -4), new(-4, -4), new(-4, 4)
            }),
            new[]
            {
                new LinearRing(new Coordinate[]
                {
                    new(0, 0), new(0, -1), new(3, 2), new(0, 2), new(1, 1), new(0, 0)
                }),
                new LinearRing(new Coordinate[]
                {
                    new(0, 0), new(-2, 0), new(-3, -3), new(-2, -2), new(-1, -2), new(-1, -1), new(0, 0)
                })
            });

        LineString lineA = new LineString(new Coordinate[] { new(-4, -4), new(4, 4) });
        LineString lineB = new LineString(new Coordinate[] { new(4, 4), new(-4, -4) });

        Polygon expectedA = new Polygon(
            new LinearRing(new Coordinate[]
            {
                new(4, 4), new(4, -4), new(-4, -4), new(-2, -2), new(-1, -2), new(-1, -1), new(0, 0), new(0, -1),
                new(3, 2), new(2, 2), new(4, 4)
            }));
        Polygon expectedB = new Polygon(
            new LinearRing(new Coordinate[]
            {
                new(-4, 4), new(4, 4), new(2, 2), new(0, 2), new(1, 1), new(0, 0), new(-2, 0), new(-3, -3), new(-4, -4),
                new(-4, 4)
            }));

        //Act
        Polygon[] actualA = WeilerAtherton.WeilerAtherton(polygon, lineA);
        Polygon[] actualB = WeilerAtherton.WeilerAtherton(polygon, lineB);

        //Assert
        Assert.Single(actualA);
        Assert.Single(actualB);

        Assert.True(expectedA.IsEqualsPolygons(actualA[0]));
        Assert.True(expectedB.IsEqualsPolygons(actualB[0]));
    }

    [Fact]
    // Дыры пересекаются с линией, разделяя геометрию на несколько
    public void Diagonal_TwoPart()
    {
        Polygon polygon = new Polygon(
            new LinearRing(new Coordinate[]
            {
                new(-4, 4), new(4, 4), new(4, -4), new(-4, -4), new(-4, 4)
            }),
            new[]
            {
                new LinearRing(new Coordinate[]
                {
                    new(1, 1), new(0, 0), new(0, -1), new(-1, -1), new(1, -2), new(1, 1)
                }),
                new LinearRing(new Coordinate[]
                {
                    new(3, 3), new(-3, 3), new(-3, -3), new(-2, -2), new(-2, -1), new(1, 2), new(2, 2), new(3, 1),
                    new(3, 3)
                })
            });

        LineString lineA = new LineString(new Coordinate[] { new(-4, -4), new(4, 4) });
        LineString lineB = new LineString(new Coordinate[] { new(4, 4), new(-4, -4) });

        Polygon[] expectedA = new Polygon[]
        {
            new Polygon(
                new LinearRing(new Coordinate[]
                {
                    new(4, 4), new(4, -4), new(-4, -4), new(-1, -1), new(1, -2), new(1, 1), new(2, 2), new(3, 1), new(3, 3), new(4, 4)
                })),
            new Polygon(
                new LinearRing(new Coordinate[]
                {
                    new(0, 0), new(0, -1), new(-1, -1), new(0, 0)
                }))
        };
        
        Polygon[] expectedB = new Polygon[]
        {
            new Polygon(
                new LinearRing(new Coordinate[]
                {
                    new(-4, 4), new(4, 4), new(3, 3), new(-3, 3), new(-3, -3), new(-4, -4), new(-4, 4)
                })),
            new Polygon(
                new LinearRing(new Coordinate[]
                {
                    new(2, 2), new(-2, -2), new(-2, -1), new(1, 2), new(2, 2)
                }))
        };


        //Act
        Polygon[] actualA = WeilerAtherton.WeilerAtherton(polygon, lineA);
        Polygon[] actualB = WeilerAtherton.WeilerAtherton(polygon, lineB);
        
        //Assert
        Assert.True(expectedA.IsEqualsPolygonCollections(actualA));
        Assert.True(expectedB.IsEqualsPolygonCollections(actualB));
    }
}