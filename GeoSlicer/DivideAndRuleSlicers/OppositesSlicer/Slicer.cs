using System;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using NetTopologySuite.Geometries;

namespace GeoSlicer.DivideAndRuleSlicers.OppositesSlicer;

public class Slicer
{
    private readonly OppositesSlicerHelper _oppositesSlicerHelper;
    private readonly LinesIntersector _linesIntersector;
    private readonly LineService _lineService;
    private readonly int _maxPointsCount;

    public Slicer(OppositesSlicerHelper oppositesSlicerHelper, LinesIntersector linesIntersector,
        LineService lineService, int maxPointsCount)
    {
        _oppositesSlicerHelper = oppositesSlicerHelper;
        _linesIntersector = linesIntersector;
        _lineService = lineService;
        _maxPointsCount = maxPointsCount;
    }


    public IEnumerable<LinearRing> Slice(LinearRing input, int maxPointsCount = -1)
    {
        LinkedList<LinearRing> result = new LinkedList<LinearRing>();

        if (input.Count <= maxPointsCount)
        {
            result.AddLast(input);
            return result;
        }

        Queue<LinearRing> queue = new Queue<LinearRing>();
        queue.Enqueue(input);

        while (queue.Count != 0)
        {
            LinearRing current = queue.Dequeue();
            int oppositesIndex = Utils.GetNearestOppositesInner(current);
            IEnumerable<LinearRing> sliced = SliceByLine(
                current,
                current.GetCoordinateN(oppositesIndex),
                current.GetCoordinateN((oppositesIndex + current.Count / 2) % current.Count));
            foreach (LinearRing ring in sliced)
            {
                if (ring.Count <= maxPointsCount)
                {
                    result.AddLast(ring);
                }
                else
                {
                    queue.Enqueue(ring);
                }
            }
        }

        return result;
    }

    // todo Сделать соединение без создания новых точек по возможности (мб принимать опцию на это)
    // todo Вынести в отдельный класс, мб пригодится извне
    // todo Подумать, есть ли смысл возвращать 2 коллекции с разделенными данными вместо одной
    private IEnumerable<LinearRing> SliceByLine(LinearRing input, Coordinate a, Coordinate b)
    {
        LinkedList<LinearRing> result = new LinkedList<LinearRing>();
        Coordinate[] coordinates = input.Coordinates;
        
        LinkedList<Coordinate> current

        // todo учесть вырезы колец
        // todo Задуматься о быстром объединении LinkedList
        int sign = Math.Sign(_lineService.VectorProduct(
            a, b, 
            coordinates[0], coordinates[1]));

        for (int i = 1; i < coordinates.Length - 1; i++)
        {
            
        }
    }
}