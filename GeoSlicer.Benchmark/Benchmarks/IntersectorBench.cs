using BenchmarkDotNet.Attributes;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using LineIntersector = GeoSlicer.Utils.Intersectors.LineIntersector;

namespace GeoSlicer.Benchmark.Benchmarks;

[MemoryDiagnoser(false)]
public class IntersectorBench
{
    private const double Epsilon = 1E-5;

    private readonly RobustLineIntersector _robustLineIntersector = new RobustLineIntersector();

    private readonly LineIntersector _lineIntersector =
        new LineIntersector(new EpsilonCoordinateComparator(Epsilon), new LineService(Epsilon), Epsilon);

    private readonly Coordinate[] _coordinates = GeoJsonFileService.GeoJsonFileService
        .ReadGeometryFromFile<LineString>("TestFiles\\maloeOzeroLinearRing.geojson").Coordinates;

    [Benchmark]
    public void TestRobust()
    {
        for (int i = 0; i < _coordinates.Length - 1; i++)
        {
            for (int j = 0; j < _coordinates.Length - 1; j++)
            {
                _robustLineIntersector.ComputeIntersection(_coordinates[i], _coordinates[i + 1], _coordinates[j],
                    _coordinates[j + 1]);
                _robustLineIntersector.IsInteriorIntersection();
                _robustLineIntersector.ComputeIntersection(_coordinates[i], _coordinates[j + 1], _coordinates[j],
                    _coordinates[i + 1]);
                _robustLineIntersector.IsInteriorIntersection();
                _robustLineIntersector.ComputeIntersection(_coordinates[i], _coordinates[i + 1], _coordinates[i],
                    _coordinates[j + 1]);
                _robustLineIntersector.IsInteriorIntersection();
                _robustLineIntersector.ComputeIntersection(_coordinates[i], _coordinates[i + 1], _coordinates[i + 1],
                    _coordinates[i]);
                _robustLineIntersector.IsInteriorIntersection();
            }
        }
    }

    [Benchmark]
    public void TestOur()
    {
        for (int i = 0; i < _coordinates.Length - 1; i++)
        {
            for (int j = 0; j < _coordinates.Length - 1; j++)
            {
                _lineIntersector.CheckIntersection(IntersectionType.Inner, _coordinates[i], _coordinates[i + 1],
                    _coordinates[j], _coordinates[j + 1]);
                _lineIntersector.CheckIntersection(IntersectionType.Inner, _coordinates[i], _coordinates[j + 1],
                    _coordinates[j], _coordinates[i + 1]);
                _lineIntersector.CheckIntersection(IntersectionType.Inner, _coordinates[i], _coordinates[i + 1],
                    _coordinates[i], _coordinates[j + 1]);
                _lineIntersector.CheckIntersection(IntersectionType.Inner, _coordinates[i], _coordinates[i + 1],
                    _coordinates[i + 1], _coordinates[i]);
            }
        }
    }
}