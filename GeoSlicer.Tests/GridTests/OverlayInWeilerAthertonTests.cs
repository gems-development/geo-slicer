using System.Collections.Generic;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.PolygonClippingAlghorithm;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.GridTests;

public class OverlayInWeilerAthertonTests
{
    private static readonly double Epsilon = 1E-9;

    private static readonly LineService LineService = new LineService(Epsilon);

    private static readonly WeilerAthertonAlghorithm SlicerHelper =
        new(new LinesIntersector(new EpsilonCoordinateComparator(Epsilon), LineService, Epsilon), LineService,
            new EpsilonCoordinateComparator(), new ContainsChecker(LineService, Epsilon));

/*
    [Theory]
    [MemberData(nameof(Data.DataInner), MemberType = typeof(Data))]
    [MemberData(nameof(Data.DataTangent), MemberType = typeof(Data))]
    [MemberData(nameof(Data.DataResRectangle), MemberType = typeof(Data))]
    private void Test(List<Coordinate> clipped, List<Coordinate> cutting, List<Coordinate> expected)
    {
        List<List<Coordinate>> actual = Slicer.WeilerAtherton(clipped, cutting);

        Assert.Single(actual);
        Assert.True(actual[0].IsEqualsRing(expected));
    }
*/
    private static class Data
    {
        public static IEnumerable<IEnumerable<List<Coordinate>>> DataInner =>
            new[]
            {
                new[]
                {
                    new List<Coordinate>()
                    {
                        new Coordinate(-4, 2),
                        new Coordinate(0, 2),
                        new Coordinate(4, 4),
                        new Coordinate(2, 0),
                        new Coordinate(2, -4),
                        new Coordinate(-2, -4),
                        new Coordinate(-2, 0),
                        new Coordinate(-4, -2),
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(-2, 2),
                        new Coordinate(2, 2),
                        new Coordinate(2, -2),
                        new Coordinate(-2, -2),
                        new Coordinate(-2, 2)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(-2, 2),
                        new Coordinate(0, 2),
                        new Coordinate(2, 2),
                        new Coordinate(2, 0),
                        new Coordinate(2, -2),
                        new Coordinate(-2, -2),
                        new Coordinate(-2, 0),
                        new Coordinate(-2, 2)
                    }
                },
                new[]
                {
                    new List<Coordinate>()
                    {
                        new Coordinate(-2, 2),
                        new Coordinate(2, 2),
                        new Coordinate(2, -2),
                        new Coordinate(-2, -2),
                        new Coordinate(-2, 2)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(-4, 2),
                        new Coordinate(0, 2),
                        new Coordinate(4, 4),
                        new Coordinate(2, 0),
                        new Coordinate(2, -4),
                        new Coordinate(-2, -4),
                        new Coordinate(-2, 0),
                        new Coordinate(-4, -2),
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(-2, 2),
                        new Coordinate(0, 2),
                        new Coordinate(2, 2),
                        new Coordinate(2, 0),
                        new Coordinate(2, -2),
                        new Coordinate(-2, -2),
                        new Coordinate(-2, 0),
                        new Coordinate(-2, 2)
                    }
                },
                new[]
                {
                    new List<Coordinate>()
                    {
                        new Coordinate(2, -4),
                        new Coordinate(2, 0),
                        new Coordinate(4, 4),
                        new Coordinate(0, 2),
                        new Coordinate(-4, 2),
                        new Coordinate(-4, -2),
                        new Coordinate(0, -2),
                        new Coordinate(-2, -4),
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(2, -2),
                        new Coordinate(2, 2),
                        new Coordinate(-2, 2),
                        new Coordinate(-2, -2),
                        new Coordinate(2, -2)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(2, -2),
                        new Coordinate(2, 0),
                        new Coordinate(2, 2),
                        new Coordinate(0, 2),
                        new Coordinate(-2, 2),
                        new Coordinate(-2, -2),
                        new Coordinate(0, -2),
                        new Coordinate(2, -2)
                    }
                },
                new[]
                {
                    new List<Coordinate>()
                    {
                        new Coordinate(2, -2),
                        new Coordinate(2, 2),
                        new Coordinate(-2, 2),
                        new Coordinate(-2, -2),
                        new Coordinate(2, -2)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(2, -4),
                        new Coordinate(2, 0),
                        new Coordinate(4, 4),
                        new Coordinate(0, 2),
                        new Coordinate(-4, 2),
                        new Coordinate(-4, -2),
                        new Coordinate(0, -2),
                        new Coordinate(-2, -4),
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(2, -2),
                        new Coordinate(2, 0),
                        new Coordinate(2, 2),
                        new Coordinate(0, 2),
                        new Coordinate(-2, 2),
                        new Coordinate(-2, -2),
                        new Coordinate(0, -2),
                        new Coordinate(2, -2)
                    }
                },
            };

        public static IEnumerable<IEnumerable<List<Coordinate>>> DataTangent =>
            new[]
            {
                new[]
                {
                    new List<Coordinate>()
                    {
                        new Coordinate(-2, 1),
                        new Coordinate(-1, 2),
                        new Coordinate(0, 1),
                        new Coordinate(-2, 1)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(-1, 1),
                        new Coordinate(1, 1),
                        new Coordinate(0, 0),
                        new Coordinate(-1, 1)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(-1, 1),
                        new Coordinate(0, 1),
                        new Coordinate(-1, 1),
                    }
                },
                new[]
                {
                    new List<Coordinate>()
                    {
                        new Coordinate(-1, 1),
                        new Coordinate(1, 1),
                        new Coordinate(0, 0),
                        new Coordinate(-1, 1)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(-2, 1),
                        new Coordinate(-1, 2),
                        new Coordinate(0, 1),
                        new Coordinate(-2, 1)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(-1, 1),
                        new Coordinate(0, 1),
                        new Coordinate(-1, 1),
                    }
                },
                new[]
                {
                    new List<Coordinate>()
                    {
                        new Coordinate(-1, 1),
                        new Coordinate(1, 1),
                        new Coordinate(0, 0),
                        new Coordinate(-1, 1)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(0, 1),
                        new Coordinate(1, 2),
                        new Coordinate(2, 1),
                        new Coordinate(0, 1)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(1, 1),
                        new Coordinate(0, 1),
                        new Coordinate(1, 1),
                    }
                },
                new[]
                {
                    new List<Coordinate>()
                    {
                        new Coordinate(0, 1),
                        new Coordinate(1, 2),
                        new Coordinate(2, 1),
                        new Coordinate(0, 1)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(-1, 1),
                        new Coordinate(1, 1),
                        new Coordinate(0, 0),
                        new Coordinate(-1, 1)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(1, 1),
                        new Coordinate(0, 1),
                        new Coordinate(1, 1),
                    }
                },
                new[]
                {
                    new List<Coordinate>()
                    {
                        new Coordinate(1, -2),
                        new Coordinate(2, -1),
                        new Coordinate(1, 0),
                        new Coordinate(1, -2)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(1, -1),
                        new Coordinate(1, 1),
                        new Coordinate(0, 0),
                        new Coordinate(1, -1)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(1, -1),
                        new Coordinate(1, 0),
                        new Coordinate(1, -1),
                    }
                },
                new[]
                {
                    new List<Coordinate>()
                    {
                        new Coordinate(1, -1),
                        new Coordinate(1, 1),
                        new Coordinate(0, 0),
                        new Coordinate(1, -1)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(1, -2),
                        new Coordinate(2, -1),
                        new Coordinate(1, 0),
                        new Coordinate(1, -2)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(1, -1),
                        new Coordinate(1, 0),
                        new Coordinate(1, -1),
                    }
                }
            };


        public static IEnumerable<IEnumerable<List<Coordinate>>> DataResRectangle =>
            new[]
            {
                new[]
                {
                    new List<Coordinate>()
                    {
                        new Coordinate(-1, -1),
                        new Coordinate(-1, 2),
                        new Coordinate(1, 2),
                        new Coordinate(1, -1),
                        new Coordinate(-1, -1)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(0, 2),
                        new Coordinate(2, 2),
                        new Coordinate(2, 0),
                        new Coordinate(0, 0),
                        new Coordinate(0, 2)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(0, 2),
                        new Coordinate(1, 2),
                        new Coordinate(1, 0),
                        new Coordinate(0, 0),
                        new Coordinate(0, 2)
                    }
                },
                new[]
                {
                    new List<Coordinate>()
                    {
                        new Coordinate(0, 2),
                        new Coordinate(2, 2),
                        new Coordinate(2, 0),
                        new Coordinate(0, 0),
                        new Coordinate(0, 2)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(-1, -1),
                        new Coordinate(-1, 2),
                        new Coordinate(1, 2),
                        new Coordinate(1, -1),
                        new Coordinate(-1, -1)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(0, 2),
                        new Coordinate(1, 2),
                        new Coordinate(1, 0),
                        new Coordinate(0, 0),
                        new Coordinate(0, 2)
                    }
                },
                new[]
                {
                    new List<Coordinate>()
                    {
                        new Coordinate(-2, 2),
                        new Coordinate(0, 2),
                        new Coordinate(0, 0),
                        new Coordinate(-2, 0),
                        new Coordinate(-2, 2)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(-1, -1),
                        new Coordinate(-1, 2),
                        new Coordinate(1, 2),
                        new Coordinate(1, -1),
                        new Coordinate(-1, -1)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(-1, 2),
                        new Coordinate(0, 2),
                        new Coordinate(0, 0),
                        new Coordinate(-1, 0),
                        new Coordinate(-1, 2)
                    }
                },
                new[]
                {
                    new List<Coordinate>()
                    {
                        new Coordinate(-1, -1),
                        new Coordinate(-1, 2),
                        new Coordinate(1, 2),
                        new Coordinate(1, -1),
                        new Coordinate(-1, -1)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(-2, 2),
                        new Coordinate(0, 2),
                        new Coordinate(0, 0),
                        new Coordinate(-2, 0),
                        new Coordinate(-2, 2)
                    },
                    new List<Coordinate>()
                    {
                        new Coordinate(-1, 2),
                        new Coordinate(0, 2),
                        new Coordinate(0, 0),
                        new Coordinate(-1, 0),
                        new Coordinate(-1, 2)
                    }
                },
            };
    }
}