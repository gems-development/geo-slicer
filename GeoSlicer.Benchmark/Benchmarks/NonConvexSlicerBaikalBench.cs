using BenchmarkDotNet.Attributes;
using GeoSlicer.NonConvexSlicer.Helpers;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Benchmark.Benchmarks;

[MemoryDiagnoser(false)]
public class NonConvexSlicerBaikalBench
{
    private static readonly double Epsilon = 1E-15;

    private static readonly GeometryFactory Gf =
        NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);

    private static readonly LineService LineService = new LineService(Epsilon, new EpsilonCoordinateComparator(Epsilon));
    private static readonly SegmentService SegmentService = new SegmentService(LineService);

    private readonly NonConvexSlicer.Slicer _slicer =
        new(Gf,
            SegmentService,
            new NonConvexSlicerHelper(
                new LinesIntersector(new EpsilonCoordinateComparator(Epsilon), LineService, Epsilon), LineService));
    
    private static readonly GeoJsonFileService GeoJsonFileService = new ();

    private static readonly MultiPolygon MultiPolygon =
        GeoJsonFileService.ReadGeometryFromFile<MultiPolygon>("TestFiles\\baikal_multy_polygon.geojson");

    private readonly LinearRing _ring = ((Polygon)MultiPolygon[0]).Shell;

    [Benchmark]
    public void Check()
    {
        _slicer.Slice(_ring);
    }
}