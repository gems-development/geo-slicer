using GeoSlicer.GridSlicer;
using GeoSlicer.GridSlicer.Helpers;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;


const double epsilon = 1E-15;

GeometryFactory gf =
    NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);

LineService lineService = new LineService(epsilon);
ICoordinateComparator coordinateComparator = new EpsilonCoordinateComparator(epsilon);

GridSlicerHelper helper = new GridSlicerHelper(new LinesIntersector(coordinateComparator, lineService, epsilon),
    lineService, coordinateComparator, new ContainsChecker(lineService, epsilon));

LinearRing linearRing = new LinearRing(GeoJsonFileService.ReadGeometryFromFile<LineString>("TestFiles/part1.geojson.ignore").Coordinates);
Polygon polygon = GeoJsonFileService.ReadGeometryFromFile<Polygon>("TestFiles/source.geojson.ignore");

var result = helper.WeilerAtherton(polygon.Shell, linearRing);



