using GeoSlicer.HoleDeleters;
using GeoSlicer.Utils;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using GeoJsonFileService = GeoSlicer.Utils.GeoJsonFileService;

string user = "User";
string fileName = "C:\\Users\\" + user + "\\Downloads\\Telegram Desktop\\";
var featureCollection = GeoJsonFileService
    .ReadGeometryFromFile<FeatureCollection>
        ("TestFiles\\baikal.geojson");
var polygon = (Polygon)((MultiPolygon)featureCollection[0].Geometry)[0];


TraverseDirection Traverse = new (new LineService(1e-15));
BoundingHoleDeleter Deleter = new (Traverse, 1e-15);
GeoJsonFileService.WriteGeometryToFile(Deleter.DeleteHoles(polygon), fileName + "newBaikal");












