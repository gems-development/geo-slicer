using BenchmarkDotNet.Attributes;
using GeoSlicer.BoxSegmentSlicer;
using GeoSlicer.Utils;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Benchmark.Benchmarks;

public class BoxSegmentSlicerKazanBench
{
  

    private static readonly Slicer Slicer = new (128);
    private static readonly GeoJsonFileService GeoJsonFileService = new ();

    private readonly Polygon _polygon =
        (Polygon)((MultiPolygon)GeoJsonFileService.ReadGeometryFromFile<FeatureCollection>("TestFiles\\kazan.geojson")
            [0]
            .Geometry)[0];


    [Benchmark]
    public void Test()
    {
        Slicer.Slice(_polygon);
    }
}