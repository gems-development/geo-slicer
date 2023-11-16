using NetTopologySuite.Geometries;

namespace GeoSlicer.NonConvexSlicer;

public class CoordinatePCN : Coordinate
{
    public new double X { get; }
    public new double Y { get; }
    public int P { get; set; }
    public int C { get; set; }
    public int N { get; set; }
    public int PL { get; set; }
    public int NL { get; set; }

    public CoordinatePCN(double x, double y, int p = -1, int c = -1, int n = -1)
    {
        X = x;
        Y = y;
        P = p;
        C = c;
        N = n;
        PL = p;
        NL = n;
    }

    public CoordinateM ToCoordinateM()
    {
        return new CoordinateM(X, Y, C);
    }
    
    public Coordinate ToCoordinate()
    {
        return new Coordinate(X, Y);
    }
}