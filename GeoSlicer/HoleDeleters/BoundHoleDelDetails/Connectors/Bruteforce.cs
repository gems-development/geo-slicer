using System;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Utils.BoundRing;
using NetTopologySuite.Geometries;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails.Connectors;

internal static class Bruteforce
{
    internal static void Connect(
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles,
        Cache cache)
    {
        LinkedListNode<BoundingRing> connectedFrame;
        LinkedNode<Coordinate> connectedPoint;
        //todo сделать объединение этих спискков без повторяющихся элементов
        var collectionABC = 
            cache.RingsInZone[Zones.A]
                .Concat(cache.RingsInZone[Zones.B])
                .Concat(cache.RingsInZone[Zones.C]);
        if (collectionABC.Any())
        {
            connectedFrame = collectionABC.First().BoundRing;
            connectedPoint = connectedFrame.Value.PointUpNode;
        }
        else
        {
            var start = cache.FramesContainThis.First!.Value.Value.Ring;
            while(true)
            {
                if (start.Elem.Y >= thisRing.Value.PointMax.Y)
                {
                    connectedPoint = start;
                    connectedFrame = cache.FramesContainThis.First!.Value;
                    break;
                }

                start = start.Next;
            }
        }
        
        bool flag = true;
        while (flag)
        {
            flag = false;

            bool flagFirstCycle = true;
            while (flagFirstCycle)
            {
                flagFirstCycle = false;
                foreach (var frame in cache.IntersectFrames)
                {
                    if (IntersectsChecker.HasIntersectsBoundRFrame(
                            frame.Value,
                            thisRing.Value.PointUpNode.Elem,
                            connectedPoint.Elem) || 
                        IntersectsChecker.PointInsideBoundRFrame(thisRing.Value.PointUpNode.Elem, frame.Value)|| 
                        IntersectsChecker.PointInsideBoundRFrame(connectedPoint.Elem, frame.Value))
                    {
                        var start = frame.Value.PointUpNode;
                        do
                        {
                            if (IntersectsChecker.HasIntersectedSegmentsNotExternalPoints(
                                    start.Elem, start.Next.Elem,
                                    thisRing.Value.PointUpNode.Elem, connectedPoint.Elem))
                            {
                                

                                connectedFrame = frame;
                                if (start.Elem.Y > thisRing.Value.PointUpNode.Elem.Y ||
                                    Math.Abs(start.Elem.Y - thisRing.Value.PointUpNode.Elem.Y) < 1e-9)
                                {
                                    connectedPoint = start;
                                }
                                else connectedPoint = start.Next;

                                flagFirstCycle = true;
                                flag = true;
                            }

                            start = start.Next;
                        } while (!ReferenceEquals(start, frame.Value.PointUpNode));
                    }
                }
            }

            bool flagSecondCycle = true;
            while (flagSecondCycle)
            {
                flagSecondCycle = false;
                foreach (var frame in collectionABC)
                {
                    if (IntersectsChecker.HasIntersectsBoundRFrame(
                            frame.BoundRing.Value,
                            thisRing.Value.PointUpNode.Elem,
                            connectedPoint.Elem))
                    {
                        var start = frame.BoundRing.Value.PointUpNode;
                        do
                        {
                            if (IntersectsChecker.HasIntersectedSegmentsNotExternalPoints(
                                    start.Elem, start.Next.Elem,
                                    thisRing.Value.PointUpNode.Elem, connectedPoint.Elem))
                            {

                                connectedFrame = frame.BoundRing;
                                if (start.Elem.Y > thisRing.Value.PointUpNode.Elem.Y ||
                                    Math.Abs(start.Elem.Y - thisRing.Value.PointUpNode.Elem.Y) < 1e-9)
                                {
                                    connectedPoint = start;
                                }
                                else connectedPoint = start.Next;

                                flagSecondCycle = true;
                                flag = true;
                            }

                            start = start.Next;
                        } while (!ReferenceEquals(start, frame.BoundRing.Value.PointUpNode));
                    }
                }
            }

            bool flagThirdCycle = true;
            while (flagThirdCycle)
            {
                flagThirdCycle = false;
                foreach (var shell in cache.FramesContainThis)
                {
                    var start = shell.Value.Ring;
                    do
                    {
                        bool flag1 = start.Elem.Y > thisRing.Value.PointUpNode.Elem.Y ||
                                     Math.Abs(start.Elem.Y - thisRing.Value.PointUpNode.Elem.Y) < 1e-9;
                        bool flag2 = start.Next.Elem.Y > thisRing.Value.PointUpNode.Elem.Y ||
                                     Math.Abs(start.Next.Elem.Y - thisRing.Value.PointUpNode.Elem.Y) < 1e-9;

                        if (flag1 || flag2)
                        {
                            if (IntersectsChecker.HasIntersectedSegmentsNotExternalPoints(
                                    thisRing.Value.PointUpNode.Elem,
                                    connectedPoint.Elem,
                                    start.Elem,
                                    start.Next.Elem))
                            {
                                connectedFrame = shell;
                                if (flag1)
                                {
                                    connectedPoint = start;
                                }
                                else
                                {
                                    connectedPoint = start.Next;
                                }

                                flagThirdCycle = true;
                                flag = true;
                            }
                        }

                        start = start.Next;
                    } while (!ReferenceEquals(start, shell.Value.Ring));
                }
            }
        }
        
        thisRing.Value.ConnectBoundRings(
            connectedFrame.Value,
            thisRing.Value.PointUpNode,
            connectedPoint);
        listOfHoles.Remove(connectedFrame);
    }
}