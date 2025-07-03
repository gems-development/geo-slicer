using System.Collections.Generic;
using GeoSlicer.Utils;
using GeoSlicer.Utils.PolygonClippingAlgorithm;
using NetTopologySuite.Geometries;

namespace GeoSlicer.DivideAndRuleSlicers;

/// <summary>
/// Разрезает полигон на куски, количество точек в которых не превышает переданное в конструкторе ограничение.
/// Разрезает путем проведения линии по точкам, что противоположны по индексам (разница индексов = length / 2)
/// </summary>
public class Slicer
{
    private readonly int _maxPointsCount;
    
    private readonly IDivisionIndexesGiver _indexesGiver;
    private readonly WeilerAthertonPolygonSlicer _slicer;

    public Slicer(
        int maxPointsCount,
        IDivisionIndexesGiver indexesGiver,
        WeilerAthertonPolygonSlicer slicer)
    {
        _maxPointsCount = maxPointsCount;
        _indexesGiver = indexesGiver;
        _slicer = slicer;
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
        while (queue.Count != 0)
        {

            Polygon current = queue.Dequeue();

            _indexesGiver.GetIndexes(current.Shell, out int firstIndex, out int secondIndex);
            if (firstIndex == -1)
            {
                Skip(current);
                continue;
            }

            IEnumerable<Polygon> sliced;

            try
            {
                sliced = _slicer.SliceByLine(
                    current,
                    current.Shell.GetCoordinateN(firstIndex),
                    current.Shell.GetCoordinateN(secondIndex));
            }
            catch (DifferentNumbersOfPointTypes)
            {
                Skip(current);
                continue;
            }

            foreach (Polygon polygon in sliced)
            {
                if (polygon.NumPoints <= _maxPointsCount)
                {
                    result.AddLast(polygon);
                }
                else if (polygon.NumPoints == current.NumPoints)
                {
                    Skip(polygon);
                }
                else
                {
                    queue.Enqueue(polygon);
                }
            }
        }

        return result;

        void Skip(Polygon skippedPolygon)
        {
            result.AddLast(skippedPolygon);
            skippedList.AddLast(result.Count - 1);
        }
    }
}