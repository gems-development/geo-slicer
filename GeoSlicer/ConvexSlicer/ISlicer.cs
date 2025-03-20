using NetTopologySuite.Geometries;

namespace GeoSlicer.ConvexSlicer;

public interface ISlicer
{
    /// <summary>
    /// Разрезает выпуклую геометрию без дыр на куски, количество точек в
    /// которых не превышает <paramref name="maxDotCount"/>.
    /// </summary>
    LinearRing[] Slice(LinearRing ring, int maxDotCount);
}