using GeoSlicer.NonConvexSlicer;
using GeoSlicer.NonConvexSlicer.Helpers;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;


const double epsilon = 1E-19;
GeometryFactory gf =
    NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
LineService lineService = new LineService(epsilon);
SegmentService segmentService = new SegmentService(lineService);
TraverseDirection traverseDirection = new TraverseDirection(lineService);
Slicer slicer =
    new(gf,
        segmentService,
        new NonConvexSlicerHelper(
            new LinesIntersector(new EpsilonCoordinateComparator(epsilon), lineService, epsilon),
            lineService), traverseDirection, lineService);

//var polygon = (Polygon)((MultiPolygon)GeoJsonFileService.ReadGeometryFromFile<FeatureCollection>("TestFiles\\kazan.geojson")[0].Geometry)[0];

var polygon = (Polygon)((MultiPolygon)GeoJsonFileService.ReadGeometryFromFile<FeatureCollection>("TestFiles\\baikal.geojson")[0].Geometry)[0];

LinearRing shell = polygon.Shell;


/*//Для нахождения особых точек
if (!traverseDirection.IsClockwiseBypass(shell)) TraverseDirection.ChangeDirection(shell);

var listSpecialPoints = new NonConvexSlicerHelper(
    new LineIntersector(new EpsilonCoordinateComparator(epsilon), lineService, epsilon), traverseDirection,
    lineService).GetSpecialPoints(shell);
GeoJsonFileService.WriteGeometryToFile(new LineString(listSpecialPoints.ToArray()), "TestFiles\\baikal_without_holes_part_123.geojson");
*/

List<LinearRing> result = slicer.Slice(shell);
IEnumerable<Polygon> polygons = result.Select(ring => new Polygon(ring));

MultiPolygon multiPolygonResult = new MultiPolygon(polygons.ToArray());
GeoJsonFileService.WriteGeometryToFile(multiPolygonResult, "TestFiles\\baikal_result_after_changes.geojson");


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

