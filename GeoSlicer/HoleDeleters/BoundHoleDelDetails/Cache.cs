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
    
    internal LinkedList<RingAndZones> ListA { get; private set; } = new ();
    internal LinkedList<RingAndZones> ListB { get; private set; } = new ();
    internal LinkedList<RingAndZones> ListC{ get; private set; } = new ();
    internal LinkedList<RingAndZones> ListD{ get; private set; } = new ();
    internal LinkedList<RingAndZones> ListE{ get; private set; } = new ();
    internal LinkedList<RingAndZones> ListF{ get; private set; } = new ();
    internal LinkedList<RingAndZones> ListG { get; private set; } = new();
    internal LinkedList<RingAndZones> ListH { get; private set; } = new();

    internal LinkedList<LinkedListNode<BoundingRing>>IntersectFrames { get; private set; } = new();
    internal LinkedList<LinkedListNode<BoundingRing>> FramesContainThis { get; private set; } = new();
    
    internal RingAndZones? NearAbc { get; private set; }
    internal RingAndZones? NearCde { get; private set; }
    internal RingAndZones? NearEfg { get; private set; }
    internal RingAndZones? NearAhg { get; private set; }
    
    internal RingAndZones? NearAbcIntersect { get; set; }
    internal RingAndZones? NearCdeIntersect { get; set; }
    internal RingAndZones? NearEfgIntersect { get; set; }
    internal RingAndZones? NearAhgIntersect { get; set; }
    internal RingAndPoint? NearAbcSegmentIntersect { get; set; }
    internal RingAndPoint? NearCdeSegmentIntersect { get; set; }
    internal RingAndPoint? NearEfgSegmentIntersect { get; set; }
    internal RingAndPoint? NearAhgSegmentIntersect { get; set; }

    internal Cache(double epsilon)
    {
        _epsilon = epsilon;
    }
    
    private void Clear()
    {
        ListA.Clear();
        ListB.Clear();
        ListC.Clear();
        ListD.Clear();
        ListE.Clear();
        ListF.Clear();
        ListG.Clear();
        ListH.Clear();
        IntersectFrames.Clear();
        FramesContainThis.Clear();
        NearAbc = null;
        NearCde = null;
        NearEfg = null;
        NearAhg = null;
        NearAbcIntersect = null;
        NearCdeIntersect = null;
        NearEfgIntersect = null;
        NearAhgIntersect = null;
        NearAbcSegmentIntersect = null;
        NearCdeSegmentIntersect = null;
        NearEfgSegmentIntersect = null;
        NearAhgSegmentIntersect = null;
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
        List<SeparatingZones> list = new List<SeparatingZones>(3);
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
                SeparatingZones.C,
                SeparatingZones.A, 
                SeparatingZones.B,
                ref flagC,
                ref flagA,
                ListC, 
                ListA,
                ListB,
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
                SeparatingZones.E,
                SeparatingZones.C, 
                SeparatingZones.D,
                ref flagE,
                ref flagC,
                ListE, 
                ListC,
                ListD,
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
                SeparatingZones.E,
                SeparatingZones.G, 
                SeparatingZones.F,
                ref flagE,
                ref flagG,
                ListE, 
                ListG,
                ListF,
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
                SeparatingZones.G,
                SeparatingZones.A, 
                SeparatingZones.H,
                ref flagG,
                ref flagA,
                ListG, 
                ListA,
                ListH,
                list);
        }
        else return false;
        
        if ((NearAbc is null ||
             boundRing.Value.PointMin.Y < NearAbc.BoundRing.Value.PointMin.Y) &&
            (flagAbc || (list.Count == 1 && (flagA || flagC))))
        {
            NearAbc = new RingAndZones(boundRing, list);
        }
        if ((NearCde is null ||
             boundRing.Value.PointMax.X > NearCde.BoundRing.Value.PointMax.X) &&
            (flagCde || (list.Count == 1 && (flagC || flagE))))
        {
            NearCde = new RingAndZones(boundRing, list);
        }

        if ((NearEfg is null ||
             boundRing.Value.PointMax.Y > NearEfg.BoundRing.Value.PointMax.Y) &&
            (flagEfg || (list.Count == 1 && (flagE || flagG))))
        {
            NearEfg = new RingAndZones(boundRing, list);
        }

        if ((NearAhg is null ||
             boundRing.Value.PointMin.X < NearAhg.BoundRing.Value.PointMin.X) &&
            (flagAhg || (list.Count == 1 && (flagA || flagG))))
        {
            NearAhg = new RingAndZones(boundRing, list);
        }
        
        return true;
    }

    private void SetFlags(
        LinkedListNode<BoundingRing> boundRing,
        double boundMin,
        double boundMax,
        double relativeMin, 
        double relativeMax,
        SeparatingZones first,
        SeparatingZones second,
        SeparatingZones arrangeZone,
        ref bool firstFlag,
        ref bool secondFlag,
        LinkedList<RingAndZones> firstList,
        LinkedList<RingAndZones> secondList,
        LinkedList<RingAndZones> arrangeZoneList,
        List<SeparatingZones> list
        )
    {
        if (boundMin < relativeMin ||
            IntersectsChecker
                .CompareEquality(relativeMin, boundMin, _epsilon))
        {
            list.Add(first);
            firstList.AddFirst(new RingAndZones(boundRing, list));
            firstFlag = true;
        }

        if (boundMax > relativeMax ||
            IntersectsChecker
                .CompareEquality(boundMax, relativeMax, _epsilon))
        {
            list.Add(second);
            secondList.AddFirst(new RingAndZones(boundRing, list));
            secondFlag = true;
        }

        if (firstFlag == secondFlag
            || IntersectsChecker.SegmentContainAtLeastOneNumber
            (relativeMin,
                relativeMax,
                new[] { boundMin, boundMax }))
        {
            list.Add(arrangeZone);
            arrangeZoneList.AddFirst(new RingAndZones(boundRing, list));
        }
    }
}