using GeoSlicer.GeoJsonFileService;
using GeoSlicer.NonConvexSlicer;
using GeoSlicer.NonConvexSlicer.Helpers;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;


const double epsilon = 1E-15;
GeometryFactory gf =
    NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
LineService lineService = new LineService(epsilon);
SegmentService segmentService = new SegmentService(lineService);
TraverseDirection traverseDirection = new TraverseDirection(lineService);
NonConvexSlicer nonConvexSlicer =
    new(gf,
        segmentService,
        new NonConvexSlicerHelper(
            new LineIntersector(new EpsilonCoordinateComparator(epsilon), lineService, epsilon),
            lineService), traverseDirection, lineService);

MultiPolygon multiPolygon =
    GeoJsonFileService.ReadGeometryFromFile<MultiPolygon>("TestFiles\\baikal_multy_polygon.geojson");

LinearRing shell = ((Polygon)multiPolygon[0]).Shell;

/*//Для нахождения особых точек
if (!traverseDirection.IsClockwiseBypass(shell)) TraverseDirection.ChangeDirection(shell);

var listSpecialPoints = new NonConvexSlicerHelper(
    new LineIntersector(new EpsilonCoordinateComparator(epsilon), lineService, epsilon), traverseDirection,
    lineService).GetSpecialPoints(shell);
GeoJsonFileService.WriteGeometryToFile(new LineString(listSpecialPoints.ToArray()), "TestFiles\\baikal_without_holes_part_123.geojson");
*/

List<LinearRing> result = nonConvexSlicer.Slice(shell);
IEnumerable<Polygon> polygons = result.Select(ring => new Polygon(ring));

MultiPolygon multiPolygonResult = new MultiPolygon(polygons.ToArray());
GeoJsonFileService.WriteGeometryToFile(multiPolygonResult, "TestFiles\\baikal_result.geojson");


// MultiPolygon baikalMultiPolygon =
//     GeoJsonFileService.ReadGeometryFromFile<MultiPolygon>("TestFiles\\baikal_multy_polygon.geojson");
// MultiPolygon resultMultiPolygon =
//     GeoJsonFileService.ReadGeometryFromFile<MultiPolygon>("TestFiles\\baikal_result.geojson");
//
// HashSet<Coordinate> baikalSet = new HashSet<Coordinate>(((Polygon)baikalMultiPolygon[0]).Shell.Coordinates);
//
// var resultCoordinateArrays = resultMultiPolygon.Select(geometry => ((Polygon)geometry).Coordinates);
//
// HashSet<Coordinate> resultSet = new HashSet<Coordinate>();
// foreach (Coordinate[] coordinateArray in resultCoordinateArrays)
// {
//     resultSet.UnionWith(coordinateArray);
// }
// Console.WriteLine(baikalSet.Count);
// Console.WriteLine(resultSet.Count);
//
// baikalSet.ExceptWith(resultSet);
// foreach (Coordinate coordinate in baikalSet)
// {
//     Console.WriteLine(coordinate);
// }
// Console.WriteLine(baikalSet);

