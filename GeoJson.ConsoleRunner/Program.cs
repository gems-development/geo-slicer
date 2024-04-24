using GeoSlicer.DivideAndRuleSlicers.OppositesSlicer;
using GeoSlicer.GridSlicer.Helpers;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.Validators;
using NetTopologySuite.Geometries;


const double epsilon = 1E-19;

GeometryFactory gf =
    NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);

LineService lineService = new LineService(epsilon);
ICoordinateComparator coordinateComparator = new EpsilonCoordinateComparator(epsilon);

Slicer slicer = new Slicer(lineService, 10,
    new GridSlicerHelper(new LinesIntersector(coordinateComparator, lineService, epsilon), epsilon,
        lineService, coordinateComparator, new ContainsChecker(lineService, epsilon)));


LinearRing linearRing = new LinearRing(
    GeoJsonFileService.ReadGeometryFromFile<LineString>("TestFiles\\maloeOzeroLinearRing.geojson").Coordinates);

RepeatingPointsValidator repeatingPointsValidator =
    new RepeatingPointsValidator(new EpsilonCoordinateComparator(epsilon));

Console.WriteLine(repeatingPointsValidator.Check(linearRing, true));


IEnumerable<Polygon> result = slicer.Slice(new Polygon(linearRing));

MultiPolygon multiPolygon = new MultiPolygon(result.ToArray());

GeoJsonFileService.WriteGeometryToFile(multiPolygon, "TestFiles\\moDivideAndRule10.geojson.ignore");