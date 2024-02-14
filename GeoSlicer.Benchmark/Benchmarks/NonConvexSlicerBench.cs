using BenchmarkDotNet.Attributes;
using GeoSlicer.Utils;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Benchmark.Benchmarks;

[MemoryDiagnoser(false)]
public class NonConvexSlicerBench
{
    private static readonly GeometryFactory Gf =
        NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);

    NonConvexSlicer.NonConvexSlicer _slicer = new NonConvexSlicer.NonConvexSlicer(1E-20, segmentService:new SegmentService(1E-9));

    private readonly LinearRing _ring = Gf.CreateLinearRing(
        GeoJsonFileService.GeoJsonFileService.ReadGeometryFromFile<LineString>(
            "TestFiles\\maloeOzeroLinearRing.geojson").Coordinates);

    [Benchmark]
    public void Check()
    {
        _slicer.Slice(_ring);
    }
    
}