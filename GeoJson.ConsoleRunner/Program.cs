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
LineService lineService = new LineService(1E-15, coordinateComparator);

WeilerAthertonForLine weilerAtherton = new WeilerAthertonForLine(
    new LinesIntersector(new EpsilonCoordinateComparator(1E-15), new LineService(1E-10, coordinateComparator), 1E-12), lineService,
    coordinateComparator, new ContainsChecker(lineService, epsilon), epsilon);
Slicer slicer = new Slicer(5,
    weilerAtherton, new OppositesSlicerUtils(new LineService(1E-10, coordinateComparator)));

GeoJsonFileService geoJsonFileService = new GeoJsonFileService();


var polygon =
    (Polygon)((MultiPolygon)geoJsonFileService.ReadGeometryFromFile<FeatureCollection>("TestFiles\\kazan.geojson")[0]
        .Geometry)[0];

IEnumerable<Polygon> result = slicer.Slice(polygon, out _);

MultiPolygon multiPolygon = new MultiPolygon(result.ToArray());

geoJsonFileService.WriteGeometryToFile(multiPolygon, "Out\\darBaikal1500New.geojson.ignore");


/*
Polygon source = geoJsonFileService.ReadGeometryFromFile<Polygon>("Out\\polygon.geojson.ignore");
LineString part =
    geoJsonFileService.ReadGeometryFromFile<LineString>("Out\\line2.geojson.ignore");



IEnumerable<Polygon> result = weilerAtherton.WeilerAtherton(source, part);
MultiPolygon multiPolygon = new MultiPolygon(result.ToArray());
geoJsonFileService.WriteGeometryToFile(multiPolygon, "Out\\res.geojson.ignore");*/