using System;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.HoleDeleters.BoundHoleDelDetails;
using GeoSlicer.Utils;
using NetTopologySuite.Geometries;
using GeoSlicer.Utils.BoundRing;

namespace GeoSlicer.HoleDeleters;

public class BoundingHoleDeleter
{
    private readonly TraverseDirection _direction;
    private readonly PartitionBoundRingsCache _cache;

    public BoundingHoleDeleter(TraverseDirection direction)
    {
        _direction = direction;
        _cache = new PartitionBoundRingsCache();
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


        //int count = listOfHoles.Count;
        
        while (listOfHoles.First!.Next is not null)
        {
            /*i++;
            if (count != listOfHoles.Count)
            {
                count = listOfHoles.Count;
                i = 0;
            }
            else if (i == count) return;*/
            if (_cache.FramesContainThis.Any())
            {
                LinkedList<BoundingRing> list = new LinkedList<BoundingRing>();
                foreach (var ring in _cache.FramesContainThis)
                {
                    list.AddLast(ring.Value);
                }
                
            }
            
            if (thisRing.Next is null)
                thisRing = listOfHoles.First.Next;
            else thisRing = thisRing.Next;
            bool hasIntersectFrames = _cache.FillListsRelativeRing(thisRing, listOfHoles);
            bool isConnected = false;
            if (!hasIntersectFrames)
            {
                bool frameOfThisChanged = false;
                /*if (cache.IntersectFrames.Any())
                {
                    //frameOfThisChanged = ConnectContainsRingsInThis(thisRing);
                }*/

                if (frameOfThisChanged)
                {
                    if (thisRing.Value.PointMin.Equals(pointMinShell) && thisRing.Value.PointMax.Equals(pointMaxShell))
                    {
                        var buff = thisRing.Value;
                        listOfHoles.Remove(thisRing);
                        listOfHoles.AddFirst(buff);
                        thisRing = listOfHoles.First;
                    }
                }
                else
                {

                    if (!ConnectNoIntersectRectan(thisRing, listOfHoles, _cache))
                    {
                        isConnected = BruteforceConnectWithIntersectRing(thisRing, listOfHoles, _cache);
                    }
                    else isConnected = true;

                    if (thisRing.Value.PointMin.Equals(pointMinShell) && thisRing.Value.PointMax.Equals(pointMaxShell))
                    {
                        var buff = thisRing.Value;
                        listOfHoles.Remove(thisRing);
                        listOfHoles.AddFirst(buff);
                        thisRing = listOfHoles.First;
                    }
                }
            }
            else
            {
                isConnected = BruteforceConnectIntersectionBoundRFrames(thisRing, listOfHoles, _cache);
                if (thisRing.Value.PointMin.Equals(pointMinShell) && thisRing.Value.PointMax.Equals(pointMaxShell))
                {
                    var buff = thisRing.Value;
                    listOfHoles.Remove(thisRing);
                    listOfHoles.AddFirst(buff);
                    thisRing = listOfHoles.First;
                }
            }

            if (!isConnected)
            {
                BruteforceConnect(thisRing, listOfHoles, _cache);
                if (thisRing.Value.PointMin.Equals(pointMinShell) && thisRing.Value.PointMax.Equals(pointMaxShell))
                {
                    var buff = thisRing.Value;
                    listOfHoles.Remove(thisRing);
                    listOfHoles.AddFirst(buff);
                    thisRing = listOfHoles.First;
                }
            }
                
        }
    }
    

    private bool ConnectNoIntersectRectan(
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles,
        PartitionBoundRingsCache cache)
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

        if (cache.NearABC is null)
            flagAbcCanConnect = false;
        else
        {
            bool flagA = false;
            bool flagB = false;
            bool flagC = false;
            firstCoordLineConnectNearAbc = thisRing.Value.PointUpNode.Elem;
            secondCoordLineConnectNearAbc = cache.NearABC!.Value.boundRing.Value.PointDownNode.Elem;
            foreach (var zone in cache.NearABC!.Value.zones)
            {
                if (!flagA && zone == PartitioningZones.A)
                    flagA = true;
                else if (!flagB && zone == PartitioningZones.B)
                    flagB = true;
                else flagC = true;
            }

            if (flagC)
            {
                foreach (var frame in cache.ListC)
                {
                    if (!frame.zones.Contains(PartitioningZones.B)
                        && frame.boundRing.Value.PointMin.Y < thisRing.Value.PointMax.Y
                        && BoundRIntersectsChecker.HasIntersectsBoundRFrame(frame.boundRing.Value, firstCoordLineConnectNearAbc,
                            secondCoordLineConnectNearAbc) &&
                        !ReferenceEquals(frame.boundRing.Value, cache.NearABC.Value.boundRing.Value))
                    {
                        cache.NearABCintersect = frame;
                        flagAbcCanConnect = false;
                    }
                }
            }

            if (flagA && flagAbcCanConnect)
            {
                foreach (var frame in cache.ListA)
                {
                    if (!frame.zones.Contains(PartitioningZones.B)
                        && frame.boundRing.Value.PointMin.Y < thisRing.Value.PointMax.Y
                        && BoundRIntersectsChecker.HasIntersectsBoundRFrame(frame.boundRing.Value, firstCoordLineConnectNearAbc,
                            secondCoordLineConnectNearAbc) &&
                        !ReferenceEquals(frame.boundRing.Value, cache.NearABC.Value.boundRing.Value))
                    {
                        cache.NearABCintersect = frame;
                        flagAbcCanConnect = false;
                    }
                }

            }
        }


        if (cache.NearCDE is null)
            flagCdeCanConnect = false;
        else
        {
            bool flagC = false;
            bool flagD = false;
            bool flagE = false;
            firstCoordLineConnectNearCde = thisRing.Value.PointLeftNode.Elem;
            secondCoordLineConnectNearCde = cache.NearCDE!.Value.boundRing.Value.PointRightNode.Elem;

            foreach (var zone in cache.NearCDE!.Value.zones)
            {
                if (!flagC && zone == PartitioningZones.C)
                    flagC = true;
                else if (!flagD && zone == PartitioningZones.D)
                    flagD = true;
                else flagE = true;
            }

            if (flagC)
            {
                foreach (var frame in cache.ListC)
                {
                    if (!frame.zones.Contains(PartitioningZones.D)
                        && frame.boundRing.Value.PointMax.X > thisRing.Value.PointMin.X
                        && BoundRIntersectsChecker.HasIntersectsBoundRFrame(frame.boundRing.Value, firstCoordLineConnectNearCde,
                            secondCoordLineConnectNearCde) &&
                        !ReferenceEquals(frame.boundRing.Value, cache.NearCDE.Value.boundRing.Value))
                    {
                        cache.NearCDEintersect = frame;
                        flagCdeCanConnect = false;
                    }
                }

            }

            if (flagE && flagCdeCanConnect)
            {
                foreach (var frame in cache.ListE)
                {
                    if (!frame.zones.Contains(PartitioningZones.D)
                        && frame.boundRing.Value.PointMax.X > thisRing.Value.PointMin.X
                        && BoundRIntersectsChecker.HasIntersectsBoundRFrame(frame.boundRing.Value, firstCoordLineConnectNearCde,
                            secondCoordLineConnectNearCde) &&
                        !ReferenceEquals(frame.boundRing.Value, cache.NearCDE.Value.boundRing.Value))
                    {
                        cache.NearCDEintersect = frame;
                        flagCdeCanConnect = false;
                    }
                }

            }
        }



        if (cache.NearEFG is null)
            flagEfgCanConnect = false;
        else
        {
            bool flagE = false;
            bool flagF = false;
            bool flagG = false;
            firstCoordLineConnectNearEfg = thisRing.Value.PointDownNode.Elem;
            secondCoordLineConnectNearEfg = cache.NearEFG!.Value.boundRing.Value.PointUpNode.Elem;
            foreach (var zone in cache.NearEFG!.Value.zones)
            {
                if (!flagE && zone == PartitioningZones.E)
                    flagE = true;
                else if (!flagF && zone == PartitioningZones.F)
                    flagF = true;
                else flagG = true;
            }

            if (flagE)
            {
                foreach (var frame in cache.ListE)
                {
                    if (!frame.zones.Contains(PartitioningZones.F)
                        && frame.boundRing.Value.PointMax.Y > thisRing.Value.PointMin.Y
                        && BoundRIntersectsChecker.HasIntersectsBoundRFrame(frame.boundRing.Value, firstCoordLineConnectNearEfg,
                            secondCoordLineConnectNearEfg) &&
                        !ReferenceEquals(frame.boundRing.Value, cache.NearEFG.Value.boundRing.Value))
                    {
                        cache.NearEFGintersect = frame;
                        flagEfgCanConnect = false;
                    }
                }
            }

            if (flagG && flagEfgCanConnect)
            {
                foreach (var frame in cache.ListG)
                {
                    if (!frame.zones.Contains(PartitioningZones.F)
                        && frame.boundRing.Value.PointMax.Y > thisRing.Value.PointMin.Y
                        && BoundRIntersectsChecker.HasIntersectsBoundRFrame(frame.boundRing.Value, firstCoordLineConnectNearEfg,
                            secondCoordLineConnectNearEfg) &&
                        !ReferenceEquals(frame.boundRing.Value, cache.NearEFG.Value.boundRing.Value))
                    {
                        cache.NearEFGintersect = frame;
                        flagEfgCanConnect = false;
                    }
                }
            }
        }



        if (cache.NearAHG is null)
            flagAhgCanConnect = false;
        else
        {
            bool flagA = false;
            bool flagH = false;
            bool flagG = false;
            firstCoordLineConnectNearAhg = thisRing.Value.PointRightNode.Elem;
            secondCoordLineConnectNearAhg = cache.NearAHG!.Value.boundRing.Value.PointLeftNode.Elem;
            foreach (var zone in cache.NearAHG!.Value.zones)
            {
                if (!flagA && zone == PartitioningZones.A)
                    flagA = true;
                else if (!flagH && zone == PartitioningZones.H)
                    flagH = true;
                else flagG = true;
            }

            if (flagA)
            {
                foreach (var frame in cache.ListA)
                {
                    if (!frame.zones.Contains(PartitioningZones.H)
                        && frame.boundRing.Value.PointMin.X < thisRing.Value.PointMax.X
                        && BoundRIntersectsChecker.HasIntersectsBoundRFrame(frame.boundRing.Value, firstCoordLineConnectNearAhg,
                            secondCoordLineConnectNearAhg) &&
                        !ReferenceEquals(frame.boundRing.Value, cache.NearAHG.Value.boundRing.Value))
                    {
                        cache.NearAHGintersect = frame;
                        flagAhgCanConnect = false;
                    }
                }
            }

            if (flagG && flagAhgCanConnect)
            {
                foreach (var frame in cache.ListG)
                {
                    if (!frame.zones.Contains(PartitioningZones.H)
                        && frame.boundRing.Value.PointMin.X < thisRing.Value.PointMax.X
                        && BoundRIntersectsChecker.HasIntersectsBoundRFrame(frame.boundRing.Value, firstCoordLineConnectNearAhg,
                            secondCoordLineConnectNearAhg) &&
                        !ReferenceEquals(frame.boundRing.Value, cache.NearAHG.Value.boundRing.Value))
                    {
                        cache.NearAHGintersect = frame;
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
                    if (BoundRIntersectsChecker.HasIntersectedSegments(firstCoordLineConnectNearAbc!, secondCoordLineConnectNearAbc!, buffer.Elem,
                            buffer.Next.Elem))
                    {
                        cache.NearABCSegmentintersect = (frameWhoContainThis, buffer);
                        flagAbcCanConnect = false;

                    }
                }

                if (flagCdeCanConnect &&
                    (buffer.Elem.X < thisRing.Value.PointMin.X
                     || Math.Abs(buffer.Elem.X - thisRing.Value.PointMin.X) < 1e-9
                     || buffer.Next.Elem.X < thisRing.Value.PointMin.X
                     || Math.Abs(buffer.Next.Elem.X - thisRing.Value.PointMin.X) < 1e-9))
                {
                    if (BoundRIntersectsChecker.HasIntersectedSegments(firstCoordLineConnectNearCde!, secondCoordLineConnectNearCde!, buffer.Elem,
                            buffer.Next.Elem))
                    {
                        cache.NearCDESegmentintersect = (frameWhoContainThis, buffer);
                        flagCdeCanConnect = false;
                    }
                }

                if (flagEfgCanConnect &&
                    (buffer.Elem.Y < thisRing.Value.PointMin.Y
                     || Math.Abs(buffer.Elem.Y - thisRing.Value.PointMin.Y) < 1e-9
                     || buffer.Next.Elem.Y < thisRing.Value.PointMin.Y
                     || Math.Abs(buffer.Next.Elem.Y - thisRing.Value.PointMin.Y) < 1e-9))
                {
                    if (BoundRIntersectsChecker.HasIntersectedSegments(firstCoordLineConnectNearEfg!, secondCoordLineConnectNearEfg!, buffer.Elem,
                            buffer.Next.Elem))
                    {
                        cache.NearEFGSegmentintersect = (frameWhoContainThis, buffer);
                        flagEfgCanConnect = false;
                    }
                }

                if (flagAhgCanConnect &&
                    (buffer.Elem.X > thisRing.Value.PointMax.X
                     || Math.Abs(buffer.Elem.X - thisRing.Value.PointMax.X) < 1e-9
                     || buffer.Next.Elem.X > thisRing.Value.PointMax.X
                     || Math.Abs(buffer.Next.Elem.X - thisRing.Value.PointMax.X) < 1e-9))
                {
                    if (BoundRIntersectsChecker.HasIntersectedSegments(firstCoordLineConnectNearAhg!, secondCoordLineConnectNearAhg!, buffer.Elem,
                            buffer.Next.Elem))
                    {
                        cache.NearAHGSegmentintersect = (frameWhoContainThis, buffer);
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
                cache.NearABC!.Value.boundRing.Value,
                thisRing.Value.PointUpNode,
                cache.NearABC.Value.boundRing.Value.PointDownNode);
            
            listOfHoles.Remove(cache.NearABC.Value.boundRing);
        }

        if (flagCdeCanConnect && oldPointMin.Equals(thisRing.Value.PointMin))
        {
            thisRing.Value.ConnectBoundRings(
                cache.NearCDE!.Value.boundRing.Value,
                thisRing.Value.PointLeftNode,
                cache.NearCDE.Value.boundRing.Value.PointRightNode);

            listOfHoles.Remove(cache.NearCDE.Value.boundRing);
        }

        if (flagEfgCanConnect && oldPointMin.Equals(thisRing.Value.PointMin))
        {
            thisRing.Value.ConnectBoundRings(
                cache.NearEFG!.Value.boundRing.Value,
                thisRing.Value.PointDownNode,
                cache.NearEFG.Value.boundRing.Value.PointUpNode);
            listOfHoles.Remove(cache.NearEFG.Value.boundRing);
        }

        if (flagAhgCanConnect && oldPointMax.Equals(thisRing.Value.PointMax))
        {
            thisRing.Value.ConnectBoundRings(
                cache.NearAHG!.Value.boundRing.Value,
                thisRing.Value.PointRightNode,
                cache.NearAHG.Value.boundRing.Value.PointLeftNode);
            listOfHoles.Remove(cache.NearAHG.Value.boundRing);
        }

        return flagAbcCanConnect || flagCdeCanConnect || flagEfgCanConnect || flagAhgCanConnect;
    }
    private bool BruteforceConnectIntersectionBoundRFrames(
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles,
        PartitionBoundRingsCache cache)
    {
        var currentFrameNode = cache.IntersectFrames.First;
        do
        {
            while (currentFrameNode is not null)
            {
                if (BoundRIntersectsChecker.IntersectOrContainFramesCheck(currentFrameNode.Value.Value, thisRing.Value))
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
                var intersectOfFrames =
                    BoundRIntersectsChecker.GetIntersectionBoundRFrames(thisRing.Value, currentFrame.Value);
                do
                {
                    if (!flagFirstCycle)
                    {
                        do
                        {
                            if (BoundRIntersectsChecker.PointInsideFrameCheck(startThisRing.Elem, intersectOfFrames))
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
                            if (BoundRIntersectsChecker.PointInsideFrameCheck(startCurrentFrame.Elem,
                                    intersectOfFrames))
                            {
                                flagSecondCycle = true;
                                break;
                            }

                            startCurrentFrame = startCurrentFrame.Next;
                        } while (!ReferenceEquals(startCurrentFrame, currentFrame.Value.Ring));
                    }

                    if (flagFirstCycle && flagSecondCycle)
                    {
                        if (BoundRIntersectsChecker.IntersectBoundRingNotExtPoints(thisRing, startThisRing.Elem,
                                startCurrentFrame.Elem))
                        {
                            flagFirstCycle = false;
                            startThisRing = startThisRing.Next;
                        }

                        if (BoundRIntersectsChecker.IntersectBoundRingNotExtPoints(currentFrameNode.Value,
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
                                    if (BoundRIntersectsChecker.HasIntersectsBoundRFrame
                                            (frame.Value, startThisRing.Elem, startCurrentFrame.Elem)

                                        || BoundRIntersectsChecker.PointInsideBoundRFrame
                                            (startThisRing.Elem, frame.Value)

                                        || BoundRIntersectsChecker.PointInsideBoundRFrame
                                            (startCurrentFrame.Elem, frame.Value))
                                    {
                                        if (BoundRIntersectsChecker.IntersectBoundRing(frame, startThisRing.Elem,
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
                                if (BoundRIntersectsChecker.IntersectBoundRing(frame, startThisRing.Elem,
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
        PartitionBoundRingsCache cache)
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
        if (cache.NearABCSegmentintersect is not null)
        {
            bool flag = ConnectWithBoundRFrameWhoContainThisRing(cache.NearABCSegmentintersect, thisRing, listOfHoles, PartitioningZones.ABC, cache);
            if (flag)
                return true;
        }
        if (cache.NearCDESegmentintersect is not null)
        {
            bool flag = ConnectWithBoundRFrameWhoContainThisRing(cache.NearCDESegmentintersect, thisRing, listOfHoles, PartitioningZones.CDE, cache);
            if (flag)
                return true;
        }
        if (cache.NearEFGSegmentintersect is not null)
        {
            bool flag = ConnectWithBoundRFrameWhoContainThisRing(cache.NearEFGSegmentintersect, thisRing, listOfHoles, PartitioningZones.EFG, cache);
            if (flag)
                return true;
        }
        if (cache.NearAHGSegmentintersect is not null)
        {
            bool flag = ConnectWithBoundRFrameWhoContainThisRing(cache.NearAHGSegmentintersect, thisRing, listOfHoles, PartitioningZones.AHG, cache);
            if (flag)
                return true;
        }

        return false;
    }
    
    
    
    //todo добавить проверку на пересечение соединения с прямоугольниками
    //todo рассмотреть ситуацию когда все ближайшие треугольники равны null (_nearAbc и другие подобные)
    private bool ConnectWithBoundRFrameWhoContainThisRing(
        (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? nearSegmentIntersect,
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles, 
        PartitioningZones zones, 
        PartitionBoundRingsCache cache)
    {
        var coord = nearSegmentIntersect!.Value._start;
        coord = RearrangePoints(coord, zones, thisRing);
        //todo изменить nearSegmentIntersect так чтобы он содержал узел в списке _framesContainThis
        var startFrameContainThis = cache.FramesContainThis.First;
        do
        {
            if (ReferenceEquals(startFrameContainThis!.Value.Value, nearSegmentIntersect.Value.boundRing.Value))
            {
                var buff = startFrameContainThis.Value;
                cache.FramesContainThis.Remove(startFrameContainThis);
                cache.FramesContainThis.AddFirst(buff);
                break;
            }

            startFrameContainThis = startFrameContainThis.Next;
        } while (startFrameContainThis is not null);

        LinkedNode<Coordinate> connectCoordThisR;
        if (zones == PartitioningZones.ABC)
            connectCoordThisR = thisRing.Value.PointUpNode;
        else if (zones == PartitioningZones.CDE)
            connectCoordThisR = thisRing.Value.PointLeftNode;
        else if (zones == PartitioningZones.EFG)
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
                        BoundRIntersectsChecker.CheckIntersectRingWithSegmentNotExtPoint(frame, connectCoordThisR.Elem, coord.Elem);
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
        PartitioningZones zones,
        LinkedListNode<BoundingRing> thisRing)
    {
        if (zones == PartitioningZones.ABC)
        {
            if (coord.Elem.Y < thisRing.Value.PointUpNode.Elem.Y)
            {
                coord = coord.Next;
            }
        }
        else if (zones == PartitioningZones.CDE)
        {
            if (coord.Elem.X > thisRing.Value.PointLeftNode.Elem.X)
            {
                coord = coord.Next;
            }
        }
        else if (zones == PartitioningZones.EFG)
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
        PartitionBoundRingsCache cache)
    {
        LinkedListNode<BoundingRing> connectedFrame;
        LinkedNode<Coordinate> connectedPoint;
        var collectionABC = cache.ListA.Concat(cache.ListB).Concat(cache.ListC);
        if (collectionABC.Any())
        {
            connectedFrame = collectionABC.First().boundRing;
            connectedPoint = connectedFrame.Value.PointUpNode;
        }
        else
        {
            var start = cache.FramesContainThis.First!.Value.Value.Ring;

            while(true)
            {
                if (start.Elem.Y > thisRing.Value.PointMax.Y
                    || (Math.Abs(start.Elem.Y - thisRing.Value.PointMax.Y) < 1e-9))
                {
                    connectedPoint = start;
                    connectedFrame = cache.FramesContainThis.First!.Value;
                    break;
                }

                start = start.Next;
            }
        }
        bool flag;
        do
        {
            flag = false;

            bool flagFirstCycle;
            do
            {
                flagFirstCycle = false;
                foreach (var frame in cache.IntersectFrames)
                {
                    if (BoundRIntersectsChecker.HasIntersectsBoundRFrame(
                            frame.Value,
                            thisRing.Value.PointUpNode.Elem,
                            connectedPoint.Elem) || 
                        BoundRIntersectsChecker.PointInsideBoundRFrame(thisRing.Value.PointUpNode.Elem, frame.Value)|| 
                        BoundRIntersectsChecker.PointInsideBoundRFrame(connectedPoint.Elem, frame.Value))
                    {
                        var start = frame.Value.PointUpNode;
                        do
                        {
                            if (BoundRIntersectsChecker.HasIntersectedSegmentsNotExternalPoints(
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
            } while (flagFirstCycle);

            bool flagSecondCycle;
            do
            {
                flagSecondCycle = false;
                foreach (var frame in collectionABC)
                {
                    if (BoundRIntersectsChecker.HasIntersectsBoundRFrame(
                            frame.boundRing.Value,
                            thisRing.Value.PointUpNode.Elem,
                            connectedPoint.Elem))
                    {
                        var start = frame.boundRing.Value.PointUpNode;
                        do
                        {
                            if (BoundRIntersectsChecker.HasIntersectedSegmentsNotExternalPoints(
                                    start.Elem, start.Next.Elem,
                                    thisRing.Value.PointUpNode.Elem, connectedPoint.Elem))
                            {

                                connectedFrame = frame.boundRing;
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
                        } while (!ReferenceEquals(start, frame.boundRing.Value.PointUpNode));
                    }
                }
            } while (flagSecondCycle);

            bool flagThirdCycle;
            do
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
                            if (BoundRIntersectsChecker.HasIntersectedSegmentsNotExternalPoints(
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
            } while (flagThirdCycle);
        } while (flag);
        
        thisRing.Value.ConnectBoundRings(
            connectedFrame.Value,
            thisRing.Value.PointUpNode,
            connectedPoint);
        listOfHoles.Remove(connectedFrame);
    }
    
}

            
        