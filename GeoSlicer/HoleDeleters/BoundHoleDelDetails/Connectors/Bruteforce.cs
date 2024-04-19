using System;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.HoleDeleters.BoundHoleDelDetails.Structures;
using GeoSlicer.Utils.BoundRing;
using NetTopologySuite.Geometries;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails.Connectors;

internal static class Bruteforce
{ 
    private static void InitializeConnectRing(
        LinkedListNode<BoundingRing> thisRing,
        Cache cache,
        List<LinkedListNode<BoundingRing>> boundRingsInAbc,
        out LinkedListNode<BoundingRing> connectRing,
        out LinkedNode<Coordinate> connectPoint)
    {
        if (boundRingsInAbc.Any())
        {
            connectRing = boundRingsInAbc.First();
            connectPoint = connectRing.Value.PointUpNode;
        }
        else
        {
            var startPoint = cache.FramesContainThis.First!.Value.Value.Ring;
            while(true)
            {
                if (startPoint.Elem.Y >= thisRing.Value.PointMax.Y)
                {
                    connectPoint = startPoint;
                    connectRing = cache.FramesContainThis.First!.Value;
                    break;
                }

                startPoint = startPoint.Next;
            }
        }
    }
    
    internal static void Connect(
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles,
        Cache cache)
    {
        var boundRingsInAbc = 
            cache.RingsInZone[Zones.A]
                .Union(cache.RingsInZone[Zones.B])
                .Union(cache.RingsInZone[Zones.C])
                .Select(a => a.BoundRing).ToList();   
        InitializeConnectRing(
            thisRing, cache, boundRingsInAbc,
            out LinkedListNode<BoundingRing> connectRing,
            out LinkedNode<Coordinate> connectPoint);
        
        bool findNewConnectRing = true;
        while (findNewConnectRing)
        {
            findNewConnectRing = false;
            FindNewConnectRing
                (thisRing, cache.IntersectFrames, ref connectRing, ref connectPoint, ref findNewConnectRing);
            FindNewConnectRing
                (thisRing, boundRingsInAbc, ref connectRing, ref connectPoint, ref findNewConnectRing);
            FindNewConnectRingInFramesWhoContainThis
                (thisRing, cache, ref connectRing, ref connectPoint, ref findNewConnectRing);
        }
        thisRing.Value.ConnectBoundRings(
            connectRing.Value,
            thisRing.Value.PointUpNode,
            connectPoint);
        listOfHoles.Remove(connectRing);
    }
    
    private static void FindNewConnectRing(
        LinkedListNode<BoundingRing> thisRing,
        IEnumerable<LinkedListNode<BoundingRing>> checkedRings,
        ref LinkedListNode<BoundingRing> connectRing,
        ref LinkedNode<Coordinate> connectPoint,
        ref bool findNewConnectRing)
    {
        bool findIntersectCheckedR = true;
        Coordinate connectPointThisR = thisRing.Value.PointUpNode.Elem;
        while (findIntersectCheckedR)
        {
            findIntersectCheckedR = false;
            foreach (var checkedRing in checkedRings)
            {
                if (IntersectsChecker.LineIntersectsOrContainsInBoundRFrame(
                        checkedRing.Value, 
                        connectPointThisR, connectPoint.Elem))
                {
                    var pointInCheckedR = checkedRing.Value.PointUpNode;
                    do
                    {
                        if (IntersectsChecker.HasIntersectedSegmentsNotExternalPoints(
                                pointInCheckedR.Elem, pointInCheckedR.Next.Elem,
                                connectPointThisR, connectPoint.Elem))
                        {
                            connectRing = checkedRing;
                            connectPoint = 
                                pointInCheckedR.Elem.Y > connectPointThisR.Y ? pointInCheckedR : pointInCheckedR.Next;
                            findIntersectCheckedR = true;
                            findNewConnectRing = true;
                        }
                        pointInCheckedR = pointInCheckedR.Next;
                    } while (!ReferenceEquals(pointInCheckedR, checkedRing.Value.PointUpNode));
                }
            }
        }
    }
    
    private static void FindNewConnectRingInFramesWhoContainThis(
        LinkedListNode<BoundingRing> thisRing,
        Cache cache,
        ref LinkedListNode<BoundingRing> connectRing,
        ref LinkedNode<Coordinate> connectPoint,
        ref bool findNewConnectRing)
    {
        Coordinate connectPointThisR = thisRing.Value.PointUpNode.Elem;
        bool findRFramesContainThis = true;
        while (findRFramesContainThis)
        {
            findRFramesContainThis = false;
            foreach (var ringFramesContainThis in cache.FramesContainThis)
            {
                var currentCoord = ringFramesContainThis.Value.Ring;
                do
                {
                    bool flag1 = currentCoord.Elem.Y > connectPointThisR.Y;
                    bool flag2 = currentCoord.Next.Elem.Y > connectPointThisR.Y;
                    if ((flag1 || flag2) && 
                        IntersectsChecker.HasIntersectedSegmentsNotExternalPoints(
                            connectPointThisR, connectPoint.Elem,
                            currentCoord.Elem, currentCoord.Next.Elem))
                    {
                        connectRing = ringFramesContainThis;
                        connectPoint = flag1 ? currentCoord : currentCoord.Next;
                        findRFramesContainThis = true;
                        findNewConnectRing = true;
                    }
                    currentCoord = currentCoord.Next;
                } while (!ReferenceEquals(currentCoord, ringFramesContainThis.Value.Ring));
            }
        }
    }
}