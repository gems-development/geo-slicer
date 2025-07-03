using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GeoSlicer.Utils.PolygonClippingAlgorithm;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils;

public class WeilerAthertonPolygonSlicer
{
    private readonly WeilerAthertonAlgorithm _weilerAtherton;

    public WeilerAthertonPolygonSlicer(WeilerAthertonAlgorithm weilerAtherton)
    {
        _weilerAtherton = weilerAtherton;
    }

    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public IEnumerable<Polygon> SliceByLine(Polygon polygon, Coordinate a, Coordinate b)
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

        IEnumerable<Polygon> resPart1 = _weilerAtherton.WeilerAtherton(polygon, part1);
        IEnumerable<Polygon> resPart2 = _weilerAtherton.WeilerAtherton(polygon, part2);

        return resPart1.Concat(resPart2);
    }
}