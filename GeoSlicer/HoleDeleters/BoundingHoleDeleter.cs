using System;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.HoleDeleters.BoundHoleDelDetails;
using GeoSlicer.Utils;
using NetTopologySuite.Geometries;
using GeoSlicer.Utils.BoundRing;
using GeoSlicer.HoleDeleters.BoundHoleDelDetails.Structures;

namespace GeoSlicer.HoleDeleters;

public class BoundingHoleDeleter
{
    private readonly TraverseDirection _direction;
    private readonly Cache _cache;
    private readonly double _epsilon;

    public BoundingHoleDeleter(TraverseDirection direction, double epsilon)
    {
        _direction = direction;
        _cache = new Cache(epsilon);
        _epsilon = epsilon;
    }

    public Polygon DeleteHoles(Polygon polygon)
    {
        LinkedList<BoundingRing> list = BoundingRing.PolygonToBoundRings(polygon, _direction);
        DeleteHoles(list);
        return BoundingRing.BoundRingsToPolygon(list);
    }
    private void DeleteHoles(LinkedList<BoundingRing> listOfHoles)
    {
        var thisRing = listOfHoles.First;
        var pointMinShell = thisRing!.Value.PointMin;
        var pointMaxShell = thisRing.Value.PointMax;
        
        while (listOfHoles.First!.Next is not null)
        { 
            if (thisRing.Next is null)
                thisRing = listOfHoles.First.Next;
            else thisRing = thisRing.Next;
            
            bool isConnected;
            
            if (!_cache.FillListsRelativeRing(thisRing, listOfHoles))
            {
                isConnected = 
                    ConnectNoIntersectRectan(thisRing, listOfHoles, _cache) ||
                    BruteforceConnectWithIntersectRing(thisRing, listOfHoles, _cache);
            }
            else
            {
                isConnected = BruteforceConnectIntersectionBoundRFrames(thisRing, listOfHoles, _cache);
            }

            if (!isConnected)
            {
                BruteforceConnect(thisRing, listOfHoles, _cache);
            }
            
            if (thisRing.Value.PointMin.Equals(pointMinShell) && thisRing.Value.PointMax.Equals(pointMaxShell))
            {
                var buff = thisRing.Value;
                listOfHoles.Remove(thisRing);
                listOfHoles.AddFirst(buff);
                thisRing = listOfHoles.First;
            }
        }
    }
    

    private bool ConnectNoIntersectRectan(
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles,
        Cache cache)
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
    private bool BruteforceConnectIntersectionBoundRFrames(
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles,
        Cache cache)
    {
        var currentFrameNode = cache.IntersectFrames.First;
        do
        {
            while (currentFrameNode is not null)
            {
                if (IntersectsChecker.IntersectOrContainFrames(currentFrameNode.Value.Value, thisRing.Value))
                {
                    break;
                }

                currentFrameNode = currentFrameNode.Next;
            }

            if (currentFrameNode is not null)
            {
                var currentFrame = currentFrameNode.Value;
                var startThisRing = thisRing.Value.Ring;
                var startCurrentFrame = currentFrame.Value.Ring;
                bool flagFirstCycle = false;
                bool flagSecondCycle = false;
                IntersectsChecker.GetIntersectionBoundRFrames(
                    thisRing.Value, 
                    currentFrame.Value,
                    out var framesIntersectionPointMin,
                    out var framesIntersectionPointMax);
                do
                {
                    if (!flagFirstCycle)
                    {
                        do
                        {
                            if (IntersectsChecker.PointInsideFrameCheck(
                                    startThisRing.Elem, framesIntersectionPointMin, framesIntersectionPointMax))
                            {
                                flagFirstCycle = true;
                                break;
                            }

                            startThisRing = startThisRing.Next;
                        } while (!ReferenceEquals(startThisRing, thisRing.Value.Ring));
                    }

                    if (!flagSecondCycle)
                    {
                        do
                        {
                            if (IntersectsChecker.PointInsideFrameCheck(
                                    startCurrentFrame.Elem, framesIntersectionPointMin, framesIntersectionPointMax))
                            {
                                flagSecondCycle = true;
                                break;
                            }

                            startCurrentFrame = startCurrentFrame.Next;
                        } while (!ReferenceEquals(startCurrentFrame, currentFrame.Value.Ring));
                    }

                    if (flagFirstCycle && flagSecondCycle)
                    {
                        if (IntersectsChecker.IntersectRingWithSegmentNotExtPoints(thisRing, startThisRing.Elem,
                                startCurrentFrame.Elem))
                        {
                            flagFirstCycle = false;
                            startThisRing = startThisRing.Next;
                        }

                        if (IntersectsChecker.IntersectRingWithSegmentNotExtPoints(currentFrameNode.Value,
                                startThisRing.Elem,
                                startCurrentFrame.Elem))
                        {
                            flagSecondCycle = false;
                            startCurrentFrame = startCurrentFrame.Next;
                        }

                        if (flagFirstCycle && flagSecondCycle)
                        {
                            foreach (var frame in cache.IntersectFrames)
                            {
                                if (!ReferenceEquals(currentFrameNode.Value, frame))
                                {
                                    if (IntersectsChecker.HasIntersectsBoundRFrame
                                            (frame.Value, startThisRing.Elem, startCurrentFrame.Elem)

                                        || IntersectsChecker.PointInsideBoundRFrame
                                            (startThisRing.Elem, frame.Value)

                                        || IntersectsChecker.PointInsideBoundRFrame
                                            (startCurrentFrame.Elem, frame.Value))
                                    {
                                        if (IntersectsChecker.IntersectBoundRingWithSegment(frame, startThisRing.Elem,
                                                startCurrentFrame.Elem))
                                        {
                                            flagFirstCycle = false;
                                            flagSecondCycle = false;
                                            startThisRing = startThisRing.Next;
                                            startCurrentFrame = startCurrentFrame.Next;
                                        }
                                    }
                                }
                            }
                        }

                        if (flagFirstCycle && flagSecondCycle)
                        {
                            foreach (var frame in cache.FramesContainThis)
                            {
                                if (IntersectsChecker.IntersectBoundRingWithSegment(frame, startThisRing.Elem,
                                        startCurrentFrame.Elem))
                                {
                                    flagFirstCycle = false;
                                    flagSecondCycle = false;
                                    startThisRing = startThisRing.Next;
                                    startCurrentFrame = startCurrentFrame.Next;
                                }
                            }

                            if (flagFirstCycle && flagSecondCycle)
                            {
                                thisRing.Value.ConnectBoundRings(
                                    currentFrame.Value,
                                    startThisRing,
                                    startCurrentFrame);

                                listOfHoles.Remove(currentFrame);
                                return true;
                            }
                        }
                    }
                } while (!ReferenceEquals(startThisRing, thisRing.Value.Ring) &&

                         !ReferenceEquals(startCurrentFrame, currentFrame.Value.Ring));

                currentFrameNode = currentFrameNode.Next;
            }
        } while (currentFrameNode is not null);

        return false;
    }
    //todo нахождение прямоугольника, соединение с которым не пересекает другие 
    private bool BruteforceConnectWithIntersectRing(
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles,
        Cache cache)
    {
        
        /*if (_nearABC is not null)
        {
            if (_nearABCintersect is not null)
            {
                var listAbc =
                    new LinkedList<
                        (LinkedListNode<BoundingRing> boundRing,
                        List<PartitioningZones> zones)>();
                int listAbcCount = 0;
                foreach (var frame in _listA)
                {
                    listAbc.AddFirst(frame);
                    listAbcCount++;
                }
                foreach (var frame in _listB)
                {
                    listAbc.AddFirst(frame);
                    listAbcCount++;
                }
                foreach (var frame in _listC)
                {
                    listAbc.AddFirst(frame);
                    listAbcCount++;
                }
                LinkedListNode<BoundingRing>? testingRing = null;
                foreach (var frame in listAbc)
                {
                    if (!(frame.boundRing.Value.PointMin.Y < thisRing.Value.PointMax.Y))
                    {
                        testingRing = frame.boundRing;
                        break;
                    }
                }
                
                for (int i = 0; i < listAbcCount; i++)
                {
                    var res = CheckIntersectFramesWithoutThisFrame(listAbc, thisRing.Value.PointUpNode.Elem,
                        testingRing.Value.PointDownNode.Elem, testingRing);
                    if (res is null)
                        break;
                    testingRing = res;
                }
            }
        }*/
        if (cache.NearAbcSegmentIntersect is not null)
        {
            bool flag = ConnectWithBoundRFrameWhoContainThisRing(cache.NearAbcSegmentIntersect, thisRing, listOfHoles, SeparatingZones.ABC, cache);
            if (flag)
                return true;
        }
        if (cache.NearCdeSegmentIntersect is not null)
        {
            bool flag = ConnectWithBoundRFrameWhoContainThisRing(cache.NearCdeSegmentIntersect, thisRing, listOfHoles, SeparatingZones.CDE, cache);
            if (flag)
                return true;
        }
        if (cache.NearEfgSegmentIntersect is not null)
        {
            bool flag = ConnectWithBoundRFrameWhoContainThisRing(cache.NearEfgSegmentIntersect, thisRing, listOfHoles, SeparatingZones.EFG, cache);
            if (flag)
                return true;
        }
        if (cache.NearAhgSegmentIntersect is not null)
        {
            bool flag = ConnectWithBoundRFrameWhoContainThisRing(cache.NearAhgSegmentIntersect, thisRing, listOfHoles, SeparatingZones.AHG, cache);
            if (flag)
                return true;
        }

        return false;
    }
    
    
    
    //todo добавить проверку на пересечение соединения с прямоугольниками
    //todo рассмотреть ситуацию когда все ближайшие треугольники равны null (_nearAbc и другие подобные)
    private bool ConnectWithBoundRFrameWhoContainThisRing(
        RingAndPoint? nearSegmentIntersect,
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles, 
        SeparatingZones zones, 
        Cache cache)
    {
        var coord = nearSegmentIntersect!.Start;
        coord = RearrangePoints(coord, zones, thisRing);
        //todo изменить nearSegmentIntersect так чтобы он содержал узел в списке _framesContainThis
        var startFrameContainThis = cache.FramesContainThis.First;
        do
        {
            if (ReferenceEquals(startFrameContainThis!.Value.Value, nearSegmentIntersect.BoundRing.Value))
            {
                var buff = startFrameContainThis.Value;
                cache.FramesContainThis.Remove(startFrameContainThis);
                cache.FramesContainThis.AddFirst(buff);
                break;
            }

            startFrameContainThis = startFrameContainThis.Next;
        } while (startFrameContainThis is not null);

        LinkedNode<Coordinate> connectCoordThisR;
        if (zones == SeparatingZones.ABC)
            connectCoordThisR = thisRing.Value.PointUpNode;
        else if (zones == SeparatingZones.CDE)
            connectCoordThisR = thisRing.Value.PointLeftNode;
        else if (zones == SeparatingZones.EFG)
            connectCoordThisR = thisRing.Value.PointDownNode;
        else
            connectCoordThisR = thisRing.Value.PointRightNode;
        
        
        bool flag;
        var correctNode = cache.FramesContainThis.First!.Value;
        do
        {
            flag = false;
            foreach (var frame in cache.FramesContainThis)
            {
                (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? intersectSegment;
                do
                {
                    intersectSegment =
                        IntersectsChecker.GetIntersectRingWithSegmentNotExtPoint(frame, connectCoordThisR.Elem, coord.Elem);
                    if (intersectSegment is not null)
                    {
                        flag = true;
                        coord = intersectSegment.Value._start;
                        coord = RearrangePoints(coord, zones, thisRing);

                        correctNode = frame;
                    }
                    
                } while (intersectSegment is not null);
                
            }
            
        } while (flag);

        thisRing.Value.ConnectBoundRings(correctNode.Value,
            connectCoordThisR, coord);
        listOfHoles.Remove(correctNode);
        return true;
    }
    
    
    
    private LinkedNode<Coordinate> RearrangePoints(
        LinkedNode<Coordinate> coord,
        SeparatingZones zones,
        LinkedListNode<BoundingRing> thisRing)
    {
        if (zones == SeparatingZones.ABC)
        {
            if (coord.Elem.Y < thisRing.Value.PointUpNode.Elem.Y)
            {
                coord = coord.Next;
            }
        }
        else if (zones == SeparatingZones.CDE)
        {
            if (coord.Elem.X > thisRing.Value.PointLeftNode.Elem.X)
            {
                coord = coord.Next;
            }
        }
        else if (zones == SeparatingZones.EFG)
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
    
    
    private void BruteforceConnect(
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles,
        Cache cache)
    {
        LinkedListNode<BoundingRing> connectedFrame;
        LinkedNode<Coordinate> connectedPoint;
        //todo сделать объединение этих спискков без повторяющихся элементов
        var collectionABC = cache.ListA.Concat(cache.ListB).Concat(cache.ListC);
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

            
        