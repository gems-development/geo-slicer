using System;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils;

public static class Generators
{
    private static readonly GeometryFactory Gf =
        NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);

    /// <summary>
    /// Создает выпуклый многоугольник
    /// </summary>
    /// <param name="pointCount">Количество точек, должно быть кратно 4-м</param>
    /// <param name="gf">Генератор геометрии. Если не передать, используется вариант по умолчанию</param>
    /// <returns>Созданный многоугольник</returns>
    /// <exception cref="ArgumentException">Если требуемое число точек не кратно 4-м</exception>
    public static LinearRing GenerateConvexLinearRing(int pointCount, GeometryFactory? gf = null)
    {
        gf ??= Gf;

        if (pointCount % 4 != 0)
        {
            throw new ArgumentException("Количество точек должно быть кратно 4-м");
        }

        Coordinate[] coordinates = new Coordinate[pointCount + 1];
        double x = 0;
        double y = 0;
        double dx = 0;
        double dy = (double)pointCount / 4;
        int index = 0;

        coordinates[index] = new Coordinate(x, y);
        index++;

        while (dy > 0)
        {
            x += dx;
            y += dy;
            dy--;
            dx++;
            coordinates[index] = new Coordinate(x, y);
            index++;
        }

        while (dx > 0)
        {
            x += dx;
            y += dy;
            dy--;
            dx--;
            coordinates[index] = new Coordinate(x, y);
            index++;
        }

        while (dy < 0)
        {
            x += dx;
            y += dy;
            dy++;
            dx--;
            coordinates[index] = new Coordinate(x, y);
            index++;
        }

        while (dx < 0)
        {
            x += dx;
            y += dy;
            dy++;
            dx++;
            coordinates[index] = new Coordinate(x, y);
            index++;
        }

        LinearRing result = gf.CreateLinearRing(coordinates);
        return result;
    }
}