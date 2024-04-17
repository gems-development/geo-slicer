using System;
using System.Collections;
using System.Collections.Generic;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using NetTopologySuite.Geometries;

namespace GeoSlicer.DivideAndRuleSlicers.OppositesSlicer;

public class Slicer
{
    private readonly LineService _lineService;
    private readonly int _maxPointsCount;

    public Slicer(LineService lineService, int maxPointsCount)
    {
        _lineService = lineService;
        _maxPointsCount = maxPointsCount;
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

        while (queue.Count != 0)
        {
            Polygon current = queue.Dequeue();
            int oppositesIndex = Utils.GetNearestOppositesInner(current.Shell);
            Func<Polygon, Coordinate, Coordinate, IEnumerable<Polygon>> sliceByLine = (polygon, coordinate, arg3) =>
                throw new NotImplementedException();
            IEnumerable<Polygon> sliced = sliceByLine.Invoke(
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
        }

        return result;
    }
}