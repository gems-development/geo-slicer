using System;
using System.Collections.Generic;
using GeoSlicer.Utils.BoundRing;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails;

public static class BoundRIntersectsChecker
{
    public static
        (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)?
        CheckIntersectRingWithSegmentNotExtPoint(
            LinkedListNode<BoundingRing> ring, Coordinate a, Coordinate b)
    {
        var start = ring.Value.Ring;
        do
        {
            if (HasIntersectedSegments(start.Elem, start.Next.Elem, a, b)
                && !ReferenceEquals(a, start.Elem)
                && !ReferenceEquals(b, start.Elem)
                && !ReferenceEquals(a, start.Next.Elem)
                && !ReferenceEquals(b, start.Next.Elem))
                return (ring, start);
            start = start.Next;
        } while (!ReferenceEquals(start, ring.Value.Ring));

        return null;
    }


    public static bool IntersectBoundRingNotExtPoints(
        LinkedListNode<BoundingRing> boundRing, 
        Coordinate a,
        Coordinate b)
    {
        LinkedNode<Coordinate> start = boundRing.Value.Ring;
        do
        {
            if (HasIntersectedSegments(start.Elem, start.Next.Elem, a, b)
                && !ReferenceEquals(a, start.Elem)
                && !ReferenceEquals(b, start.Elem)
                && !ReferenceEquals(a, start.Next.Elem)
                && !ReferenceEquals(b, start.Next.Elem))
                return true;
            start = start.Next;
        } while (!ReferenceEquals(start, boundRing.Value.Ring));

        return false;
    }
    
    
    public static bool IntersectBoundRing(
        LinkedListNode<BoundingRing> boundRing, 
        Coordinate a,
        Coordinate b)
    {
        LinkedNode<Coordinate> start = boundRing.Value.Ring;
        do
        {
            if (HasIntersectedSegments(start.Elem, start.Next.Elem, a, b))
                return true;
            start = start.Next;
        } while (!ReferenceEquals(start, boundRing.Value.Ring));

        return false;
    }
    
    
    //todo возможно улучшение
    public static bool HasIntersectsBoundRFrame(BoundingRing ring, Coordinate a, Coordinate b)
    {
        LineSegment AB = new LineSegment(a, b);
        LineSegment[] sides = new LineSegment [4];
        sides[0] = new LineSegment(ring.PointMin, new Coordinate(ring.PointMin.X, ring.PointMax.Y));
        sides[1] = new LineSegment(new Coordinate(ring.PointMin.X, ring.PointMax.Y), ring.PointMax);
        sides[2] = new LineSegment(ring.PointMax, new Coordinate(ring.PointMax.X, ring.PointMin.Y));
        sides[3] = new LineSegment(new Coordinate(ring.PointMax.X, ring.PointMin.Y), ring.PointMin);
        foreach (var side in sides)
        {
            if (side.Intersection(AB) is not null)
                return true;
        }

        return false;
    }
    
    
    //todo нужно улучшить
    private static RobustLineIntersector _li = new RobustLineIntersector();
    public static bool HasIntersectedSegments(Coordinate a1, Coordinate b1, Coordinate a2, Coordinate b2)
    {
        //return new LineSegment(a1, b1).Intersection(new LineSegment(a2, b2)) is not null;
        _li.ComputeIntersection(a1, b1, a2, b2);
        return _li.HasIntersection;
        //return li.GetIntersection(0);
    }
    public static bool HasIntersectedSegmentsNotExternalPoints(Coordinate a1, Coordinate b1, Coordinate a2, Coordinate b2)
    {
        //return new LineSegment(a1, b1).Intersection(new LineSegment(a2, b2)) is not null;
        if (ReferenceEquals(a1, a2) ||
            ReferenceEquals(a1, b2) ||
            ReferenceEquals(b1, a2) ||
            ReferenceEquals(b1, b2))
            return false;
        _li.ComputeIntersection(a1, b1, a2, b2);
        return _li.HasIntersection;
        //return li.GetIntersection(0);
    }

    
    //возращает false если одна рамка содержится в другой
    //true в противном случае(могут пересекаться и не пересекаться)
    public static bool IntersectOrContainFramesCheck(
        BoundingRing ring1,
        BoundingRing ring2)
    {
        Coordinate pointMin = new Coordinate(
            Math.Min(ring1.PointMin.X, ring2.PointMin.X),
            Math.Min(ring1.PointMin.Y, ring2.PointMin.Y));
        Coordinate pointMax = new Coordinate(
            Math.Max(ring1.PointMax.X, ring2.PointMax.X),
            Math.Max(ring1.PointMax.Y, ring2.PointMax.Y));
        if ((pointMin.Equals(ring1.PointMin) && pointMax.Equals(ring1.PointMax)) || 
            (pointMin.Equals(ring2.PointMin) && pointMax.Equals(ring2.PointMax)))
        {
            return false;
        }
        return true;
    }
    
    //todo Переделать под out переменные
    public static (Coordinate pointMin, Coordinate pointMax) GetIntersectionBoundRFrames(BoundingRing ring1, BoundingRing ring2)
    {
        Coordinate pointMin = new Coordinate(
            Math.Max(ring1.PointMin.X, ring2.PointMin.X),
            Math.Max(ring1.PointMin.Y, ring2.PointMin.Y));
        Coordinate pointMax = new Coordinate(
            Math.Min(ring1.PointMax.X, ring2.PointMax.X),
            Math.Min(ring1.PointMax.Y, ring2.PointMax.Y));
        return (pointMin, pointMax);
    }
    
    //todo Есть аналогичный метод в Intersector.AreasIntersector
    public static bool NotIntersectCheck(BoundingRing relativeBoundRing, BoundingRing boundingRing)
    {
        return boundingRing.PointMin.Y > relativeBoundRing.PointMax.Y
               || Math.Abs(boundingRing.PointMin.Y - relativeBoundRing.PointMax.Y) < 1e-9 ||


               boundingRing.PointMax.X < relativeBoundRing.PointMin.X
               || Math.Abs(boundingRing.PointMax.X - relativeBoundRing.PointMin.X) < 1e-9 ||

               boundingRing.PointMax.Y < relativeBoundRing.PointMin.Y
               || Math.Abs(boundingRing.PointMax.Y - relativeBoundRing.PointMin.Y) < 1e-9 ||


               boundingRing.PointMin.X > relativeBoundRing.PointMax.X
               || Math.Abs(boundingRing.PointMin.X - relativeBoundRing.PointMax.X) < 1e-9;
    }
    
    
    public static bool SegmentContainAtLeastOneNumber(double a, double b, double[] arr)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            if ((arr[i] > a && arr[i] < b) || Math.Abs(a - arr[i]) < 1e-9 || Math.Abs(b - arr[i]) < 1e-9)
                return true;
        }
        
        return false;
    }
    
    
    public static bool PointInsideBoundRFrame(Coordinate point, BoundingRing ring)
    {
        var pointMin = ring.PointMin;
        var pointMax = ring.PointMax;
        return point.X < pointMax.X && point.X > pointMin.X && point.Y < pointMax.Y && point.Y > pointMin.Y;
    }
    
    
    public static bool PointInsideFrameCheck(Coordinate point, (Coordinate pointMin, Coordinate pointMax) frame)
    {
        return point.X < frame.pointMax.X && point.X > frame.pointMin.X && point.Y < frame.pointMax.Y && point.Y > frame.pointMin.Y;
    }
    
    
}