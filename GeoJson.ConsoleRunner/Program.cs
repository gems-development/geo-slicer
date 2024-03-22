using BenchmarkDotNet.Validators;
using GeoSlicer.GeoJsonFileService;
using GeoSlicer.HoleDeleters;
using GeoSlicer.Tests.HoleDeletersTests;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Operation.Valid;

string user = "user";
string fileName = "C:\\Users\\" + user + "\\Downloads\\Telegram Desktop\\kazan_fix_2.geojson";
//string fileName = "C:\\Users\\Данил\\Downloads\\Telegram Desktop\\baikal.geojson";
var featureCollection = GeoJsonFileService.ReadGeometryFromFile<FeatureCollection>(fileName);

var polygon = (Polygon)(((MultiPolygon)(featureCollection[0].Geometry))[0]);
var newPolygon = BoundingHoleDeleter.DeleteHoles(polygon, new TraverseDirection(new LineService(1e-9)));

GeoJsonFileService.WriteGeometryToFile(newPolygon, "C:\\Users\\" + user +"\\Downloads\\Telegram Desktop\\newKazan.geojson");


var newSample = BoundingHoleDeleter.DeleteHoles
    (GeoJsonFileService.ReadGeometryFromFile<Polygon>("C:\\Users\\" + user + "\\Downloads\\Telegram Desktop\\sample.geojson"), new TraverseDirection(new LineService(1e-9)));
GeoJsonFileService.WriteGeometryToFile(newSample, "C:\\Users\\"+ user +"\\Downloads\\Telegram Desktop\\newSample.geojson");


/*var zeroDivider = new ZeroTunnelDivider(10, 0.01, new LineIntersector(new EpsilonCoordinateComparator(1e-9), new LineService(1e-9), 1e-9), 1e-9);
var res = zeroDivider.DivideZeroTunnels(newSample.Shell);
GeoJsonFileService.WriteGeometryToFile(res.Item1, "C:\\Users\\Данил\\Downloads\\Telegram Desktop\\newSample2.geojson");*/
IList<(int countOfSteps, double stepSize)> stepCharacteristic = new List<(int countOfSteps, double stepSize)>();
//stepCharacteristic.Add((3, 0.0001));
//stepCharacteristic.Add((3, 0.000_01));
stepCharacteristic.Add((3, 0.000_001));
stepCharacteristic.Add((3, 0.000_000_1));
stepCharacteristic.Add((3, 0.000_000_01));
stepCharacteristic.Add((3, 0.000_000_005));
stepCharacteristic.Add((3, 0.000_000_000_3));
//stepCharacteristic.Add((3, 0.000_000_000_1));
var zeroDivider = new ZeroTunnelDivider(stepCharacteristic, new LineIntersector(new EpsilonCoordinateComparator(1e-14), new LineService(1e-14), 1e-14), 1e-14);
var res = zeroDivider.DivideZeroTunnels(newPolygon.Shell);
GeoJsonFileService.WriteGeometryToFile(res.Item1, "C:\\Users\\" + user + "\\Downloads\\Telegram Desktop\\newKazan2.geojson");
foreach (var coordInf in res.Item2)
{
    var coord = coordInf.Item1;
    Console.WriteLine(coord.Y + ", " + coord.X + " " + coordInf.Item2 + " " + coordInf.Item3);
}

Console.WriteLine(res.Item1.IsValid);



/*string fileName = "C:\\Users\\Данил\\Downloads\\newKazan2.geojson";
var polygon =
    new Polygon(
        new LinearRing(((LineString)(GeoJsonFileService.ReadGeometryFromFile<FeatureCollection>(fileName)[0].Geometry))
            .Coordinates), new LinearRing[0]);
//var polygon = new Polygon(new LinearRing( (GeoJsonFileService.ReadGeometryFromFile<LineString>(fileName)).Coordinates), new LinearRing[0]);
//var polygon = GeoJsonFileService.ReadGeometryFromFile<Polygon>(fileName);
//GeometryGraph graph = new GeometryGraph(0, polygon);
IsValidOp validator = new IsValidOp(polygon);
Console.WriteLine(validator.IsValid);
TopologyValidationError error = validator.ValidationError;
Console.WriteLine(error.ErrorType);
Console.WriteLine(error.Coordinate);
Console.WriteLine(error.Message);
Console.WriteLine(polygon.IsValid);*/












