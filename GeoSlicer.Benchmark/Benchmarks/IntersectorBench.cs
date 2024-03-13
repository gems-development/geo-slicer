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

    private readonly LineLineIntersector _lineLineIntersector =
        new LineLineIntersector(new EpsilonCoordinateComparator(Epsilon), new LineService(Epsilon), Epsilon);
    
    private readonly LineAreaIntersector _lineAreaIntersector =
        new LineAreaIntersector(new EpsilonCoordinateComparator(Epsilon), new LineService(Epsilon), Epsilon);
    
    private readonly AreaAreaIntersector _areaAreaIntersector = new AreaAreaIntersector();

    private const LineAreaIntersectionType SuitableLineAreaIntersectionType =
        LineAreaIntersectionType.Inside| LineAreaIntersectionType.PartlyInside | LineAreaIntersectionType.Overlay;

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
    public void TestOur_LineLine()
    {
        for (int i = 0; i < _coordinates.Length - 1; i++)
        {
            for (int j = 0; j < _coordinates.Length - 1; j++)
            {
                _lineLineIntersector.CheckIntersection(LineLineIntersectionType.Inner, _coordinates[i], _coordinates[i + 1],
                    _coordinates[j], _coordinates[j + 1]);
                _lineLineIntersector.CheckIntersection(LineLineIntersectionType.Inner, _coordinates[i], _coordinates[j + 1],
                    _coordinates[j], _coordinates[i + 1]);
                _lineLineIntersector.CheckIntersection(LineLineIntersectionType.Inner, _coordinates[i], _coordinates[i + 1],
                    _coordinates[i], _coordinates[j + 1]);
                _lineLineIntersector.CheckIntersection(LineLineIntersectionType.Inner, _coordinates[i], _coordinates[i + 1],
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
                _lineAreaIntersector.CheckIntersection(SuitableLineAreaIntersectionType, _coordinates[i], _coordinates[i + 1],
                    _coordinates[j], _coordinates[j + 1]);
                _lineAreaIntersector.CheckIntersection(SuitableLineAreaIntersectionType, _coordinates[i], _coordinates[j + 1],
                    _coordinates[j], _coordinates[i + 1]);
                _lineAreaIntersector.CheckIntersection(SuitableLineAreaIntersectionType, _coordinates[i], _coordinates[i + 1],
                    _coordinates[i], _coordinates[j + 1]);
                _lineAreaIntersector.CheckIntersection(SuitableLineAreaIntersectionType, _coordinates[i], _coordinates[i + 1],
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
                _areaAreaIntersector.CheckIntersection(AreaAreaIntersectionType.Inside, _coordinates[i], _coordinates[i + 1],
                    _coordinates[j], _coordinates[j + 1]);
                _areaAreaIntersector.CheckIntersection(AreaAreaIntersectionType.Inside, _coordinates[i], _coordinates[j + 1],
                    _coordinates[j], _coordinates[i + 1]);
                _areaAreaIntersector.CheckIntersection(AreaAreaIntersectionType.Inside, _coordinates[i], _coordinates[i + 1],
                    _coordinates[i], _coordinates[j + 1]);
                _areaAreaIntersector.CheckIntersection(AreaAreaIntersectionType.Inside, _coordinates[i], _coordinates[i + 1],
                    _coordinates[i + 1], _coordinates[i]);
            }
        }
    }
}