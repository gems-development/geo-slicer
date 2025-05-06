using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

    private readonly WeilerAthertonAlgorithm _weilerAtherton;
    private readonly IDivisionIndexesGiver _indexesGiver;

    // todo: Удалить после отладки
    private int _debugVar;

    public Slicer(int maxPointsCount, WeilerAthertonAlgorithm weilerAtherton, IDivisionIndexesGiver indexesGiver)
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
         //   Console.WriteLine(
        //        $"Number: {_debugVar}. Queue count: {queue.Count}. Max points count: {queue.Select(polygon => polygon.Shell.Count).Max()}");

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
                sliced = SliceByLine(
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

            //new GeoJsonFileService().WriteGeometryToFile(new MultiPolygon(queue.Concat(result).ToArray())
            //    , $"Out\\iter{_debugVar}.geojson.ignore");
            _debugVar++;
        }

        return result;

        void Skip(Polygon skippedPolygon)
        {
            result.AddLast(skippedPolygon);
            skippedList.AddLast(result.Count - 1);
        }
    }

    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
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
        double minY = envelope.MinY - (envelope.MaxY - envelope.MinY) * 0.1;
        double maxY = envelope.MaxY + (envelope.MaxY - envelope.MinY) * 0.1;
        double minX = envelope.MinX - (envelope.MaxY - envelope.MinY) * 0.1;
        double maxX = envelope.MaxX + (envelope.MaxY - envelope.MinY) * 0.1;
        if (isVertical)
        {
            if (minY < a.Y)
            {
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
        minX = Math.Min(minX, Math.Min(a.X, b.X));
        minY = Math.Min(minY, Math.Min(a.Y, b.Y));
        maxX = Math.Max(maxX, Math.Max(a.X, b.X));
        maxY = Math.Max(maxY, Math.Max(a.Y, b.Y));
        LinearRing part1;
        LinearRing part2;
        if (isVertical)
        {
            // Чтобы избежать создания треугольника с лишним дублем точки
            if (minX == b.X)
            {
                part1 = new LinearRing(new[]
                    { a, new(minX, minY), b, a });
            }
            else if (minX == a.X)
            {
                part1 = new LinearRing(new[]
                    { a, new(minX, maxY), b, a });
            }
            else
            {
                part1 = new LinearRing(new[]
                    { a, new(minX, minY), new(minX, maxY), b, a });
            }

            if (maxX == a.X)
            {
                part2 = new LinearRing(new[]
                    { a, b, new(maxX, maxY), a });
            }
            else if (maxX == b.X)
            {
                part2 = new LinearRing(new[]
                    { a, b, new(maxX, minY), a });
            }
            else
            {
                part2 = new LinearRing(new[]
                    { a, b, new(maxX, maxY), new(maxX, minY), a });
            }
        }
        else
        {
            // part1 максы и Б part2 минимумы и А

            if (maxY == a.Y)
            {
                part1 = new LinearRing(new[]
                    { a, new(maxX, maxY), b, a });
            }
            else if (maxY == b.Y)
            {
                part1 = new LinearRing(new[]
                    { a, new(minX, maxY), b, a });
            }
            else
            {
                part1 = new LinearRing(new[]
                    { a, new(minX, maxY), new(maxX, maxY), b, a });
            }

            if (minY == a.Y)
            {
                part2 = new LinearRing(new[]
                    { a, b, new(maxX, minY), a });
            }
            else if (minY == b.Y)
            {
                part2 = new LinearRing(new[]
                    { a, b, new(minX, minY), a });
            }
            else
            {
                part2 = new LinearRing(new[]
                    { a, b, new(maxX, minY), new(minX, minY), a });
            }
        }

        if (_debugVar == 150002)
            // if (b.Equals2D(new Coordinate(48.9460655482931, 55.809165282259364)))
        {
            new GeoJsonFileService().WriteGeometryToFile(polygon, "Out/clipped.geojson.ignore");
            new GeoJsonFileService().WriteGeometryToFile(polygon.Shell, "Out/clippedShell.geojson.ignore");
            new GeoJsonFileService().WriteGeometryToFile(part1, "Out/cutting1.geojson.ignore");
            new GeoJsonFileService().WriteGeometryToFile(part2, "Out/cutting2.geojson.ignore");
        }

        IEnumerable<Polygon> resPart1 = _weilerAtherton.WeilerAtherton(polygon, part1);
        IEnumerable<Polygon> resPart2 = _weilerAtherton.WeilerAtherton(polygon, part2);

        return resPart1.Concat(resPart2);
    }
}