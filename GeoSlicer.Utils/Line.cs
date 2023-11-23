using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils;

public struct Line
{
    public Coordinate A;
    public Coordinate B;

    public Line(Coordinate a, Coordinate b)
    {
        A = a;
        B = b;
    }
}