using BenchmarkDotNet.Attributes;
using GeoSlicer.Slicers;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Benchmark.Benchmarks;

public class SlicersBench
{
    private static readonly GeometryFactory Gf =
        NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);

    private static readonly LinearRing Ring = Gf.CreateLinearRing(new[]
    {
        new Coordinate(0, 0), new Coordinate(0, 10), new Coordinate(1, 19), new Coordinate(3, 27),
        new Coordinate(6, 34), new Coordinate(10, 40), new Coordinate(15, 45), new Coordinate(21, 49),
        new Coordinate(28, 52), new Coordinate(36, 54), new Coordinate(45, 55), new Coordinate(55, 55),
        new Coordinate(64, 54), new Coordinate(72, 52), new Coordinate(79, 49), new Coordinate(85, 45),
        new Coordinate(90, 40), new Coordinate(94, 34), new Coordinate(97, 27), new Coordinate(99, 19),
        new Coordinate(100, 10), new Coordinate(100, 0), new Coordinate(99, -9), new Coordinate(97, -17),
        new Coordinate(94, -24), new Coordinate(90, -30), new Coordinate(85, -35), new Coordinate(79, -39),
        new Coordinate(72, -42), new Coordinate(64, -44), new Coordinate(55, -45), new Coordinate(45, -45),
        new Coordinate(36, -44), new Coordinate(28, -42), new Coordinate(21, -39), new Coordinate(15, -35),
        new Coordinate(10, -30), new Coordinate(6, -24), new Coordinate(3, -17), new Coordinate(1, -9),
        new Coordinate(0, 0)
    });

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