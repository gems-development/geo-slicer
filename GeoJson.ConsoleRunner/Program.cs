using GeoSlicer.GeoJsonFileService;
using GeoSlicer.NonConvexSlicer;
using GeoSlicer.NonConvexSlicer.Helpers;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

var fileName = "TestFiles\\kazan_fix_2.geojson";
var featureCollection = GeoJsonFileService.ReadGeometryFromFile<FeatureCollection>(fileName);

var polygon = (Polygon)(((MultiPolygon)(featureCollection[0].Geometry))[0]);

// var polygon = (Polygon)GeoJsonFileService.ReadGeometryFromFile<MultiPolygon>(fileName)[0];
var epsilon = 1E-7;
var gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
var lineService = new LineService(epsilon);
var segmentService = new SegmentService(lineService);
var traverseDirection = new TraverseDirection(lineService);
var nonConvexSlicerHelper = new NonConvexSlicerHelper(
    new LineIntersector(new EpsilonCoordinateComparator(epsilon), lineService, epsilon),
    traverseDirection, lineService);

var slicer = new NonConvexSlicer(gf, segmentService, nonConvexSlicerHelper, traverseDirection, lineService);

var list = slicer.Slice(polygon.Shell);

var listPolygons = new List<Polygon>();


foreach (var iter in list)
{
    polygon = new Polygon(iter);
    listPolygons.Add(polygon);
}

var multiPolygon = new MultiPolygon(listPolygons.ToArray());
GeoJsonFileService.WriteGeometryToFile(multiPolygon, "TestFiles\\kazan_porezannaya.geojson");