using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.GridTests;

public class OverlayInWeilerAthertonTests
{
    private static readonly double Epsilon = 1E-9;

    private static readonly LineService LineService = new LineService(Epsilon);

    private static readonly GridSlicer.GridSlicer Slicer =
        new(new LinesIntersector(new EpsilonCoordinateComparator(Epsilon), LineService, Epsilon), Epsilon, LineService);


    [Theory]
    [MemberData(nameof(Data.DataTestInner), MemberType = typeof(Data))]
    private void TestInner(List<Coordinate> clipped, List<Coordinate> cutting, List<Coordinate> expected)
    {
        List<List<Coordinate>> actual = Slicer.WeilerAtherton(clipped, cutting);

        Assert.Single(actual);
        Assert.True(actual[0].IsEqualsRing(expected));
    }

    private static class Data
    {
        public static IEnumerable<IEnumerable<int?[]>> DataContainsIEnumerableTrue =>
            new[]
            {
                new[] { new int?[] { 1, null, 3, 4, 5 }, new int?[] { 1, null } },
                new[] { new int?[] { 1, null, 3, 4, 5 }, new int?[] { 3, 4 } },
                new[] { new int?[] { 1, null, 3, 4, 5 }, new int?[] { 4, 5 } },
                new[] { new int?[] { null, null, 3, 4, null }, new int?[] { 3, 4 } },
                new[] { new int?[] { 1, 2, 1, 2, 4, 2, 1, 2, 1, 2, 1, 3, 2, 1, 2, 1, 4 }, new int?[] { 1, 2, 1, 3 } }
            };

        public static IEnumerable<IEnumerable<List<Coordinate>>> DataTestInner =>
            new[]
            {
                new[]
                {
                    new List<Coordinate>()
                    {
                        new Coordinate(-2, 3),
                        new Coordinate(2, 3),
                        new Coordinate(3, 5),
                        new Coordinate(5, 5),
                        new Coordinate(5, -2),
                        new Coordinate(-2, -2),
                        new Coordinate(-2, 3)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(0, 3),
                        new Coordinate(3, 3),
                        new Coordinate(3, 1),
                        new Coordinate(0, 3)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(0, 3),
                        new Coordinate(3, 3),
                        new Coordinate(3, 1),
                        new Coordinate(0, 3)
                    }
                }
            };
    }
}