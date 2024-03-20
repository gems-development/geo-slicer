using NetTopologySuite.Geometries;

namespace GeoSlicer.Slicers;

public interface ISlicer
{
    LinearRing[] Slice(LinearRing ring, int maxDotCount);
}