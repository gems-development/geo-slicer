using GeoSlicer.GridSlicer;
using GeoSlicer.GridSlicer.Helpers;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;


const double epsilon = 1E-19;

GeometryFactory gf =
    NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);

LineService lineService = new LineService(epsilon);
ICoordinateComparator coordinateComparator = new EpsilonCoordinateComparator(epsilon);

Slicer slicer = new Slicer(new GridSlicerHelper(new LinesIntersector(coordinateComparator, lineService, epsilon),
    lineService, coordinateComparator, new ContainsChecker(lineService, epsilon)));

LinearRing linearRing = new LinearRing(
    GeoJsonFileService.ReadGeometryFromFile<LineString>("TestFiles\\maloeOzeroLinearRing.geojson").Coordinates);


IEnumerable<LinearRing>?[,] result = slicer.Slice(linearRing, 0.0001, 0.0001, true);

LinkedList<LineString> lineStrings = new LinkedList<LineString>();

foreach (IEnumerable<LinearRing>? linearRings in result)
{
    if (linearRings is null) continue;
    foreach (LinearRing ring in linearRings)
    {
        lineStrings.AddLast(ring);
    }
}

MultiLineString multiLineString = new MultiLineString(lineStrings.ToArray());

GeoJsonFileService.WriteGeometryToFile(multiLineString, "TestFiles\\moGrid00001.geojson.ignore");


