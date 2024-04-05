using GeoSlicer.HoleDeleters;
using GeoSlicer.Tests.HoleDeletersTests;
using GeoSlicer.Utils;
using GeoSlicer.Utils.BoundRing;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Valid;
using GeoJsonFileService = GeoSlicer.Utils.GeoJsonFileService;

string user = "User";
string fileName = "C:\\Users\\" + user + "\\Downloads\\Telegram Desktop\\";
/*var featureCollection = GeoJsonFileService
    .ReadGeometryFromFile<FeatureCollection>
        ("TestFiles\\baikal.geojson");
var polygon = (Polygon)((MultiPolygon)featureCollection[0].Geometry)[0];


TraverseDirection Traverse = new (new LineService(1e-15));
BoundingHoleDeleter Deleter = new (Traverse);

IList<(int countOfSteps, double stepSize)> stepCharacteristic = new List<(int countOfSteps, double stepSize)>();
int countOfSteps = 3;
stepCharacteristic.Add((countOfSteps, 0.000_001));
stepCharacteristic.Add((countOfSteps, 0.000_000_1));
stepCharacteristic.Add((countOfSteps, 0.000_000_01));
stepCharacteristic.Add((countOfSteps, 0.000_000_005));
stepCharacteristic.Add((countOfSteps, 0.000_000_000_3));
        
double epsilon = 1e-15;
var zeroDivider = new ZeroTunnelDivider(
    stepCharacteristic, 
    new LinesIntersector(
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

Coordinate[] resArr = new Coordinate[res.Coordinates.Length];
Coordinate[] resArr2 = new Coordinate[res.Coordinates.Length];
LinkedList<BoundingRing> list1 = GeoSlicer.Utils.BoundRing.BoundingRing.PolygonToBoundRings(res, Traverse);
LinkedList<BoundingRing> list2 = GeoSlicer.Utils.BoundRing.BoundingRing.PolygonToBoundRings(new Polygon(resultRing, new LinearRing[0]), Traverse);
//list1.First.Value.Ring = list1.First.Value.Ring.Next.Next.Next.Next.Next.Next;
//list2.First.Value.Ring = list2.First.Value.Ring.Next.Next.Next.Next.Next.Next;
GeoJsonFileService.WriteGeometryToFile(BoundingRing.BoundRingsToPolygon(list1), fileName + "daniilTest");
GeoJsonFileService.WriteGeometryToFile(BoundingRing.BoundRingsToPolygon(list2), fileName + "daniilTestAfter.geojson");
Console.WriteLine(BoundingRing.BoundRingsToPolygon(list2).IsValid);
IsValidOp validator = new IsValidOp(BoundingRing.BoundRingsToPolygon(list2));
Console.WriteLine(validator.ValidationError);
//GeoJsonFileService.WriteGeometryToFile(res, fileName + "daniilTest");
//GeoJsonFileService.WriteGeometryToFile(resultRing, fileName + "daniilTestAfter");*/


/*var polygon = GeoJsonFileService
    .ReadGeometryFromFile<Polygon>
        (fileName + "original");*/
var step = 0.1;
Coordinate firstRingCoord = new Coordinate(5, 3 + step);
LinearRing ring1 = new(
    new[]
    {
        new Coordinate(0, 0),
        firstRingCoord,
        new Coordinate(10, 0),
        new Coordinate(0, 0)
    });

Coordinate secondRingCoord = new Coordinate(1, 3 - step);
LinearRing ring2 = new(
    new[]
    {
        secondRingCoord,
        new Coordinate(1.5, 4),
        new Coordinate(2, 3),
        secondRingCoord
    });

Coordinate thirdRingCoord = new Coordinate(3, 3 - step);
LinearRing ring3 = new(
    new[]
    {
        thirdRingCoord,
        new Coordinate(3, 4),
        new Coordinate(4, 4),
        new Coordinate(4, 3),
        thirdRingCoord
    });
LinearRing shell = new(
    new[]
    {
        new Coordinate(-11, -11),
        new Coordinate(-11, 11),
        new Coordinate(11, 11),
        new Coordinate(11, -11),
        new Coordinate(-11, -11)
    });
        
Random random = new Random(1);
LinearRing[] rings = { ring1, ring2, ring3};
rings = rings.OrderBy(a => random.NextDouble()).ToArray();
Polygon polygon = new Polygon(shell, rings);
polygon = GeoJsonFileService.ReadGeometryFromFile<Polygon>( fileName + "test4");
double Epsilon = 1e-15;
 TraverseDirection Traverse = new (new LineService(Epsilon));
BoundingHoleDeleter Deleter =
    new (Traverse, 1e-15);

var res = Deleter.DeleteHoles(polygon);

GeoJsonFileService.WriteGeometryToFile(res, fileName + "test4_res");












