﻿using System.Collections.Generic;
using GeoSlicer.HoleDeleters.BoundHoleDelDetails.Structures;
using GeoSlicer.Utils.BoundRing;
using NetTopologySuite.Geometries;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails.Connectors;

internal static class WithIntersectRing
{
    /// <summary>
    /// Метод пытается соединить кольцо <paramref name="thisRing"/> с каким-либо кольцом
    /// из списка <paramref name="cache.FramesContainThis"/>.
    /// Перебор начинается с какого-либо кольца из <paramref name="cache.NearSegmentIntersect"/>.
    /// </summary>
    /// <returns>True, если соединение было успешно произведено.</returns>
    internal static bool TryBruteforceConnect(
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles,
        Cache cache,
        IntersectsChecker intersectsChecker)
    {
        if (cache.NearSegmentIntersect.TryGetValue(Zones.Abc, out var ringAndPoint))
        {
            ConnectWithBoundRFrameWhoContainThisRing(ringAndPoint, thisRing, listOfHoles, Zones.Abc, cache,
                intersectsChecker);
            return true;
        }

        if (cache.NearSegmentIntersect.TryGetValue(Zones.Cde, out ringAndPoint))
        {
            ConnectWithBoundRFrameWhoContainThisRing(ringAndPoint, thisRing, listOfHoles, Zones.Cde, cache,
                intersectsChecker);
            return true;
        }

        if (cache.NearSegmentIntersect.TryGetValue(Zones.Efg, out ringAndPoint))
        {
            ConnectWithBoundRFrameWhoContainThisRing(ringAndPoint, thisRing, listOfHoles, Zones.Efg, cache,
                intersectsChecker);
            return true;
        }

        if (cache.NearSegmentIntersect.TryGetValue(Zones.Ahg, out ringAndPoint))
        {
            ConnectWithBoundRFrameWhoContainThisRing(ringAndPoint, thisRing, listOfHoles, Zones.Ahg, cache,
                intersectsChecker);
            return true;
        }

        return false;
    }
    
    // todo: добавить проверку на пересечение соединения с прямоугольниками
    private static void ConnectWithBoundRFrameWhoContainThisRing(
        RingAndPoint? nearSegmentIntersect,
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles,
        Zones zonesUnion,
        Cache cache,
        IntersectsChecker intersectsChecker)
    {
        LinkedNode<Coordinate> connectCoordFrameContainThis =
            RearrangePoints(nearSegmentIntersect!.Start, zonesUnion, thisRing);
        SwapValuesInFramesContainThis(cache, nearSegmentIntersect);
        LinkedNode<Coordinate> connectCoordThisR = FindConnectCoordThisR(zonesUnion, thisRing);

        var frameContainThis = cache.FramesContainThis.First!.Value;
        bool findFrameContainThisIntersectsLine = true;
        while (findFrameContainThisIntersectsLine)
        {
            findFrameContainThisIntersectsLine = false;
            foreach (var frame in cache.FramesContainThis)
            {
                (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? intersectSegment;
                do
                {
                    intersectSegment = intersectsChecker.GetIntersectRingWithSegmentNotExtPoint(
                        frame,
                        connectCoordThisR.Elem, connectCoordFrameContainThis.Elem);

                    if (intersectSegment is not null)
                    {
                        findFrameContainThisIntersectsLine = true;
                        connectCoordFrameContainThis =
                            RearrangePoints(intersectSegment.Value._start, zonesUnion, thisRing);

                        frameContainThis = frame;
                    }
                } while (intersectSegment is not null);
            }
        }

        Connector.Connect(
            thisRing, frameContainThis,
            connectCoordThisR, connectCoordFrameContainThis, listOfHoles);
    }


    private static LinkedNode<Coordinate> RearrangePoints(
        LinkedNode<Coordinate> coord,
        Zones zones,
        LinkedListNode<BoundingRing> thisRing)
    {
        if (zones == Zones.Abc)
        {
            if (coord.Elem.Y < thisRing.Value.PointUpNode.Elem.Y)
            {
                coord = coord.Next;
            }
        }
        else if (zones == Zones.Cde)
        {
            if (coord.Elem.X > thisRing.Value.PointLeftNode.Elem.X)
            {
                coord = coord.Next;
            }
        }
        else if (zones == Zones.Efg)
        {
            if (coord.Elem.Y > thisRing.Value.PointDownNode.Elem.Y)
            {
                coord = coord.Next;
            }
        }
        else
        {
            if (coord.Elem.X < thisRing.Value.PointRightNode.Elem.X)
            {
                coord = coord.Next;
            }
        }

        return coord;
    }

    private static LinkedNode<Coordinate> FindConnectCoordThisR(
        Zones zonesUnion,
        LinkedListNode<BoundingRing> thisRing)
    {
        if (zonesUnion == Zones.Abc)
            return thisRing.Value.PointUpNode;

        if (zonesUnion == Zones.Cde)
            return thisRing.Value.PointLeftNode;

        if (zonesUnion == Zones.Efg)
            return thisRing.Value.PointDownNode;

        return thisRing.Value.PointRightNode;
    }

    private static void SwapValuesInFramesContainThis(Cache cache, RingAndPoint nearSegmentIntersect)
    {
        var buff = nearSegmentIntersect.BoundRing.Value;
        cache.FramesContainThis.Remove(nearSegmentIntersect.BoundRing);
        cache.FramesContainThis.AddFirst(buff);
        nearSegmentIntersect.BoundRing = cache.FramesContainThis.First!;
    }
}