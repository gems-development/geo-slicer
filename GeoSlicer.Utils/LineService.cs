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

    /// <summary>
    /// Вычисляет Z-ординату результата векторного произведения
    /// </summary>
    public static double VectorProduct(Coordinate firstVec, Coordinate secondVec)
    {
        return firstVec.X * secondVec.Y - secondVec.X * firstVec.Y;
    }

    /// <summary>
    /// Вычисляет Z-ординату результата векторного произведения
    /// </summary>
    public static double VectorProduct(Coordinate firstVecPoint1, Coordinate firstVecPoint2, Coordinate secondVecPoint1,
        Coordinate secondVecPoint2)
    {
        return (firstVecPoint2.X - firstVecPoint1.X) * (secondVecPoint2.Y - secondVecPoint1.Y) -
               (secondVecPoint2.X - secondVecPoint1.X) * (firstVecPoint2.Y - firstVecPoint1.Y);
    }

    /// <summary>
    /// Вычисляет Z-ординату результата векторного произведения
    /// </summary>
    public static double VectorProduct(Coordinate firstVecPoint1, Coordinate firstVecPoint2, Coordinate secondVecPoint1,
        double secondVecPoint2X, double secondVecPoint2Y)
    {
        return (firstVecPoint2.X - firstVecPoint1.X) * (secondVecPoint2Y - secondVecPoint1.Y) -
               (secondVecPoint2X - secondVecPoint1.X) * (firstVecPoint2.Y - firstVecPoint1.Y);
    }

    /// <summary>
    /// Вычисляет Z-ординату результата векторного произведения
    /// </summary>
    public static double VectorProduct(double firstVecX, double firstVecY, double secondVecX, double secondVecY)
    {
        return firstVecX * secondVecY - secondVecX * firstVecY;
    }

    /// <summary>
    /// Преобразует прямую, заданную 2-мя точками в канонический вид, записывая результат в out переменные
    /// </summary>
    public static void ToCanonical(Coordinate first, Coordinate second,
        out double a, out double b, out double c)
    {
        a = second.Y - first.Y;
        b = first.X - second.X;
        c = a * first.X + b * first.Y;
    }

    /// <summary>
    /// Проверяет, находится ли точка на отрезке,
    /// заданном <paramref name="first"/> и <paramref name="second"/>
    /// </summary>
    public bool IsCoordinateInSegment(Coordinate coordinate, Coordinate first, Coordinate second)
    {
        return IsCoordinateInSegmentBorders(coordinate, first, second) && IsCoordinateAtLine(coordinate, first, second);
    }

    /// <summary>
    /// Проверяет, находится ли точка на прямой,
    /// заданной <paramref name="first"/> и <paramref name="second"/>
    /// </summary>
    public bool IsCoordinateAtLine(Coordinate coordinate, Coordinate first, Coordinate second)
    {
        return Math.Abs(VectorProduct(coordinate.X - first.X, coordinate.Y - first.Y,
            second.X - coordinate.X, second.Y - coordinate.Y)) < _epsilon;
    }

    /// <summary>
    /// Проверяет, находится ли точка внутри прямоугольной оболочки отрезка,
    /// заданного <paramref name="first"/> и <paramref name="second"/>
    /// </summary>
    public bool IsCoordinateInSegmentBorders(Coordinate coordinate, Coordinate first, Coordinate second)
    {
        return IsCoordinateInSegmentBorders(coordinate.X, coordinate.Y, first, second);
    }

    /// <summary>
    /// Проверяет, находится ли точка внутри прямоугольной оболочки отрезка,
    /// заданного <paramref name="first"/> и <paramref name="second"/>
    /// </summary>
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

    /// <summary>
    /// Проверяет, находится ли точка внутри прямоугольной оболочки отрезка,
    /// заданного <paramref name="first"/> и <paramref name="second"/>, при этом не равняясь этим точкам отрезка
    /// </summary>
    public bool IsCoordinateInIntervalBorders(Coordinate coordinate, Coordinate first, Coordinate second)
    {
        if (_coordinateComparator.IsEquals(coordinate, first) || _coordinateComparator.IsEquals(coordinate, second))
        {
            return false;
        }

        return IsCoordinateInSegmentBorders(coordinate.X, coordinate.Y, first, second);
    }

    /// <summary>
    /// Проверяет, находится ли прямоугольник, заданный <paramref name="currentPoint1"/> и <paramref name="currentPoint2"/>
    /// полностью по одну сторону от прямой, заданной <paramref name="linePoint1"/> и <paramref name="linePoint2"/>.
    /// Иначе говоря, проверяет, пересекает ли прямоугольник прямую
    /// </summary>
    public bool IsRectangleOnOneSideOfLine(
        Coordinate linePoint1,
        Coordinate linePoint2,
        Coordinate currentPoint1,
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


    /// <summary>
    /// Возвращает четверть, в которой расположен вектор,
    /// с учетом погрешности в пользу направления "по часовой стрелке".
    /// Передаваемый вектор должен иметь длину на порядки больше, чем epsilon (должен быть хоть сколь нибудь нормирован)
    /// </summary>
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

    /// <summary>
    /// Проверяет, находится ли вектор, заданный точками <paramref name="vectorPointA1"/> и <paramref name="vectorPointA2"/>
    /// внутри угла <paramref name="anglePointB1"/> <paramref name="anglePointB2"/> <paramref name="anglePointB3"/>
    /// </summary>
    public bool InsideTheAngle(
        Coordinate vectorPointA1,
        Coordinate vectorPointA2,
        Coordinate anglePointB1,
        Coordinate anglePointB2,
        Coordinate anglePointB3)
    {
        return InsideTheAngle(vectorPointA1, vectorPointA2, anglePointB1, anglePointB2, anglePointB3, true);
    }

    /// <summary>
    /// Проверяет, находится ли вектор, заданный точками <paramref name="vectorPointA1"/> и <paramref name="vectorPointA2"/>
    /// внутри угла <paramref name="anglePointB1"/> <paramref name="anglePointB2"/> <paramref name="anglePointB3"/>,
    /// исключая границы этого угла
    /// </summary>
    public bool InsideTheAngleWithoutBorders(
        Coordinate vectorPointA1,
        Coordinate vectorPointA2,
        Coordinate anglePointB1,
        Coordinate anglePointB2,
        Coordinate anglePointB3)
    {
        return InsideTheAngle(vectorPointA1, vectorPointA2, anglePointB1, anglePointB2, anglePointB3, false);
    }

    private bool IsEqualsRays(double vectorProduct, double scalarProduct)
    {
        return Math.Abs(vectorProduct) < _epsilon && scalarProduct > 0;
    }


    private bool InsideTheAngle(
        Coordinate vectorPointA1,
        Coordinate vectorPointA2,
        Coordinate anglePointB1,
        Coordinate anglePointB2,
        Coordinate anglePointB3,
        bool isWithBorders)
    {
        SetUp(
            out double ax, out double ay, 
            out double b1X, out double b1Y, out double b3X, out double b3Y,
            out double vectorProductB3ToB1, out double vectorProductB3ToA, out double vectorProductAToB1,
            out double scalarProductAToB1, out double scalarProductAToB3, out double scalarProductB1ToB3);


        if (IsEqualsRays(vectorProductB3ToB1, scalarProductB1ToB3))
        {
            return InsideTheZeroAngle();
        }

        // Линии угла не близки

        if (IsEqualsRays(vectorProductAToB1, scalarProductAToB1)
            || IsEqualsRays(vectorProductB3ToA, scalarProductAToB3))
        {
            // Вектор А близок к B1 или к B3
            return isWithBorders;
        }

        // Все 3 линии достаточно далеки.
        
        int quadrantB1 = GetQuadrant(b1X, b1Y);
        int quadrantB3 = GetQuadrant(b3X, b3Y);
        int quadrantA = GetQuadrant(ax, ay);
        
        quadrantB1 = (quadrantB1 + 4 - quadrantB3) % 4 + 1;
        quadrantA = (quadrantA + 4 - quadrantB3) % 4 + 1;
        quadrantB3 = 1;

        if (quadrantB1 == quadrantB3)
        {
            return InsideTheAngleInOneQuadrant();
        }


        return
            // A против часовой от B3
            (quadrantA > quadrantB3 || quadrantA == quadrantB3 && vectorProductB3ToA > 0)
            &&
            // A по часовой от B1
            (quadrantA < quadrantB1 || quadrantA == quadrantB1 && vectorProductAToB1 > 0);
        
        bool InsideTheZeroAngle()
        {
            if (vectorProductB3ToB1 > 0)
            {
                // Угол внутренний (очень маленький)
                return isWithBorders && IsEqualsRays(vectorProductAToB1, scalarProductAToB1);
            }

            // Угол внешний
            return isWithBorders || !IsEqualsRays(vectorProductAToB1, scalarProductAToB1);
        }
        
        void SetUp(out double axLocal, out double ayLocal,
            out double b1XLocal, out double b1YLocal, out double b3XLocal, out double b3YLocal,
            out double vectorProductB3ToB1Local, out double vectorProductB3ToALocal, out double vectorProductAToB1Local,
            out double scalarProductAToB1Local, out double scalarProductAToB3Local, out double scalarProductB1ToB3Local)
        {
            axLocal = vectorPointA2.X - vectorPointA1.X;
            ayLocal = vectorPointA2.Y - vectorPointA1.Y;
            double lenA = Math.Sqrt(axLocal * axLocal + ayLocal * ayLocal);
            axLocal /= lenA;
            ayLocal /= lenA;
            b1XLocal = anglePointB1.X - anglePointB2.X;
            b1YLocal = anglePointB1.Y - anglePointB2.Y;
            double lenB1 = Math.Sqrt(b1XLocal * b1XLocal + b1YLocal * b1YLocal);
            b1XLocal /= lenB1;
            b1YLocal /= lenB1;
            b3XLocal = anglePointB3.X - anglePointB2.X;
            b3YLocal = anglePointB3.Y - anglePointB2.Y;
            double len3 = Math.Sqrt(b3XLocal * b3XLocal + b3YLocal * b3YLocal);
            b3XLocal /= len3;
            b3YLocal /= len3;

            vectorProductB3ToB1Local = VectorProduct(b3XLocal, b3YLocal, b1XLocal, b1YLocal);
            vectorProductB3ToALocal = VectorProduct(b3XLocal, b3YLocal, axLocal, ayLocal);
            vectorProductAToB1Local = VectorProduct(axLocal, ayLocal, b1XLocal, b1YLocal);
            scalarProductAToB1Local = ScalarProduct(axLocal, ayLocal, b1XLocal, b1YLocal);
            scalarProductAToB3Local = ScalarProduct(axLocal, ayLocal, b3XLocal, b3YLocal);
            scalarProductB1ToB3Local = ScalarProduct(b1XLocal, b1YLocal, b3XLocal, b3YLocal);
        }

        bool InsideTheAngleInOneQuadrant()
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
    }


}