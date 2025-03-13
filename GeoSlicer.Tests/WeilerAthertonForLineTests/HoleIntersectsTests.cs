using System.Collections.Generic;
using System.Linq;
using GeoSlicer.DivideAndRuleSlicers.OppositesSlicer;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.PolygonClippingAlghorithm;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.WeilerAthertonTests;

public class HoleIntersectsTests
{
    private const double Epsilon = 1E-15;

    private static readonly EpsilonCoordinateComparator CoordinateComparator = new EpsilonCoordinateComparator(1e-8);
    private static readonly LineService LineService = new LineService(Epsilon, CoordinateComparator);


    private static readonly WeilerAthertonForLine WeilerAtherton = new WeilerAthertonForLine(
        new LinesIntersector(CoordinateComparator, LineService, Epsilon), LineService,
        CoordinateComparator, new ContainsChecker(LineService, Epsilon), Epsilon);

    [Fact]
    public void Diagonal_IntersectsAtZero()
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
                    new(0, 2), new(0, -2), new(3, -2), new(3, 2), new(0, 2)
                })
            });

        LineString lineA = new LineString(new Coordinate[] { new(-4, -4), new(4, 4) });
        LineString lineB = new LineString(new Coordinate[] { new(4, 4), new(-4, -4) });

        Polygon expectedA = new Polygon(
            new LinearRing(new Coordinate[]
            {
                new(4, 4), new(4, -4), new(-4, -4), new(0, 0), new(0, -2), new(3, -2), new(3, 2), new(2, 2), new(4, 4)
            }));
        Polygon expectedB = new Polygon(
            new LinearRing(new Coordinate[]
            {
                new(-4, 4), new(4, 4), new(2, 2), new(0, 2), new(0, 0), new(-4, -4), new(-4, 4)
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
}