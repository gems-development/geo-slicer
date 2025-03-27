using GeoSlicer.DivideAndRuleSlicers;
using GeoSlicer.DivideAndRuleSlicers.OppositesIndexesGivers;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.PolygonClippingAlghorithm;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;


const double epsilon = 1E-15;

LineService lineService = new LineService(1E-15, new EpsilonCoordinateComparator(1E-8));

WeilerAthertonForLine weilerAtherton = new WeilerAthertonForLine(
    new LinesIntersector(new EpsilonCoordinateComparator(1E-15),
        new LineService(1E-10, new EpsilonCoordinateComparator(1E-8)), 1E-12),
    new LineService(1E-15, new EpsilonCoordinateComparator(1E-8)),
    new EpsilonCoordinateComparator(1E-8),
    new ContainsChecker(new LineService(1E-15, new EpsilonCoordinateComparator(1E-8)), 1E-15), 1E-15);
Slicer slicer = new Slicer(25,
    weilerAtherton, new ConvexityIndexesGiver(new LineService(1E-10, new EpsilonCoordinateComparator(1E-8))));

GeoJsonFileService geoJsonFileService = new GeoJsonFileService();


var polygon =
    (Polygon)((MultiPolygon)geoJsonFileService.ReadGeometryFromFile<FeatureCollection>("TestFiles\\kazan.geojson")[0]
        .Geometry)[0];

Polygon[] result = slicer.Slice(polygon, out ICollection<int> skippedGeomsIndexes).ToArray();

Console.WriteLine("Skipped: " + skippedGeomsIndexes.Count);
Console.WriteLine("Sum(Skipped.Count): " + skippedGeomsIndexes.Sum(i => result[i].Shell.Count));

MultiPolygon multiPolygon = new MultiPolygon(result.ToArray());

geoJsonFileService.WriteGeometryToFile(multiPolygon, "Out\\nearests.geojson.ignore");


/*
Polygon source = geoJsonFileService.ReadGeometryFromFile<Polygon>("Out\\source.geojson.ignore");
LineString part =
    geoJsonFileService.ReadGeometryFromFile<LineString>("Out\\cutting.geojson.ignore");



IEnumerable<Polygon> result = weilerAtherton.WeilerAtherton(source, part);
MultiPolygon multiPolygon = new MultiPolygon(result.ToArray());
geoJsonFileService.WriteGeometryToFile(multiPolygon, "Out\\res.geojson.ignore");*/