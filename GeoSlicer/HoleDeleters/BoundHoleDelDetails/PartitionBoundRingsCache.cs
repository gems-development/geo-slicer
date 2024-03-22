using System;
using System.Collections.Generic;
using GeoSlicer.Utils.BoundRing;
using NetTopologySuite.Geometries;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails;

public class PartitionBoundRingsCache
{
    public LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> ListA { get; private set; }
    public LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> ListB { get; private set; }
    public LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> ListC{ get; private set; }
    public LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> ListD{ get; private set; }
    public LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> ListE{ get; private set; }
    public LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> ListF{ get; private set; }
    public LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> ListG{ get; private set; }
    public LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> ListH{ get; private set; }
    public LinkedList<LinkedListNode<BoundingRing>> IntersectFrames { get; private set; }
    public LinkedList<LinkedListNode<BoundingRing>> FramesContainThis { get; private set; }
    public (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearABC { get; private set; }
    public (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearCDE { get; private set; }
    public (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearEFG { get; private set; }
    public (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearAHG { get; private set; }


    public (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearABCintersect { get; set; }
    public (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearCDEintersect { get; set; }
    public (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearEFGintersect { get; set; }
    public (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearAHGintersect { get; set; }
    public (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? NearABCSegmentintersect { get; set; }
    public (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? NearCDESegmentintersect { get; set; }
    public (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? NearEFGSegmentintersect { get; set; }
    public (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? NearAHGSegmentintersect { get; set; }
    public PartitionBoundRingsCache()
    {
        ListA = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        ListB = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        ListC = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        ListD = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        ListE = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        ListF = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        ListG = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        ListH = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        IntersectFrames = new LinkedList<LinkedListNode<BoundingRing>>();
        FramesContainThis = new LinkedList<LinkedListNode<BoundingRing>>();
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
        NearABC = null;
        NearCDE = null;
        NearEFG = null;
        NearAHG = null;
        NearABCintersect = null;
        NearCDEintersect = null;
        NearEFGintersect = null;
        NearAHGintersect = null;
        NearABCSegmentintersect = null;
        NearCDESegmentintersect = null;
        NearEFGSegmentintersect = null;
        NearAHGSegmentintersect = null;
    }
    public bool FillListsRelativeRing(
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
    private bool DetectPartitingZone(LinkedListNode<BoundingRing> relativeBoundRing,
        LinkedListNode<BoundingRing> boundRing)
    {
        List<PartitioningZones> list = new List<PartitioningZones>(3);
        bool flagA = false;
        bool flagC = false;
        bool flagE = false;
        bool flagG = false;
        bool flagABC = false;
        bool flagCDE = false;
        bool flagEFG = false;
        bool flagAHG = false;
        if (boundRing.Value.PointMin.Y > relativeBoundRing.Value.PointMax.Y
            || Math.Abs(boundRing.Value.PointMin.Y - relativeBoundRing.Value.PointMax.Y) < 1e-9)
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

            flagABC = true;

        }

        else if (boundRing.Value.PointMax.X < relativeBoundRing.Value.PointMin.X
            || Math.Abs(boundRing.Value.PointMax.X - relativeBoundRing.Value.PointMin.X) < 1e-9)
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

            flagCDE = true;

        }

        else if (boundRing.Value.PointMax.Y < relativeBoundRing.Value.PointMin.Y
            || Math.Abs(boundRing.Value.PointMax.Y - relativeBoundRing.Value.PointMin.Y) < 1e-9)
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

            flagEFG = true;
        }

        else if (boundRing.Value.PointMin.X > relativeBoundRing.Value.PointMax.X
            || Math.Abs(boundRing.Value.PointMin.X - relativeBoundRing.Value.PointMax.X) < 1e-9)
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

            flagAHG = true;
        }
        else return false;
        
        if ((NearABC is null ||
             boundRing.Value.PointMin.Y < NearABC.Value.boundRing.Value.PointMin.Y) &&
            (flagABC || (list.Count == 1 && (flagA || flagC))))
        {
            NearABC = (boundRing, list);
        }
        if ((NearCDE is null ||
             boundRing.Value.PointMax.X > NearCDE.Value.boundRing.Value.PointMax.X) &&
            (flagCDE || (list.Count == 1 && (flagC || flagE))))
        {
            NearCDE = (boundRing, list);
        }

        if ((NearEFG is null ||
             boundRing.Value.PointMax.Y > NearEFG.Value.boundRing.Value.PointMax.Y) &&
            (flagEFG || (list.Count == 1 && (flagE || flagG))))
        {
            NearEFG = (boundRing, list);
        }

        if ((NearAHG is null ||
             boundRing.Value.PointMin.X < NearAHG.Value.boundRing.Value.PointMin.X) &&
            (flagAHG || (list.Count == 1 && (flagA || flagG))))
        {
            NearAHG = (boundRing, list);
        }
        
        return true;
    }
}