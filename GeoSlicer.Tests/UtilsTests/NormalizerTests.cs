using System;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Utils;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.UtilsTests;

public class NormalizerTests
{
    private static readonly GeometryFactory Gf =
        NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);

    private static readonly Normalizer Normalizer = new Normalizer();

    public NormalizerTests()
    {
        Normalizer.Fit(Data.SampleCoordinates);
    }

    [Fact]
    private void TestShiftedWillBeLess()
    {
        IEnumerable<Coordinate> geometry = Data.SampleCoordinates;
        IEnumerable<Coordinate> copy = Data.SampleCoordinates;
        Normalizer.Shift(geometry);
        double absSumShifted = geometry.Select(coordinate => Math.Abs(coordinate.X) + Math.Abs(coordinate.Y)).Sum();
        double absSumNotShifted = copy.Select(coordinate => Math.Abs(coordinate.X) + Math.Abs(coordinate.Y)).Sum();
        Assert.True(absSumNotShifted / 2 > absSumShifted);
    }

    [Fact]
    private void TestShiftThenUnshiftIEnumerable()
    {
        IEnumerable<Coordinate> geometry = Data.SampleCoordinates;
        IEnumerable<Coordinate> copy = Data.SampleCoordinates;
        Assert.Equal(copy, geometry);
        Normalizer.Shift(geometry);
        Assert.NotEqual(copy, geometry);
        Normalizer.Shift(geometry, isBack: true);
        Assert.Equal(copy, geometry);
    }

    [Theory]
    [MemberData(nameof(Data.DataExtendsLineString), MemberType = typeof(Data))]
    private void TestShiftThenUnshiftExtendsLineString(LineString lineString, LineString copy)
    {
        Assert.Equal(copy, lineString);
        Normalizer.Shift(lineString);
        Assert.NotEqual(copy, lineString);
        Normalizer.Shift(lineString, isBack: true);
        Assert.Equal(copy, lineString);
    }

    private static class Data
    {
        private static readonly Coordinate[] Coordinates =
            { new(101, -20), new(98, -29), new(109, -13), new(100, -9), new(101, -20) };

        public static Coordinate[] SampleCoordinates => Coordinates.Select(coordinate => coordinate.Copy()).ToArray();

        public static IEnumerable<IEnumerable<LineString>> DataExtendsLineString = new[]
        {
            new[] { Gf.CreateLineString(SampleCoordinates), Gf.CreateLineString(SampleCoordinates) },
            new LineString[] { Gf.CreateLinearRing(SampleCoordinates), Gf.CreateLinearRing(SampleCoordinates) }
        };
    }
}