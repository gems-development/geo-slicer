using GeoSlicer.HoleDeleters;
using GeoSlicer.Tests.HoleDeletersTests;
using GeoSlicer.Utils;
using NetTopologySuite.Geometries;

string user = "User";
string fileName = "C:\\Users\\" + user + "\\Downloads\\Telegram Desktop\\";


double Epsilon = 1e-15;
TraverseDirection Traverse = new(new LineService(Epsilon));

BoundingHoleDeleter Deleter =
    new(Traverse, Epsilon);
ZeroTunnelDivider divider = ObjectsForTests.GetZeroTunnelDivider();
LinkedList<Coordinate> problemCoordinates;
LinearRing extendedTunnelsTest4;
var test4 = ObjectsForTests.GetBaikal();

Polygon newTest4 = Deleter.DeleteHoles(test4);
GeoJsonFileService.WriteGeometryToFile(newTest4, fileName + "newBaikal");
//divider.DivideZeroTunnels(newTest4.Shell, out extendedTunnelsTest4, out problemCoordinates);
//GeoJsonFileService.WriteGeometryToFile(extendedTunnelsTest4, fileName + "newBaikal2");












