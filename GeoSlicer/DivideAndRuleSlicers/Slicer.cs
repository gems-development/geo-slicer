using System;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Utils.PolygonClippingAlghorithm;
using NetTopologySuite.Geometries;

namespace GeoSlicer.DivideAndRuleSlicers;

/// <summary>
/// Разрезает полигон на куски, количество точек в которых не превышает переданное в конструкторе ограничение.
/// Разрезает путем проведения линии по точкам, что противоположны по индексам (разница индексов = length / 2)
/// </summary>
public class Slicer
{
    private readonly int _maxPointsCount;

    private readonly WeilerAthertonForLine _weilerAtherton;
    private readonly IDivisionIndexesGiver _indexesGiver;

    // todo: Удалить после отладки
    private int _debugVar;

    public Slicer(int maxPointsCount, WeilerAthertonForLine weilerAtherton, IDivisionIndexesGiver indexesGiver)
    {
        _maxPointsCount = maxPointsCount;
        _weilerAtherton = weilerAtherton;
        _indexesGiver = indexesGiver;
    }

    public ICollection<Polygon> Slice(Polygon input, out ICollection<int> skippedGeometries)
    {
        LinkedList<Polygon> result = new LinkedList<Polygon>();
        LinkedList<int> skippedList = new LinkedList<int>();
        skippedGeometries = skippedList;

        if (input.NumPoints <= _maxPointsCount)
        {
            result.AddLast(input);
            return result;
        }

        Queue<Polygon> queue = new();
        queue.Enqueue(input);
        _debugVar = 0;
        while (queue.Count != 0)
        {
          //  Console.WriteLine(
                //       $"Number: {_debugVar}. Queue count: {queue.Count}. Max points count: {queue.Select(polygon => polygon.Shell.Count).Max()}");

            Polygon current = queue.Dequeue();

            _indexesGiver.GetIndexes(current.Shell, out int firstIndex, out int secondIndex);
            IEnumerable<Polygon> sliced;
            
            try
            {
                sliced = SliceByLine(
                    current,
                    current.Shell.GetCoordinateN(firstIndex),
                    current.Shell.GetCoordinateN(secondIndex));
            }
            catch (DifferentNumbersOfPointTypes)
            {
                result.AddLast(current);
                skippedList.AddLast(result.Count - 1);
                continue;
            }

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
        
        IEnumerable<Polygon> resPart1 = _weilerAtherton.WeilerAtherton(polygon, line1);
        IEnumerable<Polygon> resPart2 = _weilerAtherton.WeilerAtherton(polygon, line2);
        
        return resPart1.Concat(resPart2);
    }
}