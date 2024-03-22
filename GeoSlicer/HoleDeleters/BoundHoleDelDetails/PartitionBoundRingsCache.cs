using System;
using System.Collections.Generic;
using GeoSlicer.Utils.BoundRing;
using NetTopologySuite.Geometries;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails;
//Кэш алгоритма BoundingHoleDeleter.
//Например в списке ListA хранятся рамки, расположенные севернее рамки,
//относительно которой проводилось заполнение кэша. Соотвествие букв и сторон света описано в PartitioningZones.
internal class PartitionBoundRingsCache
{
    internal LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> 
        ListA { get; private set; } = new ();
    internal LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)>
        ListB { get; private set; } = new ();
    internal LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> 
        ListC{ get; private set; } = new ();
    internal LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> 
        ListD{ get; private set; } = new ();
    internal LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)>
        ListE{ get; private set; } = new ();
    internal LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> 
        ListF{ get; private set; } = new ();

    internal LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)>
        ListG { get; private set; } = new();

    internal LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)>
        ListH { get; private set; } = new();

    internal LinkedList<LinkedListNode<BoundingRing>>
        IntersectFrames { get; private set; } = new();

    internal LinkedList<LinkedListNode<BoundingRing>> 
        FramesContainThis { get; private set; } = new();
    
    internal (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearAbc { get; private set; }
    internal (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearCde { get; private set; }
    internal (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearEfg { get; private set; }
    internal (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearAhg { get; private set; }


    internal (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearAbcIntersect { get; set; }
    internal (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearCdeIntersect { get; set; }
    internal (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearEfgIntersect { get; set; }
    internal (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearAhgIntersect { get; set; }
    internal (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? NearAbcSegmentIntersect { get; set; }
    internal (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? NearCdeSegmentIntersect { get; set; }
    internal (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? NearEfgSegmentIntersect { get; set; }
    internal (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? NearAhgSegmentIntersect { get; set; }

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
                    if (!DetectPartitingZone(boundRing, thisRing))
                    {
                        hasIntersectFrames = IntersectOrContainFrames(boundRing, thisRing);
                        if (hasIntersectFrames)
                            IntersectFrames.AddFirst(thisRing);
                    }
                }
                else if (BoundRIntersectsChecker.NotIntersectCheck(boundRing.Value, thisRing.Value))
                {
                    if (IntersectOrContainFrames(boundRing, thisRing))
                        DetectPartitingZone(boundRing, thisRing);
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
    
    //todo Переименовать Partiting в Separating
    //todo принять решение о разбиении этой функции
    //Возращает false если рамки пересекаются(не важно как).
    //Метод вычисляет местоположение рамки boundRing относительно relativeBoundRing
    //и запоминает это местоположение в кэше.
    private bool DetectPartitingZone(LinkedListNode<BoundingRing> relativeBoundRing,
        LinkedListNode<BoundingRing> boundRing)
    {
        List<PartitioningZones> list = new List<PartitioningZones>(3);
        bool flagA = false;
        bool flagC = false;
        bool flagE = false;
        bool flagG = false;
        bool flagAbc = false;
        bool flagCde = false;
        bool flagEfg = false;
        bool flagAhg = false;
        if (boundRing.Value.PointMin.Y >= relativeBoundRing.Value.PointMax.Y)
        {
            if (boundRing.Value.PointMin.X < relativeBoundRing.Value.PointMin.X)
            {
                list.Add(PartitioningZones.C);
                ListC.AddFirst((boundRing, list));
                flagC = true;
            }

            if (boundRing.Value.PointMax.X > relativeBoundRing.Value.PointMax.X)
            {
                list.Add(PartitioningZones.A);
                ListA.AddFirst((boundRing, list));
                flagA = true;
            }

            if (flagA == flagC
                || BoundRIntersectsChecker.SegmentContainAtLeastOneNumber
                (relativeBoundRing.Value.PointMin.X,
                    relativeBoundRing.Value.PointMax.X,
                    new[] { boundRing.Value.PointMin.X, boundRing.Value.PointMax.X }))
            {
                list.Add(PartitioningZones.B);
                ListB.AddFirst((boundRing, list));
            }

            flagAbc = true;

        }

        else if (boundRing.Value.PointMax.X <= relativeBoundRing.Value.PointMin.X)
        {
            if (boundRing.Value.PointMax.Y > relativeBoundRing.Value.PointMax.Y)
            {
                list.Add(PartitioningZones.C); 
                ListC.AddFirst((boundRing, list));
                flagC = true;
            }

            if (boundRing.Value.PointMin.Y < relativeBoundRing.Value.PointMin.Y)
            {
                list.Add(PartitioningZones.E);
                ListE.AddFirst((boundRing, list));
                flagE = true;
            }

            if (flagE == flagC
                || BoundRIntersectsChecker.SegmentContainAtLeastOneNumber
                (relativeBoundRing.Value.PointMin.Y,
                    relativeBoundRing.Value.PointMax.Y,
                    new[] { boundRing.Value.PointMin.Y, boundRing.Value.PointMax.Y }))
            {
                list.Add(PartitioningZones.D);
                ListD.AddFirst((boundRing, list));
            }

            flagCde = true;

        }

        else if (boundRing.Value.PointMax.Y <= relativeBoundRing.Value.PointMin.Y)
        {
            if (boundRing.Value.PointMax.X > relativeBoundRing.Value.PointMax.X)
            {
                list.Add(PartitioningZones.G);
                ListG.AddFirst((boundRing, list));
                flagG = true;
            }

            if (boundRing.Value.PointMin.X < relativeBoundRing.Value.PointMin.X)
            {
                list.Add(PartitioningZones.E);
                ListE.AddFirst((boundRing, list));
                flagE = true;
            }

            if (flagE == flagG
                || BoundRIntersectsChecker.SegmentContainAtLeastOneNumber
                (relativeBoundRing.Value.PointMin.X,
                    relativeBoundRing.Value.PointMax.X,
                    new[] { boundRing.Value.PointMin.X, boundRing.Value.PointMax.X }))
            {
                list.Add(PartitioningZones.F);
                ListF.AddFirst((boundRing, list));
            }

            flagEfg = true;
        }

        else if (boundRing.Value.PointMin.X >= relativeBoundRing.Value.PointMax.X)
        {
            if (boundRing.Value.PointMin.Y < relativeBoundRing.Value.PointMin.Y)
            {
                list.Add(PartitioningZones.G);
                ListG.AddFirst((boundRing, list));
                flagG = true;
            }

            if (boundRing.Value.PointMax.Y > relativeBoundRing.Value.PointMax.Y)
            {
                list.Add(PartitioningZones.A);
                ListA.AddFirst((boundRing, list));
                flagA = true;
            }

            if (flagA == flagG
                || BoundRIntersectsChecker.SegmentContainAtLeastOneNumber
                (relativeBoundRing.Value.PointMin.Y,
                    relativeBoundRing.Value.PointMax.Y,
                    new[] { boundRing.Value.PointMin.Y, boundRing.Value.PointMax.Y }))
            {
                list.Add(PartitioningZones.H);
                ListH.AddFirst((boundRing, list));
            }

            flagAhg = true;
        }
        else return false;
        
        if ((NearAbc is null ||
             boundRing.Value.PointMin.Y < NearAbc.Value.boundRing.Value.PointMin.Y) &&
            (flagAbc || (list.Count == 1 && (flagA || flagC))))
        {
            NearAbc = (boundRing, list);
        }
        if ((NearCde is null ||
             boundRing.Value.PointMax.X > NearCde.Value.boundRing.Value.PointMax.X) &&
            (flagCde || (list.Count == 1 && (flagC || flagE))))
        {
            NearCde = (boundRing, list);
        }

        if ((NearEfg is null ||
             boundRing.Value.PointMax.Y > NearEfg.Value.boundRing.Value.PointMax.Y) &&
            (flagEfg || (list.Count == 1 && (flagE || flagG))))
        {
            NearEfg = (boundRing, list);
        }

        if ((NearAhg is null ||
             boundRing.Value.PointMin.X < NearAhg.Value.boundRing.Value.PointMin.X) &&
            (flagAhg || (list.Count == 1 && (flagA || flagG))))
        {
            NearAhg = (boundRing, list);
        }
        
        return true;
    }
}