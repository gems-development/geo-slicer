using System.Linq;
using GeoSlicer.Exceptions;
using NetTopologySuite.Geometries;
using NetTopologySuite.Simplify;

namespace GeoSlicer.Utils;

public static class DuplicatePointsDeleter
{
    public static Polygon HandlePolygon(Polygon polygon)
    {
        return (Polygon)HandleGeometry(polygon);
    }
    
    public static LinearRing HandleLinearRing(LinearRing linearRing)
    {
        return (LinearRing)HandleGeometry(linearRing);
    }
    
    public static LineString HandleLineString(LineString lineString)
    {
        return (LineString)HandleGeometry(lineString);
    }
    
    public static MultiPoint HandleMultiPoint(MultiPoint multiPoint)
    {
        CoordinateEqualityComparer comparer = new CoordinateEqualityComparer();
        var points = multiPoint
            .Geometries.Select(a => (Point)a).DistinctBy(a => a.Coordinate, comparer).ToArray();
        
        return new MultiPoint(points, multiPoint.Factory){SRID = multiPoint.SRID};  
    }

    private static Geometry HandleGeometry(Geometry geometry)
    {
        var algorithm = new DouglasPeuckerSimplifier(geometry) { DistanceTolerance = 0.0, EnsureValidTopology = false };
        Geometry res = algorithm.GetResultGeometry();
        if (!res.IsValid)
            throw new DeleteDuplicatePointsException("result geometry is not valid");
        return res;
    }
}