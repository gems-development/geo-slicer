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

        if (!cache.NearRing.ContainsKey(Zones.Abc))
            flagAbcCanConnect = false;
        else
        {
            bool flagA = false;
            bool flagB = false;
            bool flagC = false;
            firstCoordLineConnectNearAbc = thisRing.Value.PointUpNode.Elem;
            secondCoordLineConnectNearAbc = cache.NearRing[Zones.Abc].BoundRing.Value.PointDownNode.Elem;
            foreach (var zone in cache.NearRing[Zones.Abc].Zones)
            {
                if (!flagA && zone == Zones.A)
                    flagA = true;
                else if (!flagB && zone == Zones.B)
                    flagB = true;
                else flagC = true;
            }

            if (flagC)
            {
                foreach (var frame in cache.RingsInZone[Zones.C])
                {
                    if (!frame.Zones.Contains(Zones.B)
                        && frame.BoundRing.Value.PointMin.Y < thisRing.Value.PointMax.Y
                        && IntersectsChecker.HasIntersectsBoundRFrame(frame.BoundRing.Value, firstCoordLineConnectNearAbc,
                            secondCoordLineConnectNearAbc) &&
                        !ReferenceEquals(frame.BoundRing.Value, cache.NearRing[Zones.Abc].BoundRing.Value))
                    {
                        flagAbcCanConnect = false;
                    }
                }
            }

            if (flagA && flagAbcCanConnect)
            {
                foreach (var frame in cache.RingsInZone[Zones.A])
                {
                    if (!frame.Zones.Contains(Zones.B)
                        && frame.BoundRing.Value.PointMin.Y < thisRing.Value.PointMax.Y
                        && IntersectsChecker.HasIntersectsBoundRFrame(frame.BoundRing.Value, firstCoordLineConnectNearAbc,
                            secondCoordLineConnectNearAbc) &&
                        !ReferenceEquals(frame.BoundRing.Value, cache.NearRing[Zones.Abc].BoundRing.Value))
                    {
                        flagAbcCanConnect = false;
                    }
                }

            }
        }


        if (!cache.NearRing.ContainsKey(Zones.Cde))
            flagCdeCanConnect = false;
        else
        {
            bool flagC = false;
            bool flagD = false;
            bool flagE = false;
            firstCoordLineConnectNearCde = thisRing.Value.PointLeftNode.Elem;
            secondCoordLineConnectNearCde = cache.NearRing[Zones.Cde].BoundRing.Value.PointRightNode.Elem;

            foreach (var zone in cache.NearRing[Zones.Cde].Zones)
            {
                if (!flagC && zone == Zones.C)
                    flagC = true;
                else if (!flagD && zone == Zones.D)
                    flagD = true;
                else flagE = true;
            }

            if (flagC)
            {
                foreach (var frame in cache.RingsInZone[Zones.C])
                {
                    if (!frame.Zones.Contains(Zones.D)
                        && frame.BoundRing.Value.PointMax.X > thisRing.Value.PointMin.X
                        && IntersectsChecker.HasIntersectsBoundRFrame(frame.BoundRing.Value, firstCoordLineConnectNearCde,
                            secondCoordLineConnectNearCde) &&
                        !ReferenceEquals(frame.BoundRing.Value, cache.NearRing[Zones.Cde].BoundRing.Value))
                    {
                        flagCdeCanConnect = false;
                    }
                }

            }

            if (flagE && flagCdeCanConnect)
            {
                foreach (var frame in cache.RingsInZone[Zones.E])
                {
                    if (!frame.Zones.Contains(Zones.D)
                        && frame.BoundRing.Value.PointMax.X > thisRing.Value.PointMin.X
                        && IntersectsChecker.HasIntersectsBoundRFrame(frame.BoundRing.Value, firstCoordLineConnectNearCde,
                            secondCoordLineConnectNearCde) &&
                        !ReferenceEquals(frame.BoundRing.Value, cache.NearRing[Zones.Cde].BoundRing.Value))
                    {
                        flagCdeCanConnect = false;
                    }
                }

            }
        }



        if (!cache.NearRing.ContainsKey(Zones.Efg))
            flagEfgCanConnect = false;
        else
        {
            bool flagE = false;
            bool flagF = false;
            bool flagG = false;
            firstCoordLineConnectNearEfg = thisRing.Value.PointDownNode.Elem;
            secondCoordLineConnectNearEfg = cache.NearRing[Zones.Efg].BoundRing.Value.PointUpNode.Elem;
            foreach (var zone in cache.NearRing[Zones.Efg].Zones)
            {
                if (!flagE && zone == Zones.E)
                    flagE = true;
                else if (!flagF && zone == Zones.F)
                    flagF = true;
                else flagG = true;
            }

            if (flagE)
            {
                foreach (var frame in cache.RingsInZone[Zones.E])
                {
                    if (!frame.Zones.Contains(Zones.F)
                        && frame.BoundRing.Value.PointMax.Y > thisRing.Value.PointMin.Y
                        && IntersectsChecker.HasIntersectsBoundRFrame(frame.BoundRing.Value, firstCoordLineConnectNearEfg,
                            secondCoordLineConnectNearEfg) &&
                        !ReferenceEquals(frame.BoundRing.Value, cache.NearRing[Zones.Efg].BoundRing.Value))
                    {
                        flagEfgCanConnect = false;
                    }
                }
            }

            if (flagG && flagEfgCanConnect)
            {
                foreach (var frame in cache.RingsInZone[Zones.G])
                {
                    if (!frame.Zones.Contains(Zones.F)
                        && frame.BoundRing.Value.PointMax.Y > thisRing.Value.PointMin.Y
                        && IntersectsChecker.HasIntersectsBoundRFrame(frame.BoundRing.Value, firstCoordLineConnectNearEfg,
                            secondCoordLineConnectNearEfg) &&
                        !ReferenceEquals(frame.BoundRing.Value, cache.NearRing[Zones.Efg].BoundRing.Value))
                    {
                        flagEfgCanConnect = false;
                    }
                }
            }
        }



        if (!cache.NearRing.ContainsKey(Zones.Ahg))
            flagAhgCanConnect = false;
        else
        {
            bool flagA = false;
            bool flagH = false;
            bool flagG = false;
            firstCoordLineConnectNearAhg = thisRing.Value.PointRightNode.Elem;
            secondCoordLineConnectNearAhg = cache.NearRing[Zones.Ahg].BoundRing.Value.PointLeftNode.Elem;
            foreach (var zone in cache.NearRing[Zones.Ahg].Zones)
            {
                if (!flagA && zone == Zones.A)
                    flagA = true;
                else if (!flagH && zone == Zones.H)
                    flagH = true;
                else flagG = true;
            }

            if (flagA)
            {
                foreach (var frame in cache.RingsInZone[Zones.A])
                {
                    if (!frame.Zones.Contains(Zones.H)
                        && frame.BoundRing.Value.PointMin.X < thisRing.Value.PointMax.X
                        && IntersectsChecker.HasIntersectsBoundRFrame(frame.BoundRing.Value, firstCoordLineConnectNearAhg,
                            secondCoordLineConnectNearAhg) &&
                        !ReferenceEquals(frame.BoundRing.Value, cache.NearRing[Zones.Ahg].BoundRing.Value))
                    {
                        flagAhgCanConnect = false;
                    }
                }
            }

            if (flagG && flagAhgCanConnect)
            {
                foreach (var frame in cache.RingsInZone[Zones.G])
                {
                    if (!frame.Zones.Contains(Zones.H)
                        && frame.BoundRing.Value.PointMin.X < thisRing.Value.PointMax.X
                        && IntersectsChecker.HasIntersectsBoundRFrame(frame.BoundRing.Value, firstCoordLineConnectNearAhg,
                            secondCoordLineConnectNearAhg) &&
                        !ReferenceEquals(frame.BoundRing.Value, cache.NearRing[Zones.Ahg].BoundRing.Value))
                    {
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
                        cache.NearSegmentIntersect.Remove(Zones.Abc);
                        cache.NearSegmentIntersect.Add(Zones.Abc, new RingAndPoint(frameWhoContainThis, buffer));
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
                        cache.NearSegmentIntersect.Remove(Zones.Cde);
                        cache.NearSegmentIntersect.Add(Zones.Cde, new RingAndPoint(frameWhoContainThis, buffer));
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
                        cache.NearSegmentIntersect.Remove(Zones.Efg);
                        cache.NearSegmentIntersect.Add(Zones.Efg, new RingAndPoint(frameWhoContainThis, buffer));
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
                        cache.NearSegmentIntersect.Remove(Zones.Ahg);
                        cache.NearSegmentIntersect.Add(Zones.Ahg, new RingAndPoint(frameWhoContainThis, buffer));
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
                cache.NearRing[Zones.Abc].BoundRing.Value,
                thisRing.Value.PointUpNode,
                cache.NearRing[Zones.Abc].BoundRing.Value.PointDownNode);
            
            listOfHoles.Remove(cache.NearRing[Zones.Abc].BoundRing);
        }

        if (flagCdeCanConnect && oldPointMin.Equals(thisRing.Value.PointMin))
        {
            thisRing.Value.ConnectBoundRings(
                cache.NearRing[Zones.Cde].BoundRing.Value,
                thisRing.Value.PointLeftNode,
                cache.NearRing[Zones.Cde].BoundRing.Value.PointRightNode);

            listOfHoles.Remove(cache.NearRing[Zones.Cde].BoundRing);
        }

        if (flagEfgCanConnect && oldPointMin.Equals(thisRing.Value.PointMin))
        {
            thisRing.Value.ConnectBoundRings(
                cache.NearRing[Zones.Efg].BoundRing.Value,
                thisRing.Value.PointDownNode,
                cache.NearRing[Zones.Efg].BoundRing.Value.PointUpNode);
            listOfHoles.Remove(cache.NearRing[Zones.Efg].BoundRing);
        }

        if (flagAhgCanConnect && oldPointMax.Equals(thisRing.Value.PointMax))
        {
            thisRing.Value.ConnectBoundRings(
                cache.NearRing[Zones.Ahg].BoundRing.Value,
                thisRing.Value.PointRightNode,
                cache.NearRing[Zones.Ahg].BoundRing.Value.PointLeftNode);
            listOfHoles.Remove(cache.NearRing[Zones.Ahg].BoundRing);
        }

        return flagAbcCanConnect || flagCdeCanConnect || flagEfgCanConnect || flagAhgCanConnect;
    }
}