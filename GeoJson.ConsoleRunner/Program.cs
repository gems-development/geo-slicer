using GeoSlicer.GeoJsonFileService;
using GeoSlicer.HoleDeleters;
using GeoSlicer.Tests.HoleDeletersTests;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

string user = "Данил";
string fileName = "C:\\Users\\" + user + "\\Downloads\\Telegram Desktop\\";
var featureCollection = GeoJsonFileService
    .ReadGeometryFromFile<FeatureCollection>
        ("TestFiles\\test_final_geojson.geojson");
var polygon = (Polygon)((MultiPolygon)featureCollection[0].Geometry)[0];


TraverseDirection Traverse = new (new LineService(1e-15));
BoundingHoleDeleter Deleter = new (Traverse);

IList<(int countOfSteps, double stepSize)> stepCharacteristic = new List<(int countOfSteps, double stepSize)>();
int countOfSteps = 100;
stepCharacteristic.Add((countOfSteps, 0.000_001));
stepCharacteristic.Add((countOfSteps, 0.000_000_1));
stepCharacteristic.Add((countOfSteps, 0.000_000_01));
stepCharacteristic.Add((countOfSteps, 0.000_000_005));
stepCharacteristic.Add((countOfSteps, 0.000_000_000_3));
stepCharacteristic.Add((countOfSteps, 0.000_000_000_000_1));
stepCharacteristic.Add((countOfSteps, 0.000_000_000_000_000_001));
        
double epsilon = 1e-15;
var zeroDivider = new ZeroTunnelDivider(
    stepCharacteristic, 
    new LineIntersector(
        new EpsilonCoordinateComparator(epsilon),
        new LineService(epsilon), epsilon),
    epsilon);
Console.WriteLine(polygon.IsValid);
var res = Deleter.DeleteHoles(polygon);
zeroDivider.DivideZeroTunnels(res.Shell, out var resultRing, out var problemCoordinates);
foreach (var point in problemCoordinates)
{
    Console.WriteLine(point);
}
GeoJsonFileService.WriteGeometryToFile(res, fileName + "daniilTest");
GeoJsonFileService.WriteGeometryToFile(resultRing, fileName + "daniilTestAfter");











