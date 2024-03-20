using BenchmarkDotNet.Attributes;
using NetTopologySuite.Geometries;
using GeoSlicer.Slicers;
using GeoSlicer.Utils;

namespace GeoSlicer.Benchmark.Benchmarks;

[MemoryDiagnoser(false)]
public class SlicersBench
{
    private static readonly LinearRing Ring = Generators.GenerateConvexLinearRing(40);

    [Benchmark]
    public void TestRadialConvexSlicer()
    {
        ISlicer slicer = new RadialSlicer();
        slicer.Slice(Ring, 3);
        slicer.Slice(Ring, 5);
        slicer.Slice(Ring, 10);
    }

    [Benchmark]
    public void TestSpiralConvexSlicer()
    {
        ISlicer slicer = new SpiralSlicer();
        slicer.Slice(Ring, 3);
        slicer.Slice(Ring, 5);
        slicer.Slice(Ring, 10);
    }

    [Benchmark]
    public void TestStepConvexSlicer()
    {
        ISlicer slicer = new StepSlicer();
        slicer.Slice(Ring, 3);
        slicer.Slice(Ring, 5);
        slicer.Slice(Ring, 10);
    }
}