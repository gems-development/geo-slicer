using NetTopologySuite.Geometries;

namespace GeoSlicer.ConvexSlicer;

public interface ISlicer
{
    LinearRing[] Slice(LinearRing ring, int maxDotCount);
}