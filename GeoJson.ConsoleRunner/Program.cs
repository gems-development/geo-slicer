using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.PolygonClippingAlghorithm;
using NetTopologySuite.Geometries;


const double epsilon = 1E-14;

GeometryFactory gf =
    NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);

LineService lineService = new LineService(epsilon);
EpsilonCoordinateComparator coordinateComparator = new EpsilonCoordinateComparator(1e-7);

WeilerAthertonAlghorithm helper = new WeilerAthertonAlghorithm(new LinesIntersector(coordinateComparator, lineService, epsilon),
    lineService, coordinateComparator, new ContainsChecker(lineService, epsilon));

LinearRing linearRing = new LinearRing(GeoJsonFileService.ReadGeometryFromFile<LineString>("TestFiles/part2.geojson.ignore").Coordinates);
Polygon polygon = GeoJsonFileService.ReadGeometryFromFile<Polygon>("TestFiles/source.geojson.ignore");
if (!new TraverseDirection(lineService).IsClockwiseBypass(polygon.Shell)) new TraverseDirection(lineService).ChangeDirection(polygon.Shell);

var result = new MultiPolygon(helper.WeilerAtherton(polygon.Shell, linearRing).Select(o => new Polygon(o)).ToArray());

GeoJsonFileService.WriteGeometryToFile(result, "TestFiles\\bug_result_2.geojson.ignore");



