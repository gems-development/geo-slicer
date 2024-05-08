using GeoSlicer.DivideAndRuleSlicers.OppositesSlicer;
using GeoSlicer.GridSlicer.Helpers;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.Validators;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;


const double epsilon = 1E-15;

GeometryFactory gf =
    NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);

LineService lineService = new LineService(epsilon);
EpsilonCoordinateComparator coordinateComparator = new EpsilonCoordinateComparator(1e-9);

SlicerOld slicer = new SlicerOld(lineService, 5,
    new GridSlicerHelper(new LinesIntersector(coordinateComparator, lineService, epsilon), lineService,
        coordinateComparator, new ContainsChecker(lineService, epsilon)));

//
// var polygon =
//     (Polygon)((MultiPolygon)GeoJsonFileService.ReadGeometryFromFile<FeatureCollection>("TestFiles\\baikal.geojson")[0]
//         .Geometry)[0];
//
// LinearRing shell = polygon.Shell;

LinearRing shell = new LinearRing(GeoJsonFileService
    .ReadGeometryFromFile<LineString>("TestFiles/maloeOzeroLinearRing.geojson").Coordinates);

RepeatingPointsValidator repeatingPointsValidator =
    new RepeatingPointsValidator(new EpsilonCoordinateComparator(epsilon));

Console.WriteLine(repeatingPointsValidator.Check(shell, true));

if (!TraverseDirection.IsClockwiseBypass(shell))
{
    TraverseDirection.ChangeDirection(shell);
}

IEnumerable<Polygon> result = slicer.Slice(new Polygon(shell));

MultiPolygon multiPolygon = new MultiPolygon(result.ToArray());

GeoJsonFileService.WriteGeometryToFile(multiPolygon, "Out\\mo.geojson.ignore");