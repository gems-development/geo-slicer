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


    public (IntersectionType, Coordinate?) GetSegmentIntersection(Line line1, Line line2)
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
            GetOuterIntersection(line1.a, line1.b, line2.a, line2.b);
        if (outerIntersection is not null)
        {
            return ((IntersectionType, Coordinate?))outerIntersection;
        }

        outerIntersection =
            GetOuterIntersection(line1.a, line1.b, line2.b, line2.a);
        if (outerIntersection is not null)
        {
            return ((IntersectionType, Coordinate?))outerIntersection;
        }

        // Получаем точку пересечения
        (double a, double b, double c) canonical1 = ToCanonical(line1);
        (double a, double b, double c) canonical2 = ToCanonical(line2);
        
        Coordinate? intersection = GetLineIntersection(canonical1, canonical2, _epsilon);

        if (intersection is null)
        {
            return (IntersectionType.NoIntersection, null);
        }

        // Проверяем, является ли пересечение пересечением по касательной
        if (_epsilonCoordinateComparator.IsEquals(intersection, line1.a))
        {
            return (IntersectionType.TangentIntersection, line1.a);
        }
        if (_epsilonCoordinateComparator.IsEquals(intersection, line1.b))
        {
            return (IntersectionType.TangentIntersection, line1.b);
        }
        if (_epsilonCoordinateComparator.IsEquals(intersection, line2.a))
        {
            return (IntersectionType.TangentIntersection, line2.a);
        }
        if (_epsilonCoordinateComparator.IsEquals(intersection, line2.b))
        {
            return (IntersectionType.TangentIntersection, line2.b);
        }

        // Проверяем, внутри ли отрезков лежит пересечение
        if (IsCoordinateInLine(intersection, line1) && IsCoordinateInLine(intersection, line2))
        {
            return (IntersectionType.InnerIntersection, intersection);
        }

        return (IntersectionType.NoIntersection, null);
    }
    
    // Проверяет, принадлежит ли точка отрезку
    private bool IsCoordinateInLine(Coordinate coordinate, Line line)
    {
        return coordinate.X > Math.Min(line.a.X, line.b.X) && coordinate.X < Math.Max(line.a.X, line.b.X);
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
    private static (double a, double b, double c) ToCanonical(Line line)
    {
        double a = line.b.Y - line.a.Y;
        double b = line.a.X - line.b.X;
        double c = a * line.a.X + b * line.a.Y;
        return (a, b, c);
    }
}