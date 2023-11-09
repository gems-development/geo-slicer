using BenchmarkDotNet.Attributes;
using GeoSlicer.HoleDeleters;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Benchmark.Benchmarks;

public class HoleDeletersBench
{
    private static readonly Polygon With3AHoles =
        GeoJsonFileService.GeoJsonFileService.ReadGeometryFromFile<Polygon>("GeoSlicer.Benchmark/BenchData/with_3a_holes.geojson");

    [Benchmark]
    public void TestBruteForceHoleDeleter()
    {
        BruteForceHoleDeleter bruteForceHoleDeleter = new BruteForceHoleDeleter();
        bruteForceHoleDeleter.DeleteHoles(With3AHoles);
    }
}