using BenchmarkDotNet.Attributes;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Benchmark.Benchmarks;

[MemoryDiagnoser(false)]
public class IntersectorBench
{
    private const double Epsilon = 1E-5;

    private readonly RobustLineIntersector _robustLineIntersector = new RobustLineIntersector();

    private readonly LinesIntersector _linesIntersector =
        new LinesIntersector(new EpsilonCoordinateComparator(Epsilon), new LineService(Epsilon, new EpsilonCoordinateComparator(Epsilon)), Epsilon);

    private readonly LineAreaIntersector _lineAreaIntersector =
        new LineAreaIntersector(new LineService(Epsilon, new EpsilonCoordinateComparator(Epsilon)), Epsilon);

    private readonly AreasIntersector _areasIntersector = new AreasIntersector();

    private const LineAreaIntersectionType SuitableLineAreaIntersectionType =
        LineAreaIntersectionType.Inside | LineAreaIntersectionType.PartlyInside | LineAreaIntersectionType.Overlay;

    private static readonly GeoJsonFileService GeoJsonFileService = new ();
    
    private readonly Coordinate[] _coordinates = GeoJsonFileService
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
    public void TestOur_LineLine()
    {
        for (int i = 0; i < _coordinates.Length - 1; i++)
        {
            for (int j = 0; j < _coordinates.Length - 1; j++)
            {
                _linesIntersector.CheckIntersection(LinesIntersectionType.Inner, _coordinates[i], _coordinates[i + 1],
                    _coordinates[j], _coordinates[j + 1]);
                _linesIntersector.CheckIntersection(LinesIntersectionType.Inner, _coordinates[i], _coordinates[j + 1],
                    _coordinates[j], _coordinates[i + 1]);
                _linesIntersector.CheckIntersection(LinesIntersectionType.Inner, _coordinates[i], _coordinates[i + 1],
                    _coordinates[i], _coordinates[j + 1]);
                _linesIntersector.CheckIntersection(LinesIntersectionType.Inner, _coordinates[i], _coordinates[i + 1],
                    _coordinates[i + 1], _coordinates[i]);
            }
        }
    }

    [Benchmark]
    public void TestOur_LineArea()
    {
        for (int i = 0; i < _coordinates.Length - 1; i++)
        {
            for (int j = 0; j < _coordinates.Length - 1; j++)
            {
                _lineAreaIntersector.CheckIntersection(SuitableLineAreaIntersectionType, _coordinates[i],
                    _coordinates[i + 1],
                    _coordinates[j], _coordinates[j + 1]);
                _lineAreaIntersector.CheckIntersection(SuitableLineAreaIntersectionType, _coordinates[i],
                    _coordinates[j + 1],
                    _coordinates[j], _coordinates[i + 1]);
                _lineAreaIntersector.CheckIntersection(SuitableLineAreaIntersectionType, _coordinates[i],
                    _coordinates[i + 1],
                    _coordinates[i], _coordinates[j + 1]);
                _lineAreaIntersector.CheckIntersection(SuitableLineAreaIntersectionType, _coordinates[i],
                    _coordinates[i + 1],
                    _coordinates[i + 1], _coordinates[i]);
            }
        }
    }

    [Benchmark]
    public void TestOur_AreaArea()
    {
        for (int i = 0; i < _coordinates.Length - 1; i++)
        {
            for (int j = 0; j < _coordinates.Length - 1; j++)
            {
                _areasIntersector.IsIntersects(_coordinates[i], _coordinates[i + 1],
                    _coordinates[j], _coordinates[j + 1]);
                _areasIntersector.IsIntersects(_coordinates[i], _coordinates[j + 1],
                    _coordinates[j], _coordinates[i + 1]);
                _areasIntersector.IsIntersects(_coordinates[i], _coordinates[i + 1],
                    _coordinates[i], _coordinates[j + 1]);
                _areasIntersector.IsIntersects(_coordinates[i], _coordinates[i + 1],
                    _coordinates[i + 1], _coordinates[i]);
            }
        }
    }
}