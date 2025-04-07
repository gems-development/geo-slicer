using GeoSlicer.DivideAndRuleSlicers;
using GeoSlicer.DivideAndRuleSlicers.OppositesIndexesGivers;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using GeoSlicer.Utils.PolygonClippingAlgorithm;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

WeilerAthertonAlgorithm weilerAtherton = new WeilerAthertonAlgorithm(
    new LinesIntersector(new EpsilonCoordinateComparator(1E-9),
        new LineService(1E-10, new EpsilonCoordinateComparator(1E-10)), 1E-15),
    new LineService(1E-15, new EpsilonCoordinateComparator(1E-8)),
    new EpsilonCoordinateComparator(1E-8),
    new ContainsChecker(new LineService(1E-15, new EpsilonCoordinateComparator(1E-8)), 1E-15));
Slicer slicer = new Slicer(5,
    weilerAtherton, new ConvexityIndexesGiver(new LineService(1E-5, new EpsilonCoordinateComparator(1E-8))));

GeoJsonFileService geoJsonFileService = new GeoJsonFileService();


var polygon =
    (Polygon)((MultiPolygon)geoJsonFileService.ReadGeometryFromFile<FeatureCollection>("TestFiles\\kazan.geojson")[0]
        .Geometry)[0];
Polygon[] result = slicer.Slice(polygon, out ICollection<int> skippedGeomsIndexes).ToArray();

Console.WriteLine("Skipped: " + skippedGeomsIndexes.Count);
Console.WriteLine("Sum(Skipped.Count): " + skippedGeomsIndexes.Sum(i => result[i].Shell.Count));
Console.WriteLine("Max(Skipped.Count): " + skippedGeomsIndexes.Max(i => result[i].Shell.Count));

MultiPolygon multiPolygon = new MultiPolygon(result.ToArray());
MultiPolygon skipped = new MultiPolygon(result.Where((polygon1, i) => skippedGeomsIndexes.Contains(i)).ToArray());

geoJsonFileService.WriteGeometryToFile(multiPolygon, "Out\\resKazan.geojson.ignore");
geoJsonFileService.WriteGeometryToFile(skipped, "Out\\skippedKazan.geojson.ignore");


/*
Polygon source = geoJsonFileService.ReadGeometryFromFile<Polygon>("Out\\clipped.geojson.ignore");
LinearRing part = new LinearRing(
        geoJsonFileService.ReadGeometryFromFile<LineString>("Out\\cutting2.geojson.ignore").Coordinates);



IEnumerable<Polygon> result = weilerAtherton.WeilerAtherton(source, part);
MultiPolygon multiPolygon = new MultiPolygon(result.ToArray());
geoJsonFileService.WriteGeometryToFile(multiPolygon, "Out\\res.geojson.ignore");*/