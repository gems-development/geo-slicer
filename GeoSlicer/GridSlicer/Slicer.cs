using System;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Utils.PolygonClippingAlghorithm;
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

    private readonly WeilerAthertonAlghorithm _helper;

    public Slicer(WeilerAthertonAlghorithm helper)
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

        // Количество ячеек
        int xCount = (int)Math.Ceiling((xUp - xDown) / xScale);
        int yCount = (int)Math.Ceiling((yUp - yDown) / yScale);

        IEnumerable<Polygon>?[,] result = new IEnumerable<Polygon>?[xCount, yCount];

        // Методы получения номера ячейки, куда попадает координата
        int GetXIndex(Coordinate coordinate) => (int)Math.Floor((coordinate.X - xDown) / xScale);
        int GetXIndexCeil(Coordinate coordinate) => (int)Math.Ceiling((coordinate.X - xDown) / xScale);
        int GetYIndex(Coordinate coordinate) => (int)Math.Floor((coordinate.Y - yDown) / yScale);
        int GetYIndexCeil(Coordinate coordinate) => (int)Math.Ceiling((coordinate.Y - yDown) / yScale);

        // Очереди эмитируют очередь кортежей X-Y. Индексы ячеек, что надо проверить.
        // Если для ячейки все соседние ячейки не пересекаются с полигоном, она не будет добавлена в очередь 
        // (сделано это для оптимизации, чтобы не было кучи проверок, что заведомо вернут пустое пересечение).
        // Происходит это благодаря тому, что алгоритм идет "волной" от некоторой ячейки, не заходя в заведомо
        // пустые ячейки.
        Queue<int> xQueue = new Queue<int>();
        Queue<int> yQueue = new Queue<int>();

        // Добавление индексов в очереди, если клетка не за пределами сетки и она еще не была назначена на проверку
        void CheckAndAdd(int x, int y)
        {
            if (x < 0 || x >= xCount || y < 0 || y >= yCount)
            {
                return;
            }

            // Если не null, то там либо результат (ячейка обработана), либо вспомогательная ссылка, означающая,
            // что ячейка добавлена в очередь на обработку
            if (result[x, y] is null)
            {
                xQueue.Enqueue(x);
                yQueue.Enqueue(y);
                result[x, y] = _inQueue;
            }
        }

        // Добавляем в очередь какую либо ячейку, что пересекается с полигоном 
        int indexX1 = GetXIndex(inputPolygon.Coordinate);
        int indexX2 = GetXIndexCeil(inputPolygon.Coordinate);
        xQueue.Enqueue(indexX1);

        int indexY1 = GetYIndex(inputPolygon.Coordinate);
        int indexY2 = GetYIndexCeil(inputPolygon.Coordinate);
        yQueue.Enqueue(indexY1);

        // Добавляем еще одну ячейку. Необходимо в случае, если первая пересекается с полигоном только в одной точке
        if (indexX1 != indexX2 || indexY1 != indexY2)
        {
            xQueue.Enqueue(indexX2);
            yQueue.Enqueue(indexY2);
        }

        // Начало итеративного алгоритма
        while (xQueue.Count > 0)
        {
            int x = xQueue.Dequeue();
            int y = yQueue.Dequeue();
            // Получаем пересечение
            IntersectionType intersectionType = WeilerAthertonForGrid(inputPolygon,
                xDown + x * xScale, xDown + (x + 1) * xScale,
                yDown + y * yScale, yDown + (y + 1) * yScale, out IEnumerable<Polygon> weilerResult);


            // Заполнение текущей клетки
            switch (intersectionType)
            {
                case IntersectionType.BoxOutsideGeometry:
                    result[x, y] = _outside;
                    break;
                case IntersectionType.IntersectionWithEdge:
                    result[x, y] = weilerResult;
                    break;
                case IntersectionType.BoxInGeometry:
                    result[x, y] = _inner;
                    break;
                case IntersectionType.GeometryInBox:
                    result[x, y] = new[] { inputPolygon };
                    break;
            }

            // Добавление окружающих клеток в очередь (идем "волной" во все стороны)
            if (intersectionType is IntersectionType.IntersectionWithEdge or IntersectionType.BoxInGeometry)
            {
                CheckAndAdd(x - 1, y);
                CheckAndAdd(x + 1, y);
                CheckAndAdd(x, y - 1);
                CheckAndAdd(x, y + 1);
            }
        }

        // Объединяем внутренние прямоугольники
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
                // Пропускаем все не внутренние ячейки
                if (!ReferenceEquals(grid[x, y], _inner))
                {
                    continue;
                }

                // Нашли внутреннюю ячейку

                // Длины сторон прямоугольника (в количестве ячеек)
                int xSide = 0;
                int ySide = 0;
                // Смотрим, на сколько ячеек вправо можем расширить прямоугольник
                while (xSide + x < xLen && ReferenceEquals(grid[x + xSide + 1, y], _inner)) xSide++;
                // Смотрим, на сколько ячеек вниз можем расширить прямоугольник (его ширина больше 1 ячейки)
                while (ySide + y < yLen)
                {
                    // Флаг, показывающий, находится ли вся линия внутри прямоугольника
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

                IEnumerable<Polygon> rectangle = rectangleCreator.Invoke(x, x + xSide + 1, y, y + ySide + 1);

                // Заменяем ссылки с константного значения на большой прямоугольник / null
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

                y += ySide;
            }
        }
    }

    /// <summary>
    /// Надстройка над Атертоном. Возвращает пересечение прямоугольной ячейки с полигоном и тип этого пересечения
    /// </summary>
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