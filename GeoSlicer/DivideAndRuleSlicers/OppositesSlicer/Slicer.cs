using System;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Utils;
using GeoSlicer.Utils.PolygonClippingAlghorithm;
using NetTopologySuite.Geometries;

namespace GeoSlicer.DivideAndRuleSlicers.OppositesSlicer;

public class Slicer
{
    private readonly LineService _lineService;

    private readonly int _maxPointsCount;

    // todo После вынесения метода пересечения заменить на нужный класс
    private readonly WeilerAthertonForLine _weilerAthertonAlghorithm;

    private int _debugVar = 0;

    public Slicer(LineService lineService, int maxPointsCount, WeilerAthertonForLine weilerAthertonAlghorithm)
    {
        _lineService = lineService;
        _maxPointsCount = maxPointsCount;
        _weilerAthertonAlghorithm = weilerAthertonAlghorithm;
    }


    public IEnumerable<Polygon> Slice(Polygon input)
    {
        LinkedList<Polygon> result = new LinkedList<Polygon>();

        if (input.NumPoints <= _maxPointsCount)
        {
            result.AddLast(input);
            return result;
        }

        Queue<Polygon> queue = new Queue<Polygon>();
        queue.Enqueue(input);
        _debugVar = 0;
        while (queue.Count != 0)
        {
            Console.WriteLine(
                $"Number: {_debugVar}. Queue count: {queue.Count}. Max points count: {queue.Select(polygon => polygon.Shell.Count).Max()}");

            // todo Кажется, есть лишние разрезания
            Polygon current = queue.Dequeue();

            int oppositesIndex = Utils.GetOppositesIndexByTriangles(current.Shell);
            IEnumerable<Polygon> sliced = SliceByLine(
                current,
                current.Shell.GetCoordinateN(oppositesIndex),
                current.Shell.GetCoordinateN((oppositesIndex + current.Shell.Count / 2) % current.Shell.Count));


            foreach (Polygon ring in sliced)
            {
                if (ring.NumPoints <= _maxPointsCount)
                {
                    result.AddLast(ring);
                }
                else
                {
                    queue.Enqueue(ring);
                }
            }

            // GeoJsonFileService.WriteGeometryToFile(new MultiPolygon(queue.Concat(result).ToArray())
            //    , $"Out\\iter{_debugVar}.geojson.ignore");
            _debugVar++;
        }

        return result;
    }

    // todo Возможно можно исправить проблемы при повторяющихся точках
    // todo Вынести в отдельный класс Вэйлера-Азертона с набором надстроек над основным алгоритмом)
    private IEnumerable<Polygon> SliceByLine(Polygon polygon, Coordinate a, Coordinate b)
    {
        LineString line1 = new LineString(new[] { a, b });
        LineString line2 = new LineString(new[] { b, a });
        
        //if (_debugVar == 683)
        //{
        //    GeoJsonFileService.WriteGeometryToFile(polygon, "Out/source.geojson.ignore");
        //    GeoJsonFileService.WriteGeometryToFile(line1, "Out/line1.geojson.ignore");
        //    GeoJsonFileService.WriteGeometryToFile(line2, "Out/line2.geojson.ignore");
        //}

        IEnumerable<Polygon> resPart1 = _weilerAthertonAlghorithm.WeilerAtherton(polygon, line1);
        IEnumerable<Polygon> resPart2 = _weilerAthertonAlghorithm.WeilerAtherton(polygon, line1);


        // GeoJsonFileService.WriteGeometryToFile(new MultiPolygon(resPart1.ToArray()),
        //      "Out/resPart1.geojson.ignore");
        // GeoJsonFileService.WriteGeometryToFile(new MultiPolygon(resPart2.ToArray()),
        //     "Out/resPart2.geojson.ignore");

        return resPart1.Concat(resPart2);
    }
}