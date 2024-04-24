using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.GridSlicer.Helpers;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using NetTopologySuite.Geometries;

namespace GeoSlicer.DivideAndRuleSlicers.OppositesSlicer;

public class Slicer
{
    private readonly LineService _lineService;

    private readonly int _maxPointsCount;

    // todo После вынесения метода пересечения заменить на нужный класс
    private readonly GridSlicerHelper _helper;

    public Slicer(LineService lineService, int maxPointsCount, GridSlicerHelper helper)
    {
        _lineService = lineService;
        _maxPointsCount = maxPointsCount;
        _helper = helper;
    }


    // todo О направлении обхода
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
            // todo Кажется, есть лишние разрезания
            Polygon current = queue.Dequeue();
            
            int oppositesIndex = Utils.GetNearestOppositesInner(current.Shell);
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
        }

        return result;
    }

    // todo Возможно можно исправить проблемы при повторяющихся точках
    // todo Заменить заглушку после написания алгоритма
    // todo Вынести в отдельный класс Вэйлера-Азертона с набором надстроек над основным алгоритмом)
    private IEnumerable<Polygon> SliceByLine(Polygon polygon, Coordinate a, Coordinate b)
    {
        a = a.Copy();
        b = b.Copy();

        // Если isVertical == true, создается 2 области: слева и справа от вертикального разделителя
        bool isVertical = Math.Abs(a.Y - b.Y) > Math.Abs(a.X - b.X);

        // Сортировка к a<b
        if (isVertical && a.Y > b.Y || !isVertical && a.X > b.X)
        {
            (a, b) = (b, a);
        }

        Envelope envelope = polygon.EnvelopeInternal;

        // Создаем нахлест чтобы наверняка
        double minY = envelope.MinY - Math.Abs(envelope.MinY) * 0.1;
        double maxY = envelope.MaxY + Math.Abs(envelope.MaxY) * 0.1;
        double minX = envelope.MinX - Math.Abs(envelope.MinX) * 0.1;
        double maxX = envelope.MaxX + Math.Abs(envelope.MaxX) * 0.1;
        if (isVertical)
        {
            if (minY < a.Y)
            {
                // todo Возможны проблемы с созданием новых точек в алгоритме пересечения, потому что точки прямой
                // для разрезания сдвигаются и больше не соответствуют существующим

                // Продлеваем прямую
                a.X += (a.X - b.X) * (a.Y - minY) / (b.Y - a.Y);
                a.Y = minY;
            }

            if (b.Y < maxY)
            {
                b.X -= (a.X - b.X) * (maxY - b.Y) / (b.Y - a.Y);
                b.Y = maxY;
            }
        }
        else
        {
            if (minX < a.X)
            {
                a.Y += (a.Y - b.Y) * (a.X - minX) / (b.X - a.X);
                a.X = minX;
            }

            if (b.X < maxX)
            {
                b.Y -= (a.Y - b.Y) * (maxX - b.X) / (b.X - a.X);
                b.X = maxX;
            }
        }

        // Если пересекается с 2мя смежными сторонами, в результате будет треугольник, у которого в одной точке
        // будет на самом деле 2 точки. Без этого будет самопересечения а-ля бантик
        minX = a.X;
        minY = a.Y;
        maxX = b.X;
        maxY = b.Y;
        LinearRing part1;
        LinearRing part2;
        if (isVertical)
        {
            part1 = new LinearRing(new []
                { a, new(minX, minY), new(minX, maxY), b, a });
            part2 = new LinearRing(new[]
                { a, b, new(maxX, maxY), new(maxX, minY), a });
        }
        else
        {
            part1 = new LinearRing(new[]
                { a, new(minX, maxY), new(maxX, maxY), b, a });
            part2 = new LinearRing(new[]
                { a, b, new(maxX, minY), new(minX, minY), a });
        }

        IEnumerable<LinearRing> resPart1 = _helper.WeilerAthertonStub(polygon.Shell, part1);
        IEnumerable<LinearRing> resPart2 = _helper.WeilerAthertonStub(polygon.Shell, part2);

        return resPart1.Concat(resPart2).Select(ring => new Polygon(ring));
    }
}