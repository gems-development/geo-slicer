using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using GeoSlicer.DivideAndRuleSlicers;
using GeoSlicer.DivideAndRuleSlicers.OppositesIndexesGivers;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.PolygonClippingAlgorithm;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Benchmark.Benchmarks;

[MemoryDiagnoser(false)]
public class OppositeSlicerKazanBench
{
    private static readonly WeilerAthertonAlgorithm WeilerAtherton = new WeilerAthertonAlgorithm(
        new LinesIntersector(new EpsilonCoordinateComparator(1E-8),
            new LineService(1E-10, new EpsilonCoordinateComparator(1E-10)), 1E-15),
        new LineService(1E-15, new EpsilonCoordinateComparator(1E-8)),
        new EpsilonCoordinateComparator(1E-8),
        new ContainsChecker(new LineService(1E-15, new EpsilonCoordinateComparator(1E-8)), 1E-15));

    private static readonly Slicer Slicer = new Slicer(5,
        WeilerAtherton, new ConvexityIndexesGiver(new LineService(1E-5, new EpsilonCoordinateComparator(1E-8))));

    private static readonly GeoJsonFileService GeoJsonFileService = new GeoJsonFileService();

    private readonly Polygon _polygon =
        (Polygon)((MultiPolygon)GeoJsonFileService.ReadGeometryFromFile<FeatureCollection>("TestFiles\\kazan.geojson")
            [0]
            .Geometry)[0];


    [Benchmark]
    public void Test()
    {
        Slicer.Slice(_polygon, out ICollection<int> _);
    }
}