using System;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.HoleDeletersTests;

public static class ObjectsForTests
{ 
    
    private static readonly GeoJsonFileService GeoJsonFileService = new ();
    
    public static ZeroTunnelDivider GetZeroTunnelDivider()
    {
        IList<(int countOfSteps, double stepSize)> stepCharacteristic = new List<(int countOfSteps, double stepSize)>();
        int countOfSteps = 2;
        //stepCharacteristic.Add((countOfSteps, 0.001));
        //stepCharacteristic.Add((countOfSteps, 0.000_001));
        //stepCharacteristic.Add((countOfSteps, 0.000_000_1));
        stepCharacteristic.Add((countOfSteps, 0.000_000_01));
        stepCharacteristic.Add((countOfSteps, 0.000_000_005));
        stepCharacteristic.Add((countOfSteps, 0.000_000_000_3));
        
        double epsilon = 1e-15;
        var zeroDivider = new ZeroTunnelDivider(
            stepCharacteristic, 
            new LinesIntersector(
                new EpsilonCoordinateComparator(epsilon),
                new LineService(epsilon, new EpsilonCoordinateComparator(epsilon)), epsilon),
            epsilon);
        
        return zeroDivider;
    }
    public static Polygon GetSample()
    {
        return GeoJsonFileService
            .ReadGeometryFromFile<Polygon>
                ("TestFiles\\sample.geojson");
    }

    public static Polygon GetKazan()
    { 
        var featureCollection = GeoJsonFileService
            .ReadGeometryFromFile<FeatureCollection>
                ("TestFiles\\kazan.geojson");
        return (Polygon)((MultiPolygon)featureCollection[0].Geometry)[0];
    }

    public static Polygon GetBaikal()
    {
        var featureCollection = GeoJsonFileService
            .ReadGeometryFromFile<FeatureCollection>
                ("TestFiles\\baikal.geojson");
        return (Polygon)((MultiPolygon)featureCollection[0].Geometry)[0];
    }

    public static Polygon GetTestFinal()
    {
        var featureCollection = GeoJsonFileService
            .ReadGeometryFromFile<FeatureCollection>
                ("TestFiles\\test_final_geojson.geojson");
        return (Polygon)((MultiPolygon)featureCollection[0].Geometry)[0];
    }
    public static Polygon GetTest2()
    {
        var featureCollection = GeoJsonFileService
            .ReadGeometryFromFile<FeatureCollection>
                ("TestFiles\\test2_geojson.geojson");
        return (Polygon)((MultiPolygon)featureCollection[0].Geometry)[0];
    }

    public static Polygon? GetTest3(double step)
    {
        Coordinate firstRingCoord = new Coordinate(0.3, 0.4 + step);
        LinearRing ring1 = new(
            new[]
            {
                new Coordinate(0, 0.3),
                firstRingCoord,
                new Coordinate(0.6, 0.3),
                new Coordinate(0, 0.3)
            });

        Coordinate secondRingFirstCoord = new Coordinate(0.4, 0.4 - step);
        Coordinate secondRingSecondCoord = new Coordinate(0.6 + step, 0.5);
        LinearRing ring2 = new(
            new[]
            {
                new Coordinate(0, 0.5),
                new Coordinate(0.5, 1),
                secondRingSecondCoord,
                secondRingFirstCoord,
                new Coordinate(0, 0.5)
            });

        Coordinate thirdRingFirstCoord = new Coordinate(0.6 - step, 0.6);
        Coordinate thirdRingSecondCoord = new Coordinate(0.7, 0.3 - step);
        LinearRing ring3 = new(
            new[]
            {
                thirdRingFirstCoord,
                new Coordinate(0.7, 1),
                new Coordinate(1, 0.5),
                thirdRingSecondCoord,
                thirdRingFirstCoord
            });
        
        Coordinate fourthRingCoord = new Coordinate(0.8, 0.3 + step);
        LinearRing ring4 = new(
            new[]
            {
                new Coordinate(0, 0),
                fourthRingCoord,
                new Coordinate(1, 0.2),
                new Coordinate(1, 0),
                new Coordinate(0, 0)
            });
        LinearRing shell = new(
            new[]
            {
                new Coordinate(-2, -2),
                new Coordinate(-2, 2),
                new Coordinate(2, 2),
                new Coordinate(2, -2),
                new Coordinate(-2, -2)
            });
        
        Random random = new Random(1);
        LinearRing[] rings = { ring1, ring2, ring3, ring4 };
        rings = rings.OrderBy(a => random.NextDouble()).ToArray();
        Polygon testPolygon = new Polygon(shell,  rings);
        
        if (testPolygon.IsValid)
            return testPolygon;
        
        return null;
    }
    
    public static Polygon? GetTest4(double step, int permutationNumber)
    {
        Coordinate firstRingCoord = new Coordinate(5, 3 + step);
        LinearRing ring1 = new(
            new[]
            {
                new Coordinate(0, 0),
                firstRingCoord,
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            });

        Coordinate secondRingCoord = new Coordinate(1, 3 - step);
        LinearRing ring2 = new(
            new[]
            {
                secondRingCoord,
                new Coordinate(1.5, 4),
                new Coordinate(2, 3),
                secondRingCoord
            });

        Coordinate thirdRingCoord = new Coordinate(3, 3 - step);
        LinearRing ring3 = new(
            new[]
            {
                thirdRingCoord,
                new Coordinate(3, 4),
                new Coordinate(4, 4),
                new Coordinate(4, 3),
                thirdRingCoord
            });
        
        LinearRing shell = new(
            new[]
            {
                new Coordinate(-11, -11),
                new Coordinate(-11, 11),
                new Coordinate(11, 11),
                new Coordinate(11, -11),
                new Coordinate(-11, -11)
            });

        LinearRing[] rings = GetPermutationArray(ring1, ring2, ring3, permutationNumber);
        Polygon testPolygon = new Polygon(shell,  rings);
        
        if (testPolygon.IsValid)
            return testPolygon;
        
        return null;
    }

    private static LinearRing[] GetPermutationArray(LinearRing a, LinearRing b, LinearRing c, int permutationNumber)
    {
        if (permutationNumber == 1)
        {
            return new []{a, b, c};
        }
        if (permutationNumber == 2)
        {
            return new []{a, c, b};
        }
        if (permutationNumber == 3)
        {
            return new []{b, c, a};
        }
        if (permutationNumber == 4)
        {
           return new []{b, a, c};
        }
        if (permutationNumber == 5)
        {
            return new []{c, a, b};
        }
        if (permutationNumber == 6)
        {
            return new []{c, b, a};
        }

        throw new ArgumentException("permutationNumber belongs to the range from 1 to 6");
    }
}