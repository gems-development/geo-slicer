using GeoSlicer.DivideAndRuleSlicers.OppositesSlicer;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.PolygonClippingAlghorithm;
using GeoSlicer.Utils.Validators;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;


const double epsilon = 1E-15;

EpsilonCoordinateComparator coordinateComparator = new EpsilonCoordinateComparator(1e-8);
LineService lineService = new LineService(epsilon, coordinateComparator);

//SlicerOld slicer = new SlicerOld(lineService, 5,
WeilerAthertonForLine weilerAthertonAlghorithm = new WeilerAthertonForLine(
    new LinesIntersector(coordinateComparator, lineService, epsilon), lineService,
    coordinateComparator, new ContainsChecker(lineService, epsilon), epsilon);
//Slicer slicer = new Slicer(lineService, 140,
//    weilerAthertonAlghorithm);

/*
var polygon =
    (Polygon)((MultiPolygon)GeoJsonFileService.ReadGeometryFromFile<FeatureCollection>("TestFiles\\kazan.geojson")[0]
        .Geometry)[0];

IEnumerable<Polygon> result = slicer.Slice(polygon);

MultiPolygon multiPolygon = new MultiPolygon(result.ToArray());

GeoJsonFileService.WriteGeometryToFile(multiPolygon, "Out\\darBaikal1500New.geojson.ignore");
*/


Polygon source = GeoJsonFileService.ReadGeometryFromFile<Polygon>("Out\\source.geojson.ignore");
LineString part =
    GeoJsonFileService.ReadGeometryFromFile<LineString>("Out\\partForLine.geojson.ignore");

IEnumerable<Polygon> result = weilerAthertonAlghorithm.WeilerAtherton(source, part);
MultiPolygon multiPolygon = new MultiPolygon(result.ToArray());
GeoJsonFileService.WriteGeometryToFile(multiPolygon, "Out\\res.geojson.ignore");