using GeoSlicer.GeoJsonFileService;
using GeoSlicer.NonConvexSlicer;
using GeoSlicer.Utils;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

string fileName = "TestFiles\\kazan_fix_2.geojson";
var featureCollection = GeoJsonFileService.ReadGeometryFromFile<FeatureCollection>(fileName);

var polygon = (Polygon)(((MultiPolygon)(featureCollection[0].Geometry))[0]);

// var polygon = (Polygon)GeoJsonFileService.ReadGeometryFromFile<MultiPolygon>(fileName)[0];

var slicer = new NonConvexSlicer(1e-9);

var list = slicer.Slice(polygon.Shell);

var listPolygons = new List<Polygon>();


foreach (var iter in list)
{
    polygon = new Polygon(iter);
    listPolygons.Add(polygon);
}

MultiPolygon multiPolygon = new MultiPolygon(listPolygons.ToArray());
GeoJsonFileService.WriteGeometryToFile(multiPolygon, "TestFiles\\kazan_porezannaya.geojson");