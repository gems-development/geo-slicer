﻿using System;
using System.Collections.Generic;
using GeoSlicer.Utils.BoundRing;
using GeoSlicer.Utils.Intersectors;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails;

public class IntersectsChecker
{
    private LinesIntersector _intersector;

    public IntersectsChecker(LinesIntersector intersector)
    {
        _intersector = intersector;
    }

    //Проверяет, что прямая ab пересекает кольцо ring, игнорируя особый случай пересечения.
    //Особый случай пересечения - прямая ab касается точкой a или b кольца ring в какой-нибудь его вершине.
    //В случае пересечения возвращается кортеж (ring, start), где start - начальная точка стороны
    //ring, которую пересекла прямая ab.
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

    //Проверяет, что прямая ab пересекает кольцо ring, игнорируя особый случай пересечения.
    //Особый случай пересечения - прямая ab касается точкой a или b кольца ring в какой-нибудь его вершине.
    //В случае пересечения возвращает true, false - иначе.
    public bool IntersectRingWithSegmentNotExtPoints(
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

    //Проверяет, что прямая ab пересекает кольцо ring(касания считаются пересечением).
    //В случае пересечения возвращает true, false - иначе.
    public bool IntersectBoundRingWithLine(
        LinkedListNode<BoundingRing> boundRing,
        Coordinate a,
        Coordinate b)
    {
        if (!LineIntersectsOrContainsInBoundRFrame(boundRing.Value, a, b))
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

    //Проверяет, пересекает ли отрезок ab рамку кольца ring, или содержится ли отрезок в этой рамке.
    public bool LineIntersectsOrContainsInBoundRFrame(BoundingRing ring, Coordinate a, Coordinate b)
    {
        return HasIntersectsBoundRFrame(ring, a, b)
               || PointInsideBoundRFrame(a, ring)
               || PointInsideBoundRFrame(b, ring);
    }

    //Метод проверяет, пересекает ли отрезок ab рамку кольца ring.
    //True - если пересекает, false - иначе.
    public bool HasIntersectsBoundRFrame(BoundingRing ring, Coordinate a, Coordinate b)
    {
        return
            HasIntersectedSegments(a, b, ring.PointMin, new Coordinate(ring.PointMin.X, ring.PointMax.Y)) ||
            HasIntersectedSegments(a, b, new Coordinate(ring.PointMin.X, ring.PointMax.Y), ring.PointMax) ||
            HasIntersectedSegments(a, b, ring.PointMax, new Coordinate(ring.PointMax.X, ring.PointMin.Y)) ||
            HasIntersectedSegments(a, b, new Coordinate(ring.PointMax.X, ring.PointMin.Y), ring.PointMin);
    }

    private RobustLineIntersector _li = new();

    public bool HasIntersectedSegments(Coordinate a1, Coordinate b1, Coordinate a2, Coordinate b2)
    {
        _li.ComputeIntersection(a1, b1, a2, b2);
        return _li.HasIntersection;
    }

    public bool HasIntersectedSegmentsNotExternalPoints
        (Coordinate a1, Coordinate b1, Coordinate a2, Coordinate b2)
    {
        return !_intersector.CheckIntersection(
            LinesIntersectionType.NoIntersection|
            LinesIntersectionType.Corner|
            LinesIntersectionType.Extension|
            LinesIntersectionType.Outside,
            a1, b1, a2, b2);
    }


    //возращает false если одна рамка содержится в другой
    //true в противном случае(могут пересекаться и не пересекаться)
    public bool IntersectOrContainFrames(BoundingRing ring1, BoundingRing ring2)
    {
        Coordinate pointMin = new Coordinate(
            Math.Min(ring1.PointMin.X, ring2.PointMin.X),
            Math.Min(ring1.PointMin.Y, ring2.PointMin.Y));
        Coordinate pointMax = new Coordinate(
            Math.Max(ring1.PointMax.X, ring2.PointMax.X),
            Math.Max(ring1.PointMax.Y, ring2.PointMax.Y));
        return !((pointMin.Equals(ring1.PointMin) && pointMax.Equals(ring1.PointMax)) ||
                 (pointMin.Equals(ring2.PointMin) && pointMax.Equals(ring2.PointMax)));
    }

    //ищет координаты пересечения рамок колец ring1 и ring2 и записывает их в framesIntersectionPointMin
    //и framesIntersectionPointMax.
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

    //Проверяет, не пересекает ли рамка кольца boundingRing рамку кольца relativeBoundRing
    //Если рамка касается другой рамки(или близка к ней с учетом epsilon), то пересечение есть.
    public bool NotIntersect(BoundingRing relativeBoundRing, BoundingRing boundingRing, double epsilon)
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

    //Проверяет, лежит ли хотя бы одно значение из массива arr в отрезке ab
    public bool SegmentContainAtLeastOneNumber(double a, double b, double[] arr)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] >= a && arr[i] <= b)
                return true;
        }

        return false;
    }

    //Проверяет, лежит ли точка point внутри рамки кольца ring.
    //Если лежит - возвращает true. Возвращает false если точка лежит либо на какой-нибудь стороне рамки, либо вне рамки.
    public bool PointInsideBoundRFrame(Coordinate point, BoundingRing ring)
    {
        return PointInsideFrameCheck(point, ring.PointMin, ring.PointMax);
    }

    //Проверяет, лежит ли точка point внутри рамки с координатами pointMin, pointMax.
    //Если лежит - возвращает true. Возвращает false если точка лежит либо на какой-нибудь стороне рамки, либо вне рамки.
    public bool PointInsideFrameCheck(Coordinate point, Coordinate pointMin, Coordinate pointMax)
    {
        return point.X < pointMax.X && point.X > pointMin.X && point.Y < pointMax.Y && point.Y > pointMin.Y;
    }

    public bool CompareEquality(double a, double b, double epsilon)
    {
        double difference = a - b;
        return difference >= 0 && difference < epsilon;
    }
}