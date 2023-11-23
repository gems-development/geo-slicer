using System;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.Intersectors;

using CanonicalLine = Tuple<double, double, double>;

public class Intersector

{
    public enum IntersectionType
    {
        NoIntersection,
        Equals,
        EndsIntersection,
        TangentIntersection,
        InnerIntersection
    }

    private readonly double _epsilon;

    private readonly ICoordinateComparator _coordinateComparator;
    private readonly EpsilonCoordinateComparator _epsilonCoordinateComparator;

    public Intersector(ICoordinateComparator coordinateComparator, double epsilon = 1E-5)
    {
        _coordinateComparator = coordinateComparator;
        _epsilonCoordinateComparator = new EpsilonCoordinateComparator(epsilon);
        _epsilon = epsilon;
    }


    // todo добавить док о том, что в случае нахождения одной прямой внутри другой возвращает null
    public (IntersectionType intersectionType, Coordinate? intersectionCoordinate) GetSegmentIntersection(Line line1, Line line2)
    {
        return GetSegmentIntersection(line1.A, line1.B, line2.A, line2.B);
    }

    public (IntersectionType intersectionType, Coordinate? intersectionCoordinate) GetSegmentIntersection(
        Coordinate line1First, Coordinate line1Second,
        Coordinate line2First, Coordinate line2Second)
    {
        // Проверяет, равны ли отрезки, или имеют ли общее начало или конец
        (IntersectionType, Coordinate?)? GetOuterIntersection(
            Coordinate a1, Coordinate a2,
            Coordinate b1, Coordinate b2)
        {
            if (_coordinateComparator.IsEquals(a1, b1))
            {
                if (_coordinateComparator.IsEquals(a2, b2))
                {
                    return (IntersectionType.Equals, null);
                }

                return (IntersectionType.EndsIntersection, a1);
            }

            if (_coordinateComparator.IsEquals(a2, b2))
            {
                return (IntersectionType.EndsIntersection, a2);
            }

            return null;
        }


        // Проверяем методом выше отрезки a1b1 с a2b2 и a1b1 с b2a2
        (IntersectionType, Coordinate?)? outerIntersection =
            GetOuterIntersection(line1First, line1Second, line2First, line2Second);
        if (outerIntersection is not null)
        {
            return ((IntersectionType, Coordinate?))outerIntersection;
        }

        outerIntersection =
            GetOuterIntersection(line1First, line1Second, line2Second, line2First);
        if (outerIntersection is not null)
        {
            return ((IntersectionType, Coordinate?))outerIntersection;
        }

        // Получаем точку пересечения
        (double a, double b, double c) canonical1 = ToCanonical(line1First, line1Second);
        (double a, double b, double c) canonical2 = ToCanonical(line2First, line2Second);

        Coordinate? intersection = GetLineIntersection(canonical1, canonical2, _epsilon);

        if (intersection is null)
        {
            return (IntersectionType.NoIntersection, null);
        }

        // Проверяем, является ли пересечение пересечением по касательной
        if (_epsilonCoordinateComparator.IsEquals(intersection, line1First))
        {
            return (IntersectionType.TangentIntersection, line1First);
        }

        if (_epsilonCoordinateComparator.IsEquals(intersection, line1Second))
        {
            return (IntersectionType.TangentIntersection, line1Second);
        }

        if (_epsilonCoordinateComparator.IsEquals(intersection, line2First))
        {
            return (IntersectionType.TangentIntersection, line2First);
        }

        if (_epsilonCoordinateComparator.IsEquals(intersection, line2Second))
        {
            return (IntersectionType.TangentIntersection, line2Second);
        }

        // Проверяем, внутри ли отрезков лежит пересечение
        if (IsCoordinateInSegmentBorders(intersection, line1First, line1Second) &&
            IsCoordinateInSegmentBorders(intersection, line2First, line2Second))
        {
            return (IntersectionType.InnerIntersection, intersection);
        }

        return (IntersectionType.NoIntersection, null);
    }

    // Проверяет, принадлежит ли точка отрезку. Принимает точку, лежащую на прямой отрезка
    private bool IsCoordinateInSegmentBorders(Coordinate coordinate, Coordinate first, Coordinate second)
    {
        if (Math.Abs(first.X - second.X) <= _epsilon)
        {
            return coordinate.Y > Math.Min(first.Y, second.Y) && coordinate.Y < Math.Max(first.Y, second.Y);
        }
        return coordinate.X > Math.Min(first.X, second.X) && coordinate.X < Math.Max(first.X, second.X);
    }

    // Получить пересечение прямых
    private static Coordinate? GetLineIntersection(
        (double a, double b, double c) line1,
        (double a, double b, double c) line2,
        double epsilon)
    {
        double delta = line1.a * line2.b - line2.a * line1.b;

        if (Math.Abs(delta) <= epsilon)
        {
            return null;
        }

        double x = (line2.b * line1.c - line1.b * line2.c) / delta;
        double y = (line1.a * line2.c - line2.a * line1.c) / delta;
        return new Coordinate(x, y);
    }

    // Преобразовать в канонический вид
    private static (double a, double b, double c) ToCanonical(Coordinate first, Coordinate second)
    {
        double a = second.Y - first.Y;
        double b = first.X - second.X;
        double c = a * first.X + b * first.Y;
        return (a, b, c);
    }
}