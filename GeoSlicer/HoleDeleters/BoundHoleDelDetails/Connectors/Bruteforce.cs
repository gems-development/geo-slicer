using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Utils;
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
        Cache cache, 
        IntersectsChecker intersectsChecker,
        LineService lineService,
        double epsilon)
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
                (thisRing, cache.IntersectFrames, ref connectRing, ref connectPoint, ref findNewConnectRing, intersectsChecker);
            FindNewConnectRing
                (thisRing, boundRingsInAbc, ref connectRing, ref connectPoint, ref findNewConnectRing, intersectsChecker);
            FindNewConnectRingInFramesWhoContainThis
                (thisRing, cache, ref connectRing, ref connectPoint, ref findNewConnectRing, intersectsChecker);
        }
        Connector.Connect(
            thisRing, connectRing,
            thisRing.Value.PointUpNode, connectPoint,
            listOfHoles, Zones.Abc, epsilon, lineService);
    }
    
    private static void FindNewConnectRing(
        LinkedListNode<BoundingRing> thisRing,
        IEnumerable<LinkedListNode<BoundingRing>> checkedRings,
        ref LinkedListNode<BoundingRing> connectRing,
        ref LinkedNode<Coordinate> connectPoint,
        ref bool findNewConnectRing,
        IntersectsChecker intersectsChecker)
    {
        bool findIntersectCheckedR = true;
        Coordinate connectPointThisR = thisRing.Value.PointUpNode.Elem;
        while (findIntersectCheckedR)
        {
            findIntersectCheckedR = false;
            foreach (var checkedRing in checkedRings)
            {
                if (intersectsChecker.LineIntersectsOrContainsInBoundRFrame(
                        checkedRing.Value, 
                        connectPointThisR, connectPoint.Elem))
                {
                    var pointInCheckedR = checkedRing.Value.PointUpNode;
                    do
                    {
                        if (intersectsChecker.HasIntersectedSegmentsNotExternalPoints(
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
        ref bool findNewConnectRing, 
        IntersectsChecker intersectsChecker)
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
                    bool flag1 = currentCoord.Elem.Y >= connectPointThisR.Y;
                    bool flag2 = currentCoord.Next.Elem.Y >= connectPointThisR.Y;
                    if ((flag1 || flag2) && 
                        intersectsChecker.HasIntersectedSegmentsNotExternalPoints(
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