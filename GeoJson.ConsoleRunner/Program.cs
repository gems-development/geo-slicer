using GeoSlicer.DivideAndRuleSlicers.OppositesSlicer;
using GeoSlicer.GridSlicer.Helpers;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.Validators;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;


const double epsilon = 1E-19;

GeometryFactory gf =
    NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);

LineService lineService = new LineService(epsilon);
ICoordinateComparator coordinateComparator = new EpsilonCoordinateComparator(epsilon);

Slicer slicer = new Slicer(lineService, 4,
    new GridSlicerHelper(new LinesIntersector(coordinateComparator, lineService, epsilon), lineService,
        coordinateComparator, new ContainsChecker(lineService, epsilon)));


var polygon =
    (Polygon)((MultiPolygon)GeoJsonFileService.ReadGeometryFromFile<FeatureCollection>("TestFiles\\baikal.geojson")[0]
        .Geometry)[0];

LinearRing shell = polygon.Shell;

RepeatingPointsValidator repeatingPointsValidator =
    new RepeatingPointsValidator(new EpsilonCoordinateComparator(epsilon));

Console.WriteLine(repeatingPointsValidator.Check(shell, true));

if (!TraverseDirection.IsClockwiseBypass(shell))
{
    TraverseDirection.ChangeDirection(shell);
}

IEnumerable<Polygon> result = slicer.Slice(new Polygon(shell));

MultiPolygon multiPolygon = new MultiPolygon(result.ToArray());

GeoJsonFileService.WriteGeometryToFile(multiPolygon, "TestFiles\\baikalDivideAndRule4.geojson.ignore");