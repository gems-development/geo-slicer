using System;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Exceptions;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils;

public static class NtsPolygonSlicer
{
    private const double Epsilon = 1e-12;
    public static List<Polygon> SliceByOrdinateValue(in (double value, Ordinate ordinate) ordinateValue, Polygon polygon)
    {
        EnvelopeDelimiter.Delimite(
            polygon.EnvelopeInternal,
            ordinateValue, 
            out var leftEnvelope,
            out var rightEnvelope);
        
        double fragmentsArea = 0;
        var leftBox = (Polygon) polygon.Factory.ToGeometry(leftEnvelope);
        var rightBox = (Polygon) polygon.Factory.ToGeometry(rightEnvelope);

        IEnumerable<Polygon> leftPart = GeometryToPolygons(polygon.Intersection(leftBox));
        IEnumerable<Polygon> rightPart = GeometryToPolygons(polygon.Intersection(rightBox));
            
        var result = leftPart.Concat(rightPart).Select(a => 
        {
            if (!a.IsValid)
                throw new GeometryIntersectionException("fragment of original polygon not valid");
            fragmentsArea += a.Area;
            return a;
        }).ToList();
        
        var diff = Math.Abs(fragmentsArea - polygon.Area);
        
        if (diff > Epsilon)
            throw new GeometryIntersectionException(
                $"the sum of the areas of the fragments and the original polygon differs by {diff}");
        return result;
    }

    private static IEnumerable<Polygon> GeometryToPolygons(Geometry geometry)
    {
        return geometry switch
        {
            Polygon polygon => new[] { polygon },
            GeometryCollection collection => collection.Geometries.OfType<Polygon>(),
            _ => throw new ApplicationException($"Geometry type - {geometry.GeometryType} not supported")
        };
    }

}