using System;
using System.Collections.Generic;
using GeoSlicer.Utils.BoundRing;
using NetTopologySuite.Geometries;
using GeoSlicer.HoleDeleters.BoundHoleDelDetails.Structures;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails;
//Кэш алгоритма BoundingHoleDeleter.
//Например в списке ListA хранятся рамки, расположенные севернее рамки,
//относительно которой проводилось заполнение кэша. Соотвествие букв и сторон света описано в PartitioningZones.
internal class Cache
{
    private readonly double _epsilon;

    internal readonly IReadOnlyDictionary<Zones, LinkedList<RingAndZones>> RingsInZone;
    
    internal readonly LinkedList<LinkedListNode<BoundingRing>>IntersectFrames = new();
    internal readonly LinkedList<LinkedListNode<BoundingRing>> FramesContainThis = new();

    internal readonly Dictionary<Zones, RingAndZones> NearRing = new();
    internal readonly Dictionary<Zones, RingAndPoint> NearSegmentIntersect = new();

    internal Cache(double epsilon)
    {
        _epsilon = epsilon;
        var thisRingsInZone = new Dictionary<Zones, LinkedList<RingAndZones>>();
        thisRingsInZone.Add(Zones.A, new LinkedList<RingAndZones>());
        thisRingsInZone.Add(Zones.B, new LinkedList<RingAndZones>());
        thisRingsInZone.Add(Zones.C, new LinkedList<RingAndZones>());
        thisRingsInZone.Add(Zones.D, new LinkedList<RingAndZones>());
        thisRingsInZone.Add(Zones.E, new LinkedList<RingAndZones>());
        thisRingsInZone.Add(Zones.F, new LinkedList<RingAndZones>());
        thisRingsInZone.Add(Zones.G, new LinkedList<RingAndZones>());
        thisRingsInZone.Add(Zones.H, new LinkedList<RingAndZones>());
        RingsInZone = thisRingsInZone;
    }

    private void Clear()
    {
        foreach (var rings in RingsInZone.Values)
        {
            rings.Clear();
        }

        IntersectFrames.Clear();
        FramesContainThis.Clear();
        NearRing.Clear();
        NearSegmentIntersect.Clear();
    }

    //Метод вычисляет местоположение рамок из boundRings относительно boundRing и помещает информацию в кэш.
    internal bool FillListsRelativeRing(
        LinkedListNode<BoundingRing> boundRing,
        LinkedList<BoundingRing> boundRings)
    {
        Clear();
        bool hasIntersectFrames = false;
        LinkedListNode<BoundingRing>? thisRing = boundRings.First;
        while (thisRing is not null)
        {
            if (!ReferenceEquals(thisRing, boundRing))
            {
                if (!hasIntersectFrames)
                {
                    if (!DetectSeparatingZone(boundRing, thisRing))
                    {
                        hasIntersectFrames = IntersectOrContainFrames(boundRing, thisRing);
                        if (hasIntersectFrames)
                            IntersectFrames.AddFirst(thisRing);
                    }
                }
                else if (IntersectsChecker.NotIntersect(boundRing.Value, thisRing.Value, _epsilon))
                {
                    if (IntersectOrContainFrames(boundRing, thisRing))
                        DetectSeparatingZone(boundRing, thisRing);
                }
                else
                {
                    IntersectFrames.AddFirst(thisRing);
                }
            }

            thisRing = thisRing.Next;
        }
        return hasIntersectFrames;
    }

    //возращает false если рамка boundRing содержится в рамке relativeBoundRing
    //или рамка relativeBoundRing содержится в boundRing.
    //true в противном случае(могут пересекаться и не пересекаться)
    private bool IntersectOrContainFrames(
        LinkedListNode<BoundingRing> relativeBoundRing,
        LinkedListNode<BoundingRing> boundRing)
    {
        Coordinate pointMin = new Coordinate(
            Math.Min(relativeBoundRing.Value.PointMin.X, boundRing.Value.PointMin.X),
            Math.Min(relativeBoundRing.Value.PointMin.Y, boundRing.Value.PointMin.Y));
        Coordinate pointMax = new Coordinate(
            Math.Max(relativeBoundRing.Value.PointMax.X, boundRing.Value.PointMax.X),
            Math.Max(relativeBoundRing.Value.PointMax.Y, boundRing.Value.PointMax.Y));
        if (pointMin.Equals(relativeBoundRing.Value.PointMin) &&
            pointMax.Equals(relativeBoundRing.Value.PointMax))
        {
            IntersectFrames.AddFirst(boundRing);
            return false;
        }

        if (pointMin.Equals(boundRing.Value.PointMin) && pointMax.Equals(boundRing.Value.PointMax))
        {
            FramesContainThis.AddFirst(boundRing);
            return false;
        }
        
        return true;
    }

    //Возращает false если рамки пересекаются(не важно как).
    //Метод вычисляет местоположение рамки boundRing относительно relativeBoundRing
    //и запоминает это местоположение в кэше.
    private bool DetectSeparatingZone(LinkedListNode<BoundingRing> relativeBoundRing,
        LinkedListNode<BoundingRing> boundRing)
    {
        List<Zones> list = new List<Zones>(3);
        bool flagA = false;
        bool flagC = false;
        bool flagE = false;
        bool flagG = false;
        bool flagAbc = false;
        bool flagCde = false;
        bool flagEfg = false;
        bool flagAhg = false;
        if (boundRing.Value.PointMin.Y > relativeBoundRing.Value.PointMax.Y &&
            !IntersectsChecker
                .CompareEquality(boundRing.Value.PointMin.Y, relativeBoundRing.Value.PointMax.Y, _epsilon))
        {
            flagAbc = true;
            SetFlags(
                boundRing,
                boundRing.Value.PointMin.X,
                boundRing.Value.PointMax.X,
                relativeBoundRing.Value.PointMin.X,
                relativeBoundRing.Value.PointMax.X,
                Zones.C,
                Zones.A, 
                Zones.B,
                ref flagC,
                ref flagA,
                list);
        }

        else if (boundRing.Value.PointMax.X < relativeBoundRing.Value.PointMin.X &&
                 !IntersectsChecker
                     .CompareEquality(relativeBoundRing.Value.PointMin.X, boundRing.Value.PointMax.X, _epsilon))
        {
            flagCde = true;
            SetFlags(
                boundRing,
                boundRing.Value.PointMin.Y,
                boundRing.Value.PointMax.Y,
                relativeBoundRing.Value.PointMin.Y,
                relativeBoundRing.Value.PointMax.Y,
                Zones.E,
                Zones.C, 
                Zones.D,
                ref flagE,
                ref flagC,
                list);
        }

        else if (boundRing.Value.PointMax.Y < relativeBoundRing.Value.PointMin.Y &&
                 !IntersectsChecker
                     .CompareEquality(relativeBoundRing.Value.PointMin.Y, boundRing.Value.PointMax.Y, _epsilon))
        { 
            flagEfg = true;
            SetFlags(
                boundRing,
                boundRing.Value.PointMin.X,
                boundRing.Value.PointMax.X,
                relativeBoundRing.Value.PointMin.X,
                relativeBoundRing.Value.PointMax.X,
                Zones.E,
                Zones.G, 
                Zones.F,
                ref flagE,
                ref flagG,
                list);
        }

        else if (boundRing.Value.PointMin.X > relativeBoundRing.Value.PointMax.X && 
                 !IntersectsChecker
                     .CompareEquality(boundRing.Value.PointMin.X, relativeBoundRing.Value.PointMax.X, _epsilon))
        {
            flagAhg = true;
            SetFlags(
                boundRing,
                boundRing.Value.PointMin.Y,
                boundRing.Value.PointMax.Y,
                relativeBoundRing.Value.PointMin.Y,
                relativeBoundRing.Value.PointMax.Y,
                Zones.G,
                Zones.A, 
                Zones.H,
                ref flagG,
                ref flagA,
                list);
        }
        else return false;
        
        if ((!NearRing.ContainsKey(Zones.Abc) ||
             boundRing.Value.PointMin.Y < NearRing[Zones.Abc].BoundRing.Value.PointMin.Y) &&
            (flagAbc || (list.Count == 1 && (flagA || flagC))))
        {
            NearRing.Remove(Zones.Abc);
            NearRing.Add(Zones.Abc, new RingAndZones(boundRing, list));
        }
        if ((!NearRing.ContainsKey(Zones.Cde) ||
             boundRing.Value.PointMax.X > NearRing[Zones.Cde].BoundRing.Value.PointMax.X) &&
            (flagCde || (list.Count == 1 && (flagC || flagE))))
        {
            NearRing.Remove(Zones.Cde);
            NearRing.Add(Zones.Cde, new RingAndZones(boundRing, list));
        }

        if ((!NearRing.ContainsKey(Zones.Efg) ||
             boundRing.Value.PointMax.Y > NearRing[Zones.Efg].BoundRing.Value.PointMax.Y) &&
            (flagEfg || (list.Count == 1 && (flagE || flagG))))
        {
            NearRing.Remove(Zones.Efg);
            NearRing.Add(Zones.Efg, new RingAndZones(boundRing, list));
        }

        if ((!NearRing.ContainsKey(Zones.Ahg) ||
             boundRing.Value.PointMin.X < NearRing[Zones.Ahg].BoundRing.Value.PointMin.X) &&
            (flagAhg || (list.Count == 1 && (flagA || flagG))))
        {
            NearRing.Remove(Zones.Ahg);
            NearRing.Add(Zones.Ahg, new RingAndZones(boundRing, list));
        }
        
        return true;
    }

    private void SetFlags(
        LinkedListNode<BoundingRing> boundRing,
        double boundMin,
        double boundMax,
        double relativeMin, 
        double relativeMax,
        Zones first,
        Zones second,
        Zones arrangeZone,
        ref bool firstFlag,
        ref bool secondFlag,
        List<Zones> list
        )
    {
        if (boundMin < relativeMin ||
            IntersectsChecker
                .CompareEquality(relativeMin, boundMin, _epsilon))
        {
            list.Add(first);
            RingsInZone[first].AddFirst(new RingAndZones(boundRing, list));
            firstFlag = true;
        }

        if (boundMax > relativeMax ||
            IntersectsChecker
                .CompareEquality(boundMax, relativeMax, _epsilon))
        {
            list.Add(second);
            RingsInZone[second].AddFirst(new RingAndZones(boundRing, list));
            secondFlag = true;
        }

        if (firstFlag == secondFlag
            || IntersectsChecker.SegmentContainAtLeastOneNumber
            (relativeMin,
                relativeMax,
                new[] { boundMin, boundMax }))
        {
            list.Add(arrangeZone);
            RingsInZone[arrangeZone].AddFirst(new RingAndZones(boundRing, list));
        }
    }
}