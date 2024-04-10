using System;
using System.Collections.Generic;
using GeoSlicer.HoleDeleters.BoundHoleDelDetails.Structures;
using GeoSlicer.Utils.BoundRing;
using NetTopologySuite.Geometries;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails.Algorithms;

internal static class NoIntersectRectangles
{
    internal static bool Connect(
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles,
        Cache cache,
        double epsilon)
    {
        bool flagAbcCanConnect = true;
        bool flagCdeCanConnect = true;
        bool flagEfgCanConnect = true;
        bool flagAhgCanConnect = true;
        Coordinate? firstCoordLineConnectNearAbc = null;
        Coordinate? secondCoordLineConnectNearAbc = null;
        Coordinate? firstCoordLineConnectNearCde = null;
        Coordinate? secondCoordLineConnectNearCde = null;
        Coordinate? firstCoordLineConnectNearEfg = null;
        Coordinate? secondCoordLineConnectNearEfg = null;
        Coordinate? firstCoordLineConnectNearAhg = null;
        Coordinate? secondCoordLineConnectNearAhg = null;

        if (cache.NearAbc is null)
            flagAbcCanConnect = false;
        else
        {
            bool flagA = false;
            bool flagB = false;
            bool flagC = false;
            firstCoordLineConnectNearAbc = thisRing.Value.PointUpNode.Elem;
            secondCoordLineConnectNearAbc = cache.NearAbc!.BoundRing.Value.PointDownNode.Elem;
            foreach (var zone in cache.NearAbc!.Zones)
            {
                if (!flagA && zone == SeparatingZones.A)
                    flagA = true;
                else if (!flagB && zone == SeparatingZones.B)
                    flagB = true;
                else flagC = true;
            }

            if (flagC)
            {
                foreach (var frame in cache.ListC)
                {
                    if (!frame.Zones.Contains(SeparatingZones.B)
                        && frame.BoundRing.Value.PointMin.Y < thisRing.Value.PointMax.Y
                        && IntersectsChecker.HasIntersectsBoundRFrame(frame.BoundRing.Value, firstCoordLineConnectNearAbc,
                            secondCoordLineConnectNearAbc) &&
                        !ReferenceEquals(frame.BoundRing.Value, cache.NearAbc.BoundRing.Value))
                    {
                        cache.NearAbcIntersect = frame;
                        flagAbcCanConnect = false;
                    }
                }
            }

            if (flagA && flagAbcCanConnect)
            {
                foreach (var frame in cache.ListA)
                {
                    if (!frame.Zones.Contains(SeparatingZones.B)
                        && frame.BoundRing.Value.PointMin.Y < thisRing.Value.PointMax.Y
                        && IntersectsChecker.HasIntersectsBoundRFrame(frame.BoundRing.Value, firstCoordLineConnectNearAbc,
                            secondCoordLineConnectNearAbc) &&
                        !ReferenceEquals(frame.BoundRing.Value, cache.NearAbc.BoundRing.Value))
                    {
                        cache.NearAbcIntersect = frame;
                        flagAbcCanConnect = false;
                    }
                }

            }
        }


        if (cache.NearCde is null)
            flagCdeCanConnect = false;
        else
        {
            bool flagC = false;
            bool flagD = false;
            bool flagE = false;
            firstCoordLineConnectNearCde = thisRing.Value.PointLeftNode.Elem;
            secondCoordLineConnectNearCde = cache.NearCde!.BoundRing.Value.PointRightNode.Elem;

            foreach (var zone in cache.NearCde!.Zones)
            {
                if (!flagC && zone == SeparatingZones.C)
                    flagC = true;
                else if (!flagD && zone == SeparatingZones.D)
                    flagD = true;
                else flagE = true;
            }

            if (flagC)
            {
                foreach (var frame in cache.ListC)
                {
                    if (!frame.Zones.Contains(SeparatingZones.D)
                        && frame.BoundRing.Value.PointMax.X > thisRing.Value.PointMin.X
                        && IntersectsChecker.HasIntersectsBoundRFrame(frame.BoundRing.Value, firstCoordLineConnectNearCde,
                            secondCoordLineConnectNearCde) &&
                        !ReferenceEquals(frame.BoundRing.Value, cache.NearCde.BoundRing.Value))
                    {
                        cache.NearCdeIntersect = frame;
                        flagCdeCanConnect = false;
                    }
                }

            }

            if (flagE && flagCdeCanConnect)
            {
                foreach (var frame in cache.ListE)
                {
                    if (!frame.Zones.Contains(SeparatingZones.D)
                        && frame.BoundRing.Value.PointMax.X > thisRing.Value.PointMin.X
                        && IntersectsChecker.HasIntersectsBoundRFrame(frame.BoundRing.Value, firstCoordLineConnectNearCde,
                            secondCoordLineConnectNearCde) &&
                        !ReferenceEquals(frame.BoundRing.Value, cache.NearCde.BoundRing.Value))
                    {
                        cache.NearCdeIntersect = frame;
                        flagCdeCanConnect = false;
                    }
                }

            }
        }



        if (cache.NearEfg is null)
            flagEfgCanConnect = false;
        else
        {
            bool flagE = false;
            bool flagF = false;
            bool flagG = false;
            firstCoordLineConnectNearEfg = thisRing.Value.PointDownNode.Elem;
            secondCoordLineConnectNearEfg = cache.NearEfg!.BoundRing.Value.PointUpNode.Elem;
            foreach (var zone in cache.NearEfg!.Zones)
            {
                if (!flagE && zone == SeparatingZones.E)
                    flagE = true;
                else if (!flagF && zone == SeparatingZones.F)
                    flagF = true;
                else flagG = true;
            }

            if (flagE)
            {
                foreach (var frame in cache.ListE)
                {
                    if (!frame.Zones.Contains(SeparatingZones.F)
                        && frame.BoundRing.Value.PointMax.Y > thisRing.Value.PointMin.Y
                        && IntersectsChecker.HasIntersectsBoundRFrame(frame.BoundRing.Value, firstCoordLineConnectNearEfg,
                            secondCoordLineConnectNearEfg) &&
                        !ReferenceEquals(frame.BoundRing.Value, cache.NearEfg.BoundRing.Value))
                    {
                        cache.NearEfgIntersect = frame;
                        flagEfgCanConnect = false;
                    }
                }
            }

            if (flagG && flagEfgCanConnect)
            {
                foreach (var frame in cache.ListG)
                {
                    if (!frame.Zones.Contains(SeparatingZones.F)
                        && frame.BoundRing.Value.PointMax.Y > thisRing.Value.PointMin.Y
                        && IntersectsChecker.HasIntersectsBoundRFrame(frame.BoundRing.Value, firstCoordLineConnectNearEfg,
                            secondCoordLineConnectNearEfg) &&
                        !ReferenceEquals(frame.BoundRing.Value, cache.NearEfg.BoundRing.Value))
                    {
                        cache.NearEfgIntersect = frame;
                        flagEfgCanConnect = false;
                    }
                }
            }
        }



        if (cache.NearAhg is null)
            flagAhgCanConnect = false;
        else
        {
            bool flagA = false;
            bool flagH = false;
            bool flagG = false;
            firstCoordLineConnectNearAhg = thisRing.Value.PointRightNode.Elem;
            secondCoordLineConnectNearAhg = cache.NearAhg!.BoundRing.Value.PointLeftNode.Elem;
            foreach (var zone in cache.NearAhg!.Zones)
            {
                if (!flagA && zone == SeparatingZones.A)
                    flagA = true;
                else if (!flagH && zone == SeparatingZones.H)
                    flagH = true;
                else flagG = true;
            }

            if (flagA)
            {
                foreach (var frame in cache.ListA)
                {
                    if (!frame.Zones.Contains(SeparatingZones.H)
                        && frame.BoundRing.Value.PointMin.X < thisRing.Value.PointMax.X
                        && IntersectsChecker.HasIntersectsBoundRFrame(frame.BoundRing.Value, firstCoordLineConnectNearAhg,
                            secondCoordLineConnectNearAhg) &&
                        !ReferenceEquals(frame.BoundRing.Value, cache.NearAhg.BoundRing.Value))
                    {
                        cache.NearAhgIntersect = frame;
                        flagAhgCanConnect = false;
                    }
                }
            }

            if (flagG && flagAhgCanConnect)
            {
                foreach (var frame in cache.ListG)
                {
                    if (!frame.Zones.Contains(SeparatingZones.H)
                        && frame.BoundRing.Value.PointMin.X < thisRing.Value.PointMax.X
                        && IntersectsChecker.HasIntersectsBoundRFrame(frame.BoundRing.Value, firstCoordLineConnectNearAhg,
                            secondCoordLineConnectNearAhg) &&
                        !ReferenceEquals(frame.BoundRing.Value, cache.NearAhg.BoundRing.Value))
                    {
                        cache.NearAhgIntersect = frame;
                        flagAhgCanConnect = false;
                    }
                }
            }
        }
        
        
        var shell = cache.FramesContainThis.First!.Value;
        cache.FramesContainThis.Remove(cache.FramesContainThis.First);
        cache.FramesContainThis.AddLast(shell);
        foreach (var frameWhoContainThis in cache.FramesContainThis)
        {
            var start = frameWhoContainThis.Value.PointUpNode;
            var buffer = start;
            do
            {
                if (flagAbcCanConnect &&
                    (buffer.Elem.Y > thisRing.Value.PointMax.Y
                     || Math.Abs(buffer.Elem.Y - thisRing.Value.PointMax.Y) < 1e-9
                     || buffer.Next.Elem.Y > thisRing.Value.PointMax.Y
                     || Math.Abs(buffer.Next.Elem.Y - thisRing.Value.PointMax.Y) < 1e-9))
                {
                    if (IntersectsChecker.HasIntersectedSegments(firstCoordLineConnectNearAbc!, secondCoordLineConnectNearAbc!, buffer.Elem,
                            buffer.Next.Elem))
                    {
                        cache.NearAbcSegmentIntersect = new RingAndPoint(frameWhoContainThis, buffer);
                        flagAbcCanConnect = false;

                    }
                }

                if (flagCdeCanConnect &&
                    (buffer.Elem.X < thisRing.Value.PointMin.X
                     || Math.Abs(buffer.Elem.X - thisRing.Value.PointMin.X) < 1e-9
                     || buffer.Next.Elem.X < thisRing.Value.PointMin.X
                     || Math.Abs(buffer.Next.Elem.X - thisRing.Value.PointMin.X) < 1e-9))
                {
                    if (IntersectsChecker.HasIntersectedSegments(firstCoordLineConnectNearCde!, secondCoordLineConnectNearCde!, buffer.Elem,
                            buffer.Next.Elem))
                    {
                        cache.NearCdeSegmentIntersect = new RingAndPoint(frameWhoContainThis, buffer);
                        flagCdeCanConnect = false;
                    }
                }

                if (flagEfgCanConnect &&
                    (buffer.Elem.Y < thisRing.Value.PointMin.Y
                     || Math.Abs(buffer.Elem.Y - thisRing.Value.PointMin.Y) < 1e-9
                     || buffer.Next.Elem.Y < thisRing.Value.PointMin.Y
                     || Math.Abs(buffer.Next.Elem.Y - thisRing.Value.PointMin.Y) < 1e-9))
                {
                    if (IntersectsChecker.HasIntersectedSegments(firstCoordLineConnectNearEfg!, secondCoordLineConnectNearEfg!, buffer.Elem,
                            buffer.Next.Elem))
                    {
                        cache.NearEfgSegmentIntersect = new RingAndPoint(frameWhoContainThis, buffer);
                        flagEfgCanConnect = false;
                    }
                }

                if (flagAhgCanConnect &&
                    (buffer.Elem.X > thisRing.Value.PointMax.X
                     || Math.Abs(buffer.Elem.X - thisRing.Value.PointMax.X) < 1e-9
                     || buffer.Next.Elem.X > thisRing.Value.PointMax.X
                     || Math.Abs(buffer.Next.Elem.X - thisRing.Value.PointMax.X) < 1e-9))
                {
                    if (IntersectsChecker.HasIntersectedSegments(firstCoordLineConnectNearAhg!, secondCoordLineConnectNearAhg!, buffer.Elem,
                            buffer.Next.Elem))
                    {
                        cache.NearAhgSegmentIntersect = new RingAndPoint(frameWhoContainThis, buffer);
                        flagAhgCanConnect = false;
                    }
                }

                buffer = buffer.Next;
            } while (!ReferenceEquals(buffer, start)
                     && (flagAbcCanConnect || flagCdeCanConnect || flagEfgCanConnect || flagAhgCanConnect));
        }

        Coordinate oldPointMin = thisRing.Value.PointMin;
        Coordinate oldPointMax = thisRing.Value.PointMax;
        if (flagAbcCanConnect)
        {
            thisRing.Value.ConnectBoundRings(
                cache.NearAbc!.BoundRing.Value,
                thisRing.Value.PointUpNode,
                cache.NearAbc.BoundRing.Value.PointDownNode);
            
            listOfHoles.Remove(cache.NearAbc.BoundRing);
        }

        if (flagCdeCanConnect && oldPointMin.Equals(thisRing.Value.PointMin))
        {
            thisRing.Value.ConnectBoundRings(
                cache.NearCde!.BoundRing.Value,
                thisRing.Value.PointLeftNode,
                cache.NearCde.BoundRing.Value.PointRightNode);

            listOfHoles.Remove(cache.NearCde.BoundRing);
        }

        if (flagEfgCanConnect && oldPointMin.Equals(thisRing.Value.PointMin))
        {
            thisRing.Value.ConnectBoundRings(
                cache.NearEfg!.BoundRing.Value,
                thisRing.Value.PointDownNode,
                cache.NearEfg.BoundRing.Value.PointUpNode);
            listOfHoles.Remove(cache.NearEfg.BoundRing);
        }

        if (flagAhgCanConnect && oldPointMax.Equals(thisRing.Value.PointMax))
        {
            thisRing.Value.ConnectBoundRings(
                cache.NearAhg!.BoundRing.Value,
                thisRing.Value.PointRightNode,
                cache.NearAhg.BoundRing.Value.PointLeftNode);
            listOfHoles.Remove(cache.NearAhg.BoundRing);
        }

        return flagAbcCanConnect || flagCdeCanConnect || flagEfgCanConnect || flagAhgCanConnect;
    }
}