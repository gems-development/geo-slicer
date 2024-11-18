using System;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.Validators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.UtilsTests.ValidatorsTests;

public class RepeatingPointsValidatorTests
{
    private const double Epsilon = 1E-3;
    private readonly RepeatingPointsValidator _repeatingPointsValidator = new(new EpsilonCoordinateComparator(Epsilon));

    [Theory]
    [MemberData(nameof(Data.DataCheckValid), MemberType = typeof(Data))]
    private void TestCheckValid(LineString checkable)
    {
        Assert.Empty(_repeatingPointsValidator.GetErrorsString(checkable));
        Assert.True(_repeatingPointsValidator.IsValid(checkable));
    }

    [Theory]
    [MemberData(nameof(Data.DataCheckFail), MemberType = typeof(Data))]
#pragma warning disable xUnit1026
    private void TestCheckFail(LineString checkable, int _)
#pragma warning restore xUnit1026
    {
        String result = _repeatingPointsValidator.GetErrorsString(checkable);
        Assert.NotEmpty(result);
        Assert.False(_repeatingPointsValidator.IsValid(checkable));
        Assert.Equal(1, result.Count(c => c == '\n'));
    }

    [Theory]
    [MemberData(nameof(Data.DataCheckFail), MemberType = typeof(Data))]
    private void TestCheckFailFull(LineString checkable, int errorsCount)
    {
        String result = _repeatingPointsValidator.GetErrorsString(checkable, true);
        Assert.NotEmpty(result);
        Assert.False(_repeatingPointsValidator.IsValid(checkable));
        Assert.Equal(errorsCount, result.Count(c => c == '\n'));
    }

    private static class Data
    {
        public static IEnumerable<IEnumerable<LineString>> DataCheckValid =>
            new[]
            {
                new[]
                {
                    new LineString(new[]
                    {
                        new Coordinate(-4, 2),
                        new Coordinate(0, 2),
                        new Coordinate(4, 4),
                        new Coordinate(2, 0),
                        new Coordinate(2, -4),
                        new Coordinate(-2, -4),
                        new Coordinate(-2, 0),
                        new Coordinate(-4, -2),
                    })
                },
                new[]
                {
                    new LineString(new[]
                    {
                        new Coordinate(-2, 2),
                        new Coordinate(2, 2),
                        new Coordinate(2, -2),
                        new Coordinate(-2, -2),
                        new Coordinate(-2, 2)
                    })
                },
                new LineString[]
                {
                    new LinearRing(new[]
                    {
                        new Coordinate(-2, 2),
                        new Coordinate(0, 2),
                        new Coordinate(2, 2),
                        new Coordinate(2, 0),
                        new Coordinate(2, -2),
                        new Coordinate(-2, -2),
                        new Coordinate(-2, 0),
                        new Coordinate(-2, 2)
                    })
                }
            };

        public static IEnumerable<IEnumerable<Object>> DataCheckFail =>
            new[]
            {
                new Object[]
                {
                    new LineString(new[]
                    {
                        new Coordinate(-4, 2),
                        new Coordinate(-4, 2),
                        new Coordinate(0, 2),
                        new Coordinate(0, 2),
                        new Coordinate(4, 4),
                        new Coordinate(2, 0),
                        new Coordinate(2, 0),
                        new Coordinate(2, 0),
                        new Coordinate(2, -4),
                        new Coordinate(-2, -4),
                        new Coordinate(-2, 0),
                        new Coordinate(-2, 0),
                        new Coordinate(-4, -2),
                        new Coordinate(-4, -2),
                    }),
                    6
                },
                new Object[]
                {
                    new LineString(new[]
                    {
                        new Coordinate(-2, 2),
                        new Coordinate(-2, 2),
                        new Coordinate(2, 2),
                        new Coordinate(2, -2),
                        new Coordinate(-2, -2),
                        new Coordinate(-2, 2)
                    }),
                    1
                },
                new Object[]
                {
                    new LinearRing(new[]
                    {
                        new Coordinate(-2, 2),
                        new Coordinate(0, 2),
                        new Coordinate(2, 2),
                        new Coordinate(2, 0),
                        new Coordinate(2, -2),
                        new Coordinate(-2, -2),
                        new Coordinate(-2, 0),
                        new Coordinate(-2, 2),
                        new Coordinate(-2, 2)
                    }),
                    1
                }
            };
    }
}