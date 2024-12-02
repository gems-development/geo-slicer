using System;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Utils;
using GeoSlicer.Utils.PolygonClippingAlghorithm;
using NetTopologySuite.Geometries;

// ReSharper disable UseArrayEmptyMethod

namespace GeoSlicer.GridSlicer;

public class Slicer
{
    private readonly IEnumerable<Polygon> _outside = new Polygon[0];
    private readonly IEnumerable<Polygon> _inner = new Polygon[0];
    private readonly IEnumerable<Polygon> _inQueue = new Polygon[0];

    private readonly WeilerAthertonAlghorithm _helper;

    public Slicer(WeilerAthertonAlghorithm helper)
    {
        _helper = helper;
    }

    public IEnumerable<Polygon>?[,] Slice(
        Polygon inputPolygon,
        double xScale, double yScale,
        bool uniqueOnly = false)
    {
        // SetUp

        double xDown = Double.MaxValue;
        double yDown = Double.MaxValue;
        double xUp = Double.MinValue;
        double yUp = Double.MinValue;

        // todo Рассмотреть возможность избежания копирования
        foreach (Coordinate coordinate in inputPolygon.Coordinates)
        {
            xDown = Math.Min(xDown, coordinate.X);
            yDown = Math.Min(yDown, coordinate.Y);
            xUp = Math.Max(xUp, coordinate.X);
            yUp = Math.Max(yUp, coordinate.Y);
        }

        int xCount = (int)Math.Ceiling((xUp - xDown) / xScale);
        int yCount = (int)Math.Ceiling((yUp - yDown) / yScale);


        IEnumerable<Polygon>?[,] result = new IEnumerable<Polygon>?[xCount, yCount];

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

        xQueue.Enqueue(GetXIndex(inputPolygon.Coordinate));
        yQueue.Enqueue(GetYIndex(inputPolygon.Coordinate));


        // Console.WriteLine($"xCount: {xCount}, yCount: {yCount}");
        int total = xCount * yCount;
        int current = 0;

        // Начало итеративного алгоритма
        while (xQueue.Count > 0)
        {
            Console.WriteLine($"{current}/{total}");
            current++;
            int x = xQueue.Dequeue();
            int y = yQueue.Dequeue();
            IntersectionType intersectionType = _helper.WeilerAthertonForGrid(inputPolygon,
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
            (xStart, xEnd, yStart, yEnd) => new[]
            {
                new Polygon(new LinearRing(new []
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

                IEnumerable<Polygon> rectangle = rectangleCreator.Invoke(x, x + xSide + 1, y, y + ySide + 1);


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
}