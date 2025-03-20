using System;
using System.Collections.Generic;
using GeoSlicer.Utils.BoundRing;
using NetTopologySuite.Geometries;
using GeoSlicer.HoleDeleters.BoundHoleDelDetails.Structures;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails;

/// <summary>
/// Кэш алгоритма BoundingHoleDeleter.
/// Например, в списке ListA хранятся рамки, расположенные севернее рамки,
/// относительно которой проводилось заполнение кэша. Соответствие букв и сторон света описано в PartitioningZones.
/// </summary>
internal class Cache
{
    private readonly double _epsilon;
    private readonly IntersectsChecker _intersectsChecker;

    internal readonly IReadOnlyDictionary<Zones, LinkedList<RingAndZones>> RingsInZone;

    internal readonly LinkedList<LinkedListNode<BoundingRing>> IntersectFrames = new();
    internal readonly LinkedList<LinkedListNode<BoundingRing>> FramesContainThis = new();

    internal readonly Dictionary<Zones, RingAndZones> NearRing = new();
    internal readonly Dictionary<Zones, RingAndPoint> NearSegmentIntersect = new();

    internal Cache(double epsilon, IntersectsChecker checker)
    {
        _epsilon = epsilon;
        _intersectsChecker = checker;
        var thisRingsInZone = new Dictionary<Zones, LinkedList<RingAndZones>>
        {
            { Zones.A, new LinkedList<RingAndZones>() },
            { Zones.B, new LinkedList<RingAndZones>() },
            { Zones.C, new LinkedList<RingAndZones>() },
            { Zones.D, new LinkedList<RingAndZones>() },
            { Zones.E, new LinkedList<RingAndZones>() },
            { Zones.F, new LinkedList<RingAndZones>() },
            { Zones.G, new LinkedList<RingAndZones>() },
            { Zones.H, new LinkedList<RingAndZones>() }
        };
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

    /// <summary>
    /// Метод вычисляет местоположение рамок из <paramref name="boundRings"/> относительно
    /// <paramref name="boundRing"/> и помещает информацию в кэш.
    /// </summary>
    /// <returns>
    /// Возвращает true, если были обнаружены кольца, рамки которых пересекают
    /// рамку кольца <paramref name="boundRing"/>.
    /// </returns>
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
                        hasIntersectFrames = CheckIntersectsOrContainsFrames(boundRing, thisRing);
                        if (hasIntersectFrames)
                            IntersectFrames.AddFirst(thisRing);
                    }
                }
                else if (_intersectsChecker.CheckNotIntersects(boundRing.Value, thisRing.Value, _epsilon))
                {
                    if (CheckIntersectsOrContainsFrames(boundRing, thisRing))
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


    /// <summary>
    /// Проверяет, содержится ли <paramref name="boundRing"/> в рамке <paramref name="relativeBoundRing"/>
    /// или наоборот.
    /// </summary>
    /// <returns> True, если не содержатся (могут пересекаться и не пересекаться), false иначе</returns>
    private bool CheckIntersectsOrContainsFrames(
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

    /// <summary>
    /// Метод вычисляет местоположение рамки <paramref name="boundRing"/> относительно
    /// <paramref name="relativeBoundRing"/> и запоминает это местоположение в кэше.
    /// </summary>
    /// <returns> False если рамки пересекаются (не важно как).</returns>
    private bool DetectSeparatingZone(
        LinkedListNode<BoundingRing> relativeBoundRing,
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
            !_intersectsChecker
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
                 !_intersectsChecker
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
                 !_intersectsChecker
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
                 !_intersectsChecker
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
            _intersectsChecker
                .CompareEquality(relativeMin, boundMin, _epsilon))
        {
            list.Add(first);
            RingsInZone[first].AddFirst(new RingAndZones(boundRing, list));
            firstFlag = true;
        }

        if (boundMax > relativeMax ||
            _intersectsChecker
                .CompareEquality(boundMax, relativeMax, _epsilon))
        {
            list.Add(second);
            RingsInZone[second].AddFirst(new RingAndZones(boundRing, list));
            secondFlag = true;
        }

        if (firstFlag == secondFlag
            || _intersectsChecker.CheckSegmentContainsAtLeastOneNumber
            (relativeMin,
                relativeMax,
                new[] { boundMin, boundMax }))
        {
            list.Add(arrangeZone);
            RingsInZone[arrangeZone].AddFirst(new RingAndZones(boundRing, list));
        }
    }
}