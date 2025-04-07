using System;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Utils.PolygonClippingAlgorithm;
using NetTopologySuite.Geometries;

// ReSharper disable UseArrayEmptyMethod

namespace GeoSlicer.GridSlicer;

/// <summary>
/// Разрезает полигон по прямоугольной сетке
/// </summary>
public class Slicer
{
    // Вспомогательные ссылки для временного внесения в матрицу результата.
    // Если ячейка не касается геометрии.
    private readonly IEnumerable<Polygon> _outside = new Polygon[0];

    // Если ячейка полностью содержится в геометрии
    private readonly IEnumerable<Polygon> _inner = new Polygon[0];

    // Если ячейка добавлена в очередь на обработку, но еще не обработана (см. CheckAndAdd)
    private readonly IEnumerable<Polygon> _inQueue = new Polygon[0];

    private readonly WeilerAthertonAlgorithm _helper;

    public Slicer(WeilerAthertonAlgorithm helper)
    {
        _helper = helper;
    }

    /// <summary>
    /// Разрезает полигон по прямоугольной сетке.
    /// Смежные внутренние (полностью равны ячейке) прямоугольники может объединить в один большой прямоугольник.
    /// </summary>
    /// <param name="inputPolygon">Разрезаемый полигон</param>
    /// <param name="xScale">Длина прямоугольника по X</param>
    /// <param name="yScale">Высота прямоугольника по Y</param>
    /// <param name="uniqueOnly">Если false, то для объединенных внутренних прямоугольников будет по несколько
    /// ссылок (в каждой ячейке, которая полностью содержится в этом прямоугольнике).
    /// Если true, то ссылка будет только в левой верхней ячейке</param>
    /// <returns>Матрица, где каждая ячейка хранит геометрии, попавшие в нее после разрезания</returns>
    public IEnumerable<Polygon>?[,] Slice(
        Polygon inputPolygon,
        double xScale, double yScale,
        bool uniqueOnly = false)
    {
        // SetUp
        Envelope envelope = inputPolygon.EnvelopeInternal;
        double xDown = envelope.MinX;
        double yDown = envelope.MinY;
        double xUp = envelope.MaxX;
        double yUp = envelope.MaxY;

        int xCount = (int)Math.Ceiling((xUp - xDown) / xScale);
        int yCount = (int)Math.Ceiling((yUp - yDown) / yScale);

        IEnumerable<Polygon>?[,] result = new IEnumerable<Polygon>?[xCount, yCount];

        // Очереди эмитируют очередь кортежей X-Y. Индексы ячеек, что надо проверить.
        // Если для ячейки все соседние ячейки не пересекаются с полигоном, она не будет добавлена в очередь 
        // (сделано это для оптимизации, чтобы не было кучи проверок, что заведомо вернут пустое пересечение).
        // Происходит это благодаря тому, что алгоритм идет "волной" от некоторой ячейки, не заходя в заведомо
        // пустые ячейки.
        Queue<int> xQueue = new Queue<int>();
        Queue<int> yQueue = new Queue<int>();

        EnqueueFirstIntersectsCell();

        while (xQueue.Count > 0)
        {
            int x = xQueue.Dequeue();
            int y = yQueue.Dequeue();
            IntersectionType intersectionType = WeilerAthertonForGrid(inputPolygon,
                xDown + x * xScale, xDown + (x + 1) * xScale,
                yDown + y * yScale, yDown + (y + 1) * yScale, out IEnumerable<Polygon> weilerResult);

            result[x, y] = intersectionType switch
            {
                IntersectionType.BoxOutsideGeometry => _outside,
                IntersectionType.IntersectionWithEdge => weilerResult,
                IntersectionType.BoxInGeometry => _inner,
                IntersectionType.GeometryInBox => new[] { inputPolygon },
                _ => result[x, y]
            };

            if (intersectionType is not (IntersectionType.IntersectionWithEdge or IntersectionType.BoxInGeometry))
                continue;
            // Добавление окружающих клеток в очередь (идем "волной" во все стороны)
            CheckAndAdd(x - 1, y);
            CheckAndAdd(x + 1, y);
            CheckAndAdd(x, y - 1);
            CheckAndAdd(x, y + 1);
        }

        ProcessInner(result,
            (xStart, xEnd, yStart, yEnd) => new[]
            {
                new Polygon(new LinearRing(new[]
                {
                    new Coordinate(xDown + xStart * xScale, yDown + yStart * yScale),
                    new Coordinate(xDown + xEnd * xScale, yDown + yStart * yScale),
                    new Coordinate(xDown + xEnd * xScale, yDown + yEnd * yScale),
                    new Coordinate(xDown + xStart * xScale, yDown + yEnd * yScale),
                    new Coordinate(xDown + xStart * xScale, yDown + yStart * yScale),
                }))
            },
            uniqueOnly);

        return result;


        // Методы получения номера ячейки, куда попадает координата
        int GetXIndex(Coordinate coordinate) => (int)Math.Floor((coordinate.X - xDown) / xScale);
        int GetXIndexCeil(Coordinate coordinate) => (int)Math.Ceiling((coordinate.X - xDown) / xScale);
        int GetYIndex(Coordinate coordinate) => (int)Math.Floor((coordinate.Y - yDown) / yScale);
        int GetYIndexCeil(Coordinate coordinate) => (int)Math.Ceiling((coordinate.Y - yDown) / yScale);

        void CheckAndAdd(int x, int y)
        {
            if (x < 0 || x >= xCount || y < 0 || y >= yCount)
            {
                return;
            }

            if (result[x, y] is not null) return;
            xQueue.Enqueue(x);
            yQueue.Enqueue(y);
            result[x, y] = _inQueue;
        }

        void EnqueueFirstIntersectsCell()
        {
            int indexX1 = GetXIndex(inputPolygon.Coordinate);
            int indexX2 = GetXIndexCeil(inputPolygon.Coordinate);
            xQueue.Enqueue(indexX1);

            int indexY1 = GetYIndex(inputPolygon.Coordinate);
            int indexY2 = GetYIndexCeil(inputPolygon.Coordinate);
            yQueue.Enqueue(indexY1);

            if (indexX1 == indexX2 && indexY1 == indexY2) return;
            // Добавляем еще одну ячейку. Необходимо в случае, если первая пересекается с полигоном только в одной точке
            xQueue.Enqueue(indexX2);
            yQueue.Enqueue(indexY2);
        }
    }


    // rectangleCreator принимает xStart, xEnd, yStart, yEnd
    private void ProcessInner(IEnumerable<Polygon>?[,] grid,
        Func<int, int, int, int, IEnumerable<Polygon>> rectangleCreator,
        bool uniqueOnly)
    {
        int xLen = grid.GetLength(0);
        int yLen = grid.GetLength(1);

        for (int x = 0; x < xLen; x++)
        {
            for (int y = 0; y < yLen; y++)
            {
                if (!ReferenceEquals(grid[x, y], _inner))
                {
                    continue;
                }

                GetRectangleSides(x, y, out int xSide, out int ySide);

                IEnumerable<Polygon> rectangle = rectangleCreator.Invoke(x, x + xSide + 1, y, y + ySide + 1);

                SetRefs(xSide, ySide, x, y, rectangle);

                y += ySide;
            }
        }

        void GetRectangleSides(int x, int y, out int xSide, out int ySide)
        {
            xSide = 0;
            ySide = 0;
            while (xSide + x < xLen && ReferenceEquals(grid[x + xSide + 1, y], _inner)) xSide++;
            while (ySide + y < yLen)
            {
                bool isInnerLine = true;
                for (int i = x; i <= x + xSide; i++)
                {
                    if (!ReferenceEquals(grid[i, y + ySide + 1], _inner))
                    {
                        isInnerLine = false;
                        break;
                    }
                }

                if (!isInnerLine)
                {
                    break;
                }

                ySide++;
            }
        }

        void SetRefs(int xSide, int ySide, int x, int y, IEnumerable<Polygon> rectangle)
        {
            for (int i = 0; i <= xSide; i++)
            {
                for (int j = 0; j <= ySide; j++)
                {
                    grid[x + i, y + j] = uniqueOnly ? null : rectangle;
                }
            }

            if (uniqueOnly)
            {
                grid[x, y] = rectangle;
            }
        }
    }

    private IntersectionType WeilerAthertonForGrid(
        Polygon clipped, double xDown, double xUp, double yDown, double yUp, out IEnumerable<Polygon> result)
    {
        Coordinate[] boxCoordinates =
        {
            new(xDown, yDown),
            new(xDown, yUp),
            new(xUp, yUp),
            new(xUp, yDown),
            new(xDown, yDown)
        };

        LinearRing boxLinearRing = new LinearRing(boxCoordinates);

        result = _helper.WeilerAtherton(clipped, boxLinearRing);

        // Определяем, какой у нас тип пересечения
        if (result.Count() == 1)
        {
            if (result.First().Shell == boxLinearRing)
            {
                return IntersectionType.BoxInGeometry;
            }

            if (result.First() == clipped)
            {
                return IntersectionType.GeometryInBox;
            }
        }

        if (!result.Any())
        {
            return IntersectionType.BoxOutsideGeometry;
        }

        return IntersectionType.IntersectionWithEdge;
    }
}