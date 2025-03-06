using System;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils;

public class LineService
{
    private readonly double _epsilon;
    private readonly EpsilonCoordinateComparator _coordinateComparator;


    public LineService(double epsilon, EpsilonCoordinateComparator coordinateComparator)
    {
        _epsilon = epsilon;
        _coordinateComparator = coordinateComparator;
    }

    public static double VectorProduct(Coordinate firstVec, Coordinate secondVec)
    {
        return firstVec.X * secondVec.Y - secondVec.X * firstVec.Y;
    }

    public static double VectorProduct(Coordinate firstVecPoint1, Coordinate firstVecPoint2, Coordinate secondVecPoint1,
        Coordinate secondVecPoint2)
    {
        return (firstVecPoint2.X - firstVecPoint1.X) * (secondVecPoint2.Y - secondVecPoint1.Y) -
               (secondVecPoint2.X - secondVecPoint1.X) * (firstVecPoint2.Y - firstVecPoint1.Y);
    }

    public static double VectorProduct(Coordinate firstVecPoint1, Coordinate firstVecPoint2, Coordinate secondVecPoint1,
        double secondVecPoint2X, double secondVecPoint2Y)
    {
        return (firstVecPoint2.X - firstVecPoint1.X) * (secondVecPoint2Y - secondVecPoint1.Y) -
               (secondVecPoint2X - secondVecPoint1.X) * (firstVecPoint2.Y - firstVecPoint1.Y);
    }

    public static double VectorProduct(double firstVecX, double firstVecY, double secondVecX, double secondVecY)
    {
        return firstVecX * secondVecY - secondVecX * firstVecY;
    }

    public static void ToCanonical(Coordinate first, Coordinate second,
        out double a, out double b, out double c)
    {
        a = second.Y - first.Y;
        b = first.X - second.X;
        c = a * first.X + b * first.Y;
    }

    public bool IsCoordinateInSegment(Coordinate coordinate, Coordinate first, Coordinate second)
    {
        return IsCoordinateInSegmentBorders(coordinate, first, second) && IsCoordinateAtLine(coordinate, first, second);
    }

    public bool IsCoordinateAtLine(Coordinate coordinate, Coordinate first, Coordinate second)
    {
        return Math.Abs(VectorProduct(coordinate.X - first.X, coordinate.Y - first.Y,
            second.X - coordinate.X, second.Y - coordinate.Y)) < _epsilon;
    }


    public bool IsCoordinateInSegmentBorders(Coordinate coordinate, Coordinate first, Coordinate second)
    {
        return IsCoordinateInSegmentBorders(coordinate.X, coordinate.Y, first, second);
    }

    public bool IsCoordinateInSegmentBorders(double x, double y, Coordinate first, Coordinate second)
    {
        if (_coordinateComparator.IsEquals(first.X, second.X))
        {
            return y >= Math.Min(first.Y, second.Y) - _epsilon &&
                   y <= Math.Max(first.Y, second.Y) + _epsilon;
        }

        if (_coordinateComparator.IsEquals(first.Y, second.Y))
        {
            return x >= Math.Min(first.X, second.X) - _epsilon &&
                   x <= Math.Max(first.X, second.X) + _epsilon;
        }

        return y >= Math.Min(first.Y, second.Y) - _epsilon
               && y <= Math.Max(first.Y, second.Y) + _epsilon
               && x >= Math.Min(first.X, second.X) - _epsilon
               && x <= Math.Max(first.X, second.X) + _epsilon;
    }

    public bool IsCoordinateInIntervalBorders(Coordinate coordinate, Coordinate first, Coordinate second)
    {
        if (_coordinateComparator.IsEquals(coordinate, first) || _coordinateComparator.IsEquals(coordinate, second))
        {
            return false;
        }

        return IsCoordinateInSegmentBorders(coordinate.X, coordinate.Y, first, second);
    }


    public bool IsRectangleOnOneSideOfLine(Coordinate linePoint1, Coordinate linePoint2, Coordinate currentPoint1,
        Coordinate currentPoint2)
    {
        //Проверить, что все VectorProduct имеют один знак
        return VectorProduct(linePoint1, linePoint2, linePoint2, currentPoint1) < 0
               && VectorProduct(linePoint1, linePoint2, linePoint2, currentPoint2) < 0
               && VectorProduct(linePoint1, linePoint2, linePoint2, currentPoint1.X, currentPoint2.Y) < 0
               && VectorProduct(linePoint1, linePoint2, linePoint2, currentPoint2.X, currentPoint1.Y) < 0
               || VectorProduct(linePoint1, linePoint2, linePoint2, currentPoint1) > 0
               && VectorProduct(linePoint1, linePoint2, linePoint2, currentPoint2) > 0
               && VectorProduct(linePoint1, linePoint2, linePoint2, currentPoint1.X, currentPoint2.Y) > 0
               && VectorProduct(linePoint1, linePoint2, linePoint2, currentPoint2.X, currentPoint1.Y) > 0;
    }

    private double? CalculatePhiFromZeroTo2Pi(double x, double y)
    {
        return x switch
        {
            > 0 when y >= 0 => Math.Atan(y / x),
            > 0 when y < 0 => Math.Atan(y / x) + 2 * Math.PI,
            < 0 => Math.Atan(y / x) + Math.PI,
            0 when y > 0 => Math.PI / 2,
            0 when y < 0 => 3 * Math.PI / 2,
            0 when y == 0 => null,
            _ => null
        };
    }

    private double? CalculatePhiFromMinusPiToPlusPi(double x, double y)
    {
        return x switch
        {
            > 0 => Math.Atan(y / x),
            < 0 when y >= 0 => Math.Atan(y / x) + Math.PI,
            < 0 when y < 0 => Math.Atan(y / x) - Math.PI,
            0 when y > 0 => Math.PI / 2,
            0 when y < 0 => -Math.PI / 2,
            0 when y == 0 => null,
            _ => null
        };
    }

    // Возвращает четверть, в которой расположен вектор,
    // с учетом погрешности в пользу направления "по часовой стрелке"
    // Передаваемый вектор должен иметь длину на порядки больше, чем epsilon 
    public int GetQuadrant(double x, double y)
    {
        if (Math.Abs(x) < _epsilon)
        {
            // Так как длина больше epsilon, а X близок к 0, Y далек от нуля. Сравниваем без эпсилона
            return y > 0 ? 1 : 3;
        }
        
        if (Math.Abs(y) < _epsilon)
        {
            return x > 0 ? 4 : 2;
        }

        // X и Y далеки от 0
        if (x > 0)
        {
            return y > 0 ? 1 : 4;
        }

        return y > 0 ? 2 : 3;
    }

    public double ScalarProduct(double ax, double ay, double bx, double by)
    {
        return ax * bx + ay * by;
    }

    public bool InsideTheAngle(
        Coordinate vectorPointA1,
        Coordinate vectorPointA2,
        Coordinate anglePointB1,
        Coordinate anglePointB2,
        Coordinate anglePointB3)
    {

        return InsideTheAngle(vectorPointA1, vectorPointA2, anglePointB1, anglePointB2, anglePointB3, true);
    }

    public bool InsideTheAngleWithoutBorders(
        Coordinate vectorPointA1,
        Coordinate vectorPointA2,
        Coordinate anglePointB1,
        Coordinate anglePointB2,
        Coordinate anglePointB3)
    {
        return InsideTheAngle(vectorPointA1, vectorPointA2, anglePointB1, anglePointB2, anglePointB3, false);
    }

    // vectorPointA1 == anglePointB2
    private bool InsideTheAngle(
        Coordinate vectorPointA1,
        Coordinate vectorPointA2,
        Coordinate anglePointB1,
        Coordinate anglePointB2,
        Coordinate anglePointB3,
        bool isWithBorders)
    {
        double ax = vectorPointA2.X - vectorPointA1.X;
        double ay = vectorPointA2.Y - vectorPointA1.Y;
        // Нам не обязательно нормировать к 1 длине, достаточно исключить слишком короткие вектора
        double lenA = ax * ax + ay * ay;
        ax /= lenA;
        ay /= lenA;
        double b1X = anglePointB1.X - anglePointB2.X;
        double b1Y = anglePointB1.Y - anglePointB2.Y;
        double lenB1 = b1X * b1X + b1Y * b1Y;
        b1X /= lenB1;
        b1Y /= lenB1;
        double b3X = anglePointB3.X - anglePointB2.X;
        double b3Y = anglePointB3.Y - anglePointB2.Y;
        double len3 = b3X * b3X + b3Y * b3Y;
        b3X /= len3;
        b3Y /= len3;

        double vectorProductB3ToB1 = VectorProduct(b3X, b3Y, b1X, b1Y);
        double vectorProductB3ToA = VectorProduct(b3X, b3Y, ax, ay);
        double vectorProductAToB1 = VectorProduct(ax, ay, b1X, b1Y);
        double scalarProductAToB1 = ScalarProduct(ax, ay, b1X, b1Y);
        double scalarProductAToB3 = ScalarProduct(ax, ay, b3X, b3Y);


        if (Math.Abs(vectorProductB3ToB1) < _epsilon && ScalarProduct(b1X, b1Y, b3X, b3Y) > 0)
        {
            // Линии угла в 0 градусов

            if (vectorProductB3ToB1 > 0)
            {
                // Угол внутренний (очень маленький)

                if (!isWithBorders)
                {
                    // Либо за границей, либо на границе
                    return false;
                }

                // Возвращаем условия равенства вектора линиям угла
                return Math.Abs(vectorProductAToB1) < _epsilon && scalarProductAToB1 > 0;
            }

            // Угол внешний
            if (isWithBorders)
            {
                // Либо на границе, либо в границах
                return true;
            }

            // Возвращаем условия неравенства вектора линиям угла
            return Math.Abs(vectorProductAToB1) > _epsilon || scalarProductAToB1 < 0;
        }

        // Линии угла не близки

        if (Math.Abs(vectorProductAToB1) < _epsilon && scalarProductAToB1 > 0
            || Math.Abs(vectorProductB3ToA) < _epsilon && scalarProductAToB3 > 0)
        {
            // Вектор А близок к B1 или к B3
            return isWithBorders;
        }

        // Все 3 линии достаточно далеки.
        // Можем считать четверти в одном направлении, без "сжимания" и "убегания".


        int quadrantB1 = GetQuadrant(b1X, b1Y);
        int quadrantB3 = GetQuadrant(b3X, b3Y);
        int quadrantA = GetQuadrant(ax, ay);

        // Поворачиваем плоскость на 0||90||180||270 градусов, чтобы b3 был в 1ой четверти
        
        // quadrantB1 - 1 + 4 - quadrantB3 + 1 ||| (- 1 + 1)
        quadrantB1 = (quadrantB1 + 4 - quadrantB3) % 4 + 1;
        quadrantA = (quadrantA + 4 - quadrantB3) % 4 + 1;
        quadrantB3 = 1;

        if (quadrantB1 == quadrantB3)
        {
            if (quadrantA != quadrantB3)
            {
                // A не в той четверти, что B1 и B3, возвращаем условие наружности угла
                return vectorProductB3ToB1 < 0;
            }

            // Все 3 вектора в одной четверти.
            // Если угол внутренний, возвращаем условие нахождения вектора между линиями и наоборот
            if (vectorProductB3ToB1 > 0)
            {
                return vectorProductAToB1 > 0 && vectorProductB3ToA > 0;
            }
            return vectorProductAToB1 > 0 || vectorProductB3ToA > 0;
        }


        return
            // A против часовой от B3
            (quadrantA > quadrantB3 || quadrantA == quadrantB3 && vectorProductB3ToA > 0)
            &&
            // A по часовой от B1
            (quadrantA < quadrantB1 || quadrantA == quadrantB1 && vectorProductAToB1 > 0);
        // Так как мы повернули плоскость, а B1 и B3 в разных четвертях,
        // то можем сравнивать четверти просто на неравенство, без учета зацикливания
    }
}