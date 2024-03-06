using NetTopologySuite.Geometries;

namespace GeoSlicer.NonConvexSlicer;

public class CoordinatePcn : Coordinate
{
    public int P { get; set; }
    public int C { get; set; }
    public int N { get; set; }
    public int Pl { get; set; }
    public int Nl { get; set; }

    public CoordinatePcn(double x, double y, int p = -1, int c = -1, int n = -1) : base(x, y)
    {
        P = p;
        C = c;
        N = n;
        Pl = p;
        Nl = n;
    }
}