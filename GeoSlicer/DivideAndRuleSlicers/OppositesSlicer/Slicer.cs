using System;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Utils.PolygonClippingAlghorithm;
using NetTopologySuite.Geometries;

namespace GeoSlicer.DivideAndRuleSlicers.OppositesSlicer;

/// <summary>
/// Разрезает полигон на куски, количество точек в которых не превышает переданное в конструкторе ограничение.
/// Разрезает путем проведения линии по точкам, что противоположны по индексам (разница индексов = length / 2)
/// </summary>
public class Slicer
{
    private readonly int _maxPointsCount;

    private readonly WeilerAthertonForLine _weilerAtherton;
    private readonly OppositesSlicerUtils _utils;

    // todo: Удалить после отладки
    private int _debugVar;

    public Slicer(int maxPointsCount, WeilerAthertonForLine weilerAtherton, OppositesSlicerUtils utils)
    {
        _maxPointsCount = maxPointsCount;
        _weilerAtherton = weilerAtherton;
        _utils = utils;
    }

    public IEnumerable<Polygon> Slice(Polygon input)
    {
        LinkedList<Polygon> result = new LinkedList<Polygon>();

        // Если геометрия сразу достаточно мала, возвращаем ее
        if (input.NumPoints <= _maxPointsCount)
        {
            result.AddLast(input);
            return result;
        }

        // Очередь на разрезание. Туда попадают только геометрии, в которых точек больше, чем _maxPointsCount
        Queue<Polygon> queue = new Queue<Polygon>();
        queue.Enqueue(input);
        _debugVar = 0;
        while (queue.Count != 0)
        {
            Console.WriteLine(
                $"Number: {_debugVar}. Queue count: {queue.Count}. Max points count: {queue.Select(polygon => polygon.Shell.Count).Max()}");

            Polygon current = queue.Dequeue();

            // Получаем индексы точек для разрезания.
            // Варьирование метода получения индексов может сильно изменить результат.
            int oppositesIndex = _utils.GetOppositesIndexByTriangles(current.Shell);
            IEnumerable<Polygon> sliced = SliceByLine(
                current,
                current.Shell.GetCoordinateN(oppositesIndex),
                current.Shell.GetCoordinateN((oppositesIndex + current.Shell.Count / 2) % current.Shell.Count));


            // Итерируемся по результату, отправляя маленькие геометрии в результат, большие в очередь на обработку
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

        // Необходимы оба вызова, так как Атертон возвращает только те геометрии, что в одной
        // некоторой стороны от линии.
        // Во втором вызове мы разворачиваем линию.
        IEnumerable<Polygon> resPart1 = _weilerAtherton.WeilerAtherton(polygon, line1);
        IEnumerable<Polygon> resPart2 = _weilerAtherton.WeilerAtherton(polygon, line2);


        // GeoJsonFileService.WriteGeometryToFile(new MultiPolygon(resPart1.ToArray()),
        //      "Out/resPart1.geojson.ignore");
        // GeoJsonFileService.WriteGeometryToFile(new MultiPolygon(resPart2.ToArray()),
        //     "Out/resPart2.geojson.ignore");

        return resPart1.Concat(resPart2);
    }
}