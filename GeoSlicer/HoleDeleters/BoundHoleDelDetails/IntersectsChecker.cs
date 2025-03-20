using System;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Utils.BoundRing;
using GeoSlicer.Utils.Intersectors;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails;

public class IntersectsChecker
{
    private readonly LinesIntersector _intersector;

    private readonly RobustLineIntersector _li = new();

    public IntersectsChecker(LinesIntersector intersector)
    {
        _intersector = intersector;
    }

    /// <summary>
    /// Проверяет, что прямая ab пересекает кольцо <paramref name="ring"/>, игнорируя особый случай пересечения.
    /// Особый случай пересечения - прямая ab касается точкой a или b кольца <paramref name="ring"/>
    /// в какой-нибудь его вершине.
    /// </summary>
    /// <returns>
    /// Null, если пересечения нет. Иначе кортеж (<paramref name="ring"/>, start), где start - начальная точка стороны
    /// <paramref name="ring"/>, которую пересекла прямая ab.
    /// </returns>
    public (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)?
        GetIntersectRingWithSegmentNotExtPoint(
            LinkedListNode<BoundingRing> ring,
            Coordinate a,
            Coordinate b)
    {
        var start = ring.Value.Ring;
        do
        {
            if (HasIntersectedSegmentsNotExternalPoints(start.Elem, start.Next.Elem, a, b))
                return (ring, start);
            start = start.Next;
        } while (!ReferenceEquals(start, ring.Value.Ring));

        return null;
    }

    /// <summary>
    /// Проверяет, что прямая ab пересекает кольцо <paramref name="boundRing"/>, игнорируя особый случай пересечения.
    /// Особый случай пересечения - прямая ab касается точкой a или b кольца <paramref name="boundRing"/>
    /// в какой-нибудь его вершине.
    /// </summary>
    public bool CheckIntersectsRingWithSegmentNotExtPoints(
        LinkedListNode<BoundingRing> boundRing,
        Coordinate a,
        Coordinate b)
    {
        LinkedNode<Coordinate> start = boundRing.Value.Ring;
        do
        {
            if (HasIntersectedSegmentsNotExternalPoints(start.Elem, start.Next.Elem, a, b))
                return true;
            start = start.Next;
        } while (!ReferenceEquals(start, boundRing.Value.Ring));

        return false;
    }

    /// <summary>
    /// Проверяет, что прямая ab пересекает кольцо <paramref name="boundRing"/> (касания считаются пересечением).
    /// </summary>
    public bool CheckIntersectsBoundRingWithLine(
        LinkedListNode<BoundingRing> boundRing,
        Coordinate a,
        Coordinate b)
    {
        if (!CheckLineIntersectsOrContainsInBoundRFrame(boundRing.Value, a, b))
            return false;

        LinkedNode<Coordinate> start = boundRing.Value.Ring;
        do
        {
            if (HasIntersectedSegments(start.Elem, start.Next.Elem, a, b))
                return true;
            start = start.Next;
        } while (!ReferenceEquals(start, boundRing.Value.Ring));

        return false;
    }

    /// <summary>
    /// Проверяет, пересекает ли отрезок ab рамку кольца <paramref name="ring"/>,
    /// или содержится ли отрезок в этой рамке.
    /// </summary>
    public bool CheckLineIntersectsOrContainsInBoundRFrame(BoundingRing ring, Coordinate a, Coordinate b)
    {
        return HasIntersectsBoundRFrame(ring, a, b)
               || CheckPointInsideBoundRFrame(a, ring)
               || CheckPointInsideBoundRFrame(b, ring);
    }

    /// <summary>
    /// Метод проверяет, пересекает ли отрезок ab рамку кольца <paramref name="ring"/>.
    /// </summary>
    public bool HasIntersectsBoundRFrame(BoundingRing ring, Coordinate a, Coordinate b)
    {
        return
            HasIntersectedSegments(a, b, ring.PointMin, new Coordinate(ring.PointMin.X, ring.PointMax.Y)) ||
            HasIntersectedSegments(a, b, new Coordinate(ring.PointMin.X, ring.PointMax.Y), ring.PointMax) ||
            HasIntersectedSegments(a, b, ring.PointMax, new Coordinate(ring.PointMax.X, ring.PointMin.Y)) ||
            HasIntersectedSegments(a, b, new Coordinate(ring.PointMax.X, ring.PointMin.Y), ring.PointMin);
    }

    public bool HasIntersectedSegments(Coordinate a1, Coordinate b1, Coordinate a2, Coordinate b2)
    {
        _li.ComputeIntersection(a1, b1, a2, b2);
        return _li.HasIntersection;
    }

    public bool HasIntersectedSegmentsNotExternalPoints
        (Coordinate a1, Coordinate b1, Coordinate a2, Coordinate b2)
    {
        return !_intersector.CheckIntersection(
            LinesIntersectionType.NoIntersection |
            LinesIntersectionType.Corner |
            LinesIntersectionType.Extension |
            LinesIntersectionType.Outside,
            a1, b1, a2, b2);
    }

    /// <summary>
    /// Ищет координаты пересечения рамок колец <paramref name="ring1"/> и <paramref name="ring2"/> и записывает
    /// их в <paramref name="framesIntersectionPointMin"/> и <paramref name="framesIntersectionPointMax"/>.
    /// </summary>
    public void GetIntersectionBoundRFrames
    (BoundingRing ring1, BoundingRing ring2,
        out Coordinate framesIntersectionPointMin, out Coordinate framesIntersectionPointMax)
    {
        framesIntersectionPointMin = new Coordinate(
            Math.Max(ring1.PointMin.X, ring2.PointMin.X),
            Math.Max(ring1.PointMin.Y, ring2.PointMin.Y));
        framesIntersectionPointMax = new Coordinate(
            Math.Min(ring1.PointMax.X, ring2.PointMax.X),
            Math.Min(ring1.PointMax.Y, ring2.PointMax.Y));
    }

    /// <summary>
    /// Проверяет, не пересекает ли рамка кольца <paramref name="boundingRing"/> рамку
    /// кольца <paramref name="relativeBoundRing"/>
    /// Если рамка касается другой рамки(или близка к ней с учетом <paramref name="epsilon"/>), то пересечение есть.
    /// </summary>
    public bool CheckNotIntersects(BoundingRing relativeBoundRing, BoundingRing boundingRing, double epsilon)
    {
        return (boundingRing.PointMin.Y >= relativeBoundRing.PointMax.Y &&
                !CompareEquality(boundingRing.PointMin.Y, relativeBoundRing.PointMax.Y, epsilon))
               ||
               (boundingRing.PointMax.X <= relativeBoundRing.PointMin.X &&
                !CompareEquality(relativeBoundRing.PointMin.X, boundingRing.PointMax.X, epsilon))
               ||
               (boundingRing.PointMax.Y <= relativeBoundRing.PointMin.Y &&
                !CompareEquality(relativeBoundRing.PointMin.Y, boundingRing.PointMax.Y, epsilon))
               ||
               (boundingRing.PointMin.X >= relativeBoundRing.PointMax.X &&
                !CompareEquality(boundingRing.PointMin.X, relativeBoundRing.PointMax.X, epsilon));
    }

    /// <summary>
    /// Проверяет, лежит ли хотя бы одно значение из массива <paramref name="arr"/> в отрезке ab
    /// </summary>
    public bool CheckSegmentContainsAtLeastOneNumber(double a, double b, double[] arr)
    {
        return arr.Any(elem => elem >= a && elem <= b);
    }

    /// <summary>
    /// Проверяет, лежит ли точка <paramref name="point"/> внутри рамки кольца <paramref name="ring"/>.
    /// </summary>
    /// <returns>Если лежит - true. False если точка лежит либо на какой-нибудь стороне рамки, либо вне рамки.</returns>
    private bool CheckPointInsideBoundRFrame(Coordinate point, BoundingRing ring)
    {
        return CheckPointInsideFrameCheck(point, ring.PointMin, ring.PointMax);
    }
    
    /// <summary>
    /// Проверяет, лежит ли точка <paramref name="point"/> внутри рамки с
    /// координатами <paramref name="pointMin"/>, <paramref name="pointMax"/>.
    /// </summary>
    /// <returns>Если лежит - true. False если точка лежит либо на какой-нибудь стороне рамки, либо вне рамки.</returns>
    public bool CheckPointInsideFrameCheck(Coordinate point, Coordinate pointMin, Coordinate pointMax)
    {
        return point.X < pointMax.X && point.X > pointMin.X && point.Y < pointMax.Y && point.Y > pointMin.Y;
    }

    public bool CompareEquality(double a, double b, double epsilon)
    {
        double difference = a - b;
        return difference >= 0 && difference < epsilon;
    }
}