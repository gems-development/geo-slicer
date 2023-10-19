using BenchmarkDotNet.Attributes;
using NetTopologySuite.Geometries;
using GeoSlicer.Slicers;
using GeoSlicer.Utils;

namespace GeoSlicer.Benchmark.Benchmarks;

[MemoryDiagnoser(false)]
public class SlicersBench
{
    private static readonly GeometryFactory Gf =
        NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);

    private static readonly LinearRing Ring = Generators.GenerateConvexLinearRing(40);

    [Benchmark]
    public void TestRadialConvexSlicer()
    {
        ISlicer slicer = new RadialConvexSlicer();
        slicer.Slice(Ring, 3);
        slicer.Slice(Ring, 5);
        slicer.Slice(Ring, 10);
    }

    [Benchmark]
    public void TestSpiralConvexSlicer()
    {
        ISlicer slicer = new SpiralConvexSlicer();
        slicer.Slice(Ring, 3);
        slicer.Slice(Ring, 5);
        slicer.Slice(Ring, 10);
    }

    [Benchmark]
    public void TestStepConvexSlicer()
    {
        ISlicer slicer = new StepConvexSlicer();
        slicer.Slice(Ring, 3);
        slicer.Slice(Ring, 5);
        slicer.Slice(Ring, 10);
    }
}