using System;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Utils;
using NetTopologySuite.Geometries;

// ReSharper disable UseArrayEmptyMethod

namespace GeoSlicer.GridSlicer;

public class Slicer
{
    private readonly IEnumerable<LinearRing> _outside = new LinearRing[0];
    private readonly IEnumerable<LinearRing> _fullIntersects = new LinearRing[0];
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
        
        // Добавляет индексы в очереди, если клетка не за пределами сетки и она еще не была назначена на проверку
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

            // Заполнить текущую клетку
            switch (intersectionType)
            {
                case IntersectionType.BoxOutsideGeometry:
                    result[x, y] = _outside;
                    break;
                case IntersectionType.IntersectionWithEdge:
                    result[x, y] = weilerResult;
                    break;
                case IntersectionType.BoxInGeometry:
                    result[x, y] = _fullIntersects;
                    break;
                case IntersectionType.GeometryInBox:
                    result[x, y] = new[] { inputRing };
                    break;
            }

            // Добавить нужные окружающие клетки в очередь
            if (intersectionType == IntersectionType.IntersectionWithEdge ||
                intersectionType == IntersectionType.BoxInGeometry)
            {
                CheckAndAdd(x - 1, y);
                CheckAndAdd(x + 1, y);
                CheckAndAdd(x, y - 1);
                CheckAndAdd(x, y + 1);
            }
        }

        // todo Добавить метод на внутреннюю область
        for (int x = 0; x < xCount; x++)
        {
            for (int y = 0; y < yCount; y++)
            {
                if (ReferenceEquals(result[x, y], _fullIntersects))
                {
                    result[x, y] = new LinearRing[]
                    {
                        new LinearRing(new[]
                        {
                            new Coordinate(xDown + x * xScale, yDown + y * yScale),
                            new Coordinate(xDown + x * xScale, yDown + (y + 1) * yScale),
                            new Coordinate(xDown + (x + 1) * xScale, yDown + (y + 1) * yScale),
                            new Coordinate(xDown + x * xScale, yDown + y * yScale)
                        }),
                        new LinearRing(new[]
                        {
                            new Coordinate(xDown + x * xScale, yDown + y * yScale),
                            new Coordinate(xDown + (x + 1) * xScale, yDown + (y + 1) * yScale),
                            new Coordinate(xDown + (x + 1) * xScale, yDown + y * yScale),
                            new Coordinate(xDown + x * xScale, yDown + y * yScale)
                        })
                    };
                }
            }
        }
        return result;
    }
}