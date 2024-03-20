using System.Collections.Generic;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.HoleDeletersTests;

public static class ObjectsForTests
{ 
    public static ZeroTunnelDivider GetZeroTunnelDivider()
    {
        IList<(int countOfSteps, double stepSize)> stepCharacteristic = new List<(int countOfSteps, double stepSize)>();
        int countOfSteps = 3;
        stepCharacteristic.Add((countOfSteps, 0.000_001));
        stepCharacteristic.Add((countOfSteps, 0.000_000_1));
        stepCharacteristic.Add((countOfSteps, 0.000_000_01));
        stepCharacteristic.Add((countOfSteps, 0.000_000_005));
        stepCharacteristic.Add((countOfSteps, 0.000_000_000_3));
        
        double epsilon = 1e-15;
        var zeroDivider = new ZeroTunnelDivider(
            stepCharacteristic, 
            new LineIntersector(
                new EpsilonCoordinateComparator(epsilon),
                new LineService(epsilon), epsilon),
            epsilon);
        
        return zeroDivider;
    }
    public static Polygon GetSample()
    {
        return GeoJsonFileService
            .GeoJsonFileService
            .ReadGeometryFromFile<Polygon>
                ("TestFiles\\sample.geojson");
    }

    public static Polygon GetKazan()
    { 
        var featureCollection = GeoJsonFileService
            .GeoJsonFileService
            .ReadGeometryFromFile<FeatureCollection>
                ("TestFiles\\kazan_fix_2.geojson");
        return (Polygon)((MultiPolygon)featureCollection[0].Geometry)[0];
    }

    public static Polygon GetBaikal()
    {
        var featureCollection = GeoJsonFileService
            .GeoJsonFileService
            .ReadGeometryFromFile<FeatureCollection>
                ("TestFiles\\baikal.geojson");
        return (Polygon)((MultiPolygon)featureCollection[0].Geometry)[0];
    }
}