using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using GeoSlicer.DivideAndRuleSlicers;
using GeoSlicer.DivideAndRuleSlicers.OppositesIndexesGivers;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.PolygonClippingAlgorithm;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Benchmark.Benchmarks;

[MemoryDiagnoser(false)]
public class OppositeSlicerKazanBench
{
    const double epsilon = 1E-15;

    LineService lineService = new LineService(1E-15, new EpsilonCoordinateComparator(1E-8));


    static WeilerAthertonAlgorithm weilerAtherton = new WeilerAthertonAlgorithm(
        new LinesIntersector(new EpsilonCoordinateComparator(1E-8),
            new LineService(1E-10, new EpsilonCoordinateComparator(1E-10)), 1E-15),
        new LineService(1E-15, new EpsilonCoordinateComparator(1E-8)),
        new EpsilonCoordinateComparator(1E-8),
        new ContainsChecker(new LineService(1E-15, new EpsilonCoordinateComparator(1E-8)), 1E-15));
    static Slicer slicer = new Slicer(5,
        weilerAtherton, new ConvexityIndexesGiver(new LineService(1E-5, new EpsilonCoordinateComparator(1E-8))));

    static GeoJsonFileService geoJsonFileService = new GeoJsonFileService();

    Polygon polygon =
        (Polygon)((MultiPolygon)geoJsonFileService.ReadGeometryFromFile<FeatureCollection>("TestFiles\\kazan.geojson")[0]
            .Geometry)[0];
    

    [Benchmark]
    public void Test()
    {
        slicer.Slice(polygon, out ICollection<int> skippedGeomsIndexes);
    }
}