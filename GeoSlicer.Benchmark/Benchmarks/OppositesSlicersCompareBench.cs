using BenchmarkDotNet.Attributes;
using GeoSlicer.DivideAndRuleSlicers.OppositesSlicer;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.PolygonClippingAlghorithm;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Benchmark.Benchmarks;
[MemoryDiagnoser(false)]


public class OppositesSlicersCompareBench
{
    private const double Epsilon = 1E-19;


    private static readonly LineService LineService = new LineService(Epsilon);
    private static readonly EpsilonCoordinateComparator CoordinateComparator = new EpsilonCoordinateComparator(Epsilon);

    private static readonly Slicer Slicer = new Slicer(LineService, 1000,
        new WeilerAthertonAlghorithm(new LinesIntersector(CoordinateComparator, LineService, Epsilon), LineService,
            CoordinateComparator, new ContainsChecker(LineService, Epsilon)));

    private static readonly SlicerOld SlicerOld = new SlicerOld(LineService, 1000,
        new WeilerAthertonAlghorithm(new LinesIntersector(CoordinateComparator, LineService, Epsilon), LineService,
            CoordinateComparator, new ContainsChecker(LineService, Epsilon)));


    private static readonly Polygon Polygon =
        (Polygon)((MultiPolygon)GeoJsonFileService.ReadGeometryFromFile<FeatureCollection>("TestFiles\\baikal.geojson")[0]
            .Geometry)[0];

    [Benchmark]
    public void CheckNew()
    {
        Slicer.Slice(Polygon);
    }
    
    [Benchmark]
    public void CheckOld()
    {
        SlicerOld.Slice(Polygon);
    }
}