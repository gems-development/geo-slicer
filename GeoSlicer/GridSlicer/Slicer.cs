using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

// ReSharper disable UseArrayEmptyMethod

namespace GeoSlicer.GridSlicer;

public class Slicer
{
    private readonly IEnumerable<LinearRing> _outside = new LinearRing[0];
    private readonly IEnumerable<LinearRing> _inner = new LinearRing[0];
    private readonly IEnumerable<LinearRing> _inQueue = new LinearRing[0];

    private readonly GridSlicerHelper _helper;

    public Slicer(GridSlicerHelper helper)
    {
        _helper = helper;
    }

    public IEnumerable<LinearRing>?[,] Slice(LinearRing inputRing, double xScale, double yScale)
    {
        // SetUp

        double xDown = Double.MaxValue;
        double yDown = Double.MaxValue;
        double xUp = Double.MinValue;
        double yUp = Double.MinValue;

        // todo Рассмотреть возможность избежания копирования
        foreach (Coordinate coordinate in inputRing.Coordinates)
        {
            xDown = Math.Min(xDown, coordinate.X);
            yDown = Math.Min(yDown, coordinate.Y);
            xUp = Math.Max(xUp, coordinate.X);
            yUp = Math.Max(yUp, coordinate.Y);
        }

        int xCount = (int)Math.Ceiling((xUp - xDown) / xScale);
        int yCount = (int)Math.Ceiling((yUp - yDown) / yScale);

        /*
        LinkedList<LineString> grid = new LinkedList<LineString>();
        for (int x = 0; x < xCount; x++)
        {
            for (int y = 0; y < yCount; y++)
            {
                grid.AddLast(new LineString(new[]
                {
                    new Coordinate(xDown + x * xScale, yDown + y * yScale),
                    new Coordinate(xDown + (x + 1) * xScale, yDown + y * yScale),
                    new Coordinate(xDown + (x + 1) * xScale, yDown + (y + 1) * yScale),
                    new Coordinate(xDown + x * xScale, yDown + (y + 1) * yScale),
                    new Coordinate(xDown + x * xScale, yDown + y * yScale),
                }));
            }
        }
        GeoJsonFileService.WriteGeometryToFile(new MultiLineString(grid.ToArray()), "TestFiles\\grid.geojson.ignore");
        */


        IEnumerable<LinearRing>?[,] result = new IEnumerable<LinearRing>?[xCount, yCount];

        int GetXIndex(Coordinate coordinate) => (int)Math.Ceiling((coordinate.X - xDown) / xScale);
        int GetYIndex(Coordinate coordinate) => (int)Math.Ceiling((coordinate.Y - yDown) / yScale);

        Queue<int> xQueue = new Queue<int>();
        Queue<int> yQueue = new Queue<int>();

        // Добавление индексов в очереди, если клетка не за пределами сетки и она еще не была назначена на проверку
        void CheckAndAdd(int x, int y)
        {
            if (x < 0 || x >= xCount || y < 0 || y >= yCount)
            {
                return;
            }

            if (result[x, y] is null)
            {
                xQueue.Enqueue(x);
                yQueue.Enqueue(y);
                result[x, y] = _inQueue;
            }
        }

        xQueue.Enqueue(GetXIndex(inputRing.Coordinate));
        yQueue.Enqueue(GetYIndex(inputRing.Coordinate));


        Console.WriteLine($"xCount: {xCount}, yCount: {yCount}");
        int total = xCount * yCount;
        int current = 0;

        // Начало итеративного алгоритма
        while (xQueue.Count > 0)
        {
            Console.WriteLine($"{current}/{total}");
            current++;
            int x = xQueue.Dequeue();
            int y = yQueue.Dequeue();
            IntersectionType intersectionType = _helper.WeilerAtherton(inputRing,
                xDown + x * xScale, xDown + (x + 1) * xScale,
                yDown + y * yScale, yDown + (y + 1) * yScale, out IEnumerable<LinearRing> weilerResult);

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
                    result[x, y] = new[] { inputRing };
                    break;
            }

            // Добавление нужных окружающих клеток в очередь
            if (intersectionType is IntersectionType.IntersectionWithEdge or IntersectionType.BoxInGeometry)
            {
                CheckAndAdd(x - 1, y);
                CheckAndAdd(x + 1, y);
                CheckAndAdd(x, y - 1);
                CheckAndAdd(x, y + 1);
            }
        }

        ProcessInner(result,
            (xStart, xEnd, yStart, yEnd) => new []{new LinearRing(new Coordinate[]
            {
                new Coordinate(xDown + xStart * xScale, yDown + yStart * yScale),
                new Coordinate(xDown + xEnd * xScale, yDown + yStart * yScale),
                new Coordinate(xDown + xEnd * xScale, yDown + yEnd * yScale),
                new Coordinate(xDown + xStart * xScale, yDown + yEnd * yScale),
                new Coordinate(xDown + xStart * xScale, yDown + yStart * yScale),
            })});

        return result;
    }

    // rectangleCreator принимает xStart, xEnd, yStart, yEnd
    private void ProcessInner(IEnumerable<LinearRing>?[,] grid,
        Func<int, int, int, int, IEnumerable<LinearRing>> rectangleCreator)
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

                int xSide = 0;
                int ySide = 0;
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

                IEnumerable<LinearRing> rectangle = rectangleCreator.Invoke(x, x + xSide + 1, y, y + ySide + 1);

                for (int i = 0; i <= xSide; i++)
                {
                    for (int j = 0; j <= ySide; j++)
                    {
                        grid[x + i, y + j] = rectangle;
                    }
                }

                y += ySide;
            }
        }
    }
}