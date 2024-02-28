using GeoSlicer.GeoJsonFileService;
using GeoSlicer.HoleDeleters;
using GeoSlicer.Tests.HoleDeletersTests;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

string fileName = "C:\\Users\\User\\Downloads\\Telegram Desktop\\kazan_fix_2.geojson";
var featureCollection = GeoJsonFileService.ReadGeometryFromFile<FeatureCollection>(fileName);

var polygon = (Polygon)(((MultiPolygon)(featureCollection[0].Geometry))[0]);
var newPolygon = BoundingHoleDeleter.DeleteHoles(polygon, new TraverseDirection(new SegmentService()));

GeoJsonFileService.WriteGeometryToFile(newPolygon, "C:\\Users\\User\\Downloads\\Telegram Desktop\\newKazan.geojson");

var newSample = BoundingHoleDeleter.DeleteHoles(GeoJsonFileService.ReadGeometryFromFile<Polygon>("C:\\Users\\User\\Downloads\\Telegram Desktop\\sample.geojson"), new TraverseDirection(new SegmentService()));
GeoJsonFileService.WriteGeometryToFile(newSample, "C:\\Users\\User\\Downloads\\Telegram Desktop\\newSample.geojson");


var zeroDivider = new ZeroTunnelDivider(10, 0.2, new LineIntersector(new EpsilonCoordinateComparator(1e-9)));
GeoJsonFileService.WriteGeometryToFile(zeroDivider.DivideZeroTunnels(newSample.Shell), "C:\\Users\\User\\Downloads\\Telegram Desktop\\newSample2.geojson");
