using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using GeoSlicer.Utils.BoundHoleDelDependency;
using NetTopologySuite.Algorithm;

namespace GeoSlicer.HoleDeleters;

public class BoundingHoleDeleter
{
    private LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> _listA;
    private LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> _listB;
    private LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> _listC;
    private LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> _listD;
    private LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> _listE;
    private LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> _listF;
    private LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> _listG;
    private LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> _listH;
    private LinkedList<LinkedListNode<BoundingRing>> _intersectFrames;
    private LinkedList<LinkedListNode<BoundingRing>> _framesContainThis;
    private (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? _nearABC;
    private (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? _nearCDE;
    private (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? _nearEFG;
    private (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? _nearAHG;


    private (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? _nearABCintersect;
    private (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? _nearCDEintersect;
    private (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? _nearEFGintersect;
    private (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? _nearAHGintersect;
    private (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? _nearABCSegmentintersect;
    private (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? _nearCDESegmentintersect;
    private (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? _nearEFGSegmentintersect;
    private (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? _nearAHGSegmentintersect;


    private BoundingHoleDeleter()
    {
        _listA = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        _listB = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        _listC = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        _listD = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        _listE = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        _listF = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        _listG = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        _listH = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        _intersectFrames = new LinkedList<LinkedListNode<BoundingRing>>();
        _framesContainThis = new LinkedList<LinkedListNode<BoundingRing>>();

    }

    private void Clear()
    {
        _listA.Clear();
        _listB.Clear();
        _listC.Clear();
        _listD.Clear();
        _listE.Clear();
        _listF.Clear();
        _listG.Clear();
        _listH.Clear();
        _intersectFrames.Clear();
        _framesContainThis.Clear();
        _nearABC = null;
        _nearCDE = null;
        _nearEFG = null;
        _nearAHG = null;
        _nearABCintersect = null;
        _nearCDEintersect = null;
        _nearEFGintersect = null;
        _nearAHGintersect = null;
        _nearABCSegmentintersect = null;
        _nearCDESegmentintersect = null;
        _nearEFGSegmentintersect = null;
        _nearAHGSegmentintersect = null;
    }

    public static /*LinearRing*/ Polygon DeleteHoles(Polygon polygon)
    {
        LinkedList<BoundingRing> list = BoundRingService.PolygonToBoundRings(polygon);
        new BoundingHoleDeleter().DeleteHoles(list);
        //return BoundRingService.BoundRingsToPolygon(list).Shell;
        return BoundRingService.BoundRingsToPolygon(list);
    }
    private int j = 0;
    private void DeleteHoles(LinkedList<BoundingRing> listOfHoles)
    {
        var thisRing = listOfHoles.First;
        var pointMinShell = thisRing!.Value.PointMin;
        var pointMaxShell = thisRing.Value.PointMax;

        int i = 0;

        int count = listOfHoles.Count;
        while (listOfHoles.First!.Next is not null)
        {
            //GeoJsonFileService.GeoJsonFileService.WriteGeometryToFile(BoundRingService.BoundRingsToPolygon(listOfHoles), "C:\\Users\\Данил\\Downloads\\Telegram Desktop\\newBaikal" + j + ".geojson");
            j++;
            i++;
            //Console.WriteLine(j);
            if (count != listOfHoles.Count)
            {
                count = listOfHoles.Count;
                i = 0;
            }
            else if (i == count) return;
            if (_framesContainThis.Any())
            {
                LinkedList<BoundingRing> list = new LinkedList<BoundingRing>();
                foreach (var ring in _framesContainThis)
                {
                    list.AddLast(ring.Value);
                }
                
            }
            
            if (thisRing.Next is null)
                thisRing = listOfHoles.First.Next;
            else thisRing = thisRing.Next;
            Clear();
            bool hasIntersectFrames = FillListsRelativeRing(thisRing, listOfHoles);
            if (!hasIntersectFrames)
            {
                bool frameOfThisChanged = false;
                if (_intersectFrames.Any())
                {
                    //frameOfThisChanged = ConnectContainsRingsInThis(thisRing);
                }

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
                    
                    Coordinate oldPointMin = thisRing.Value.PointMin;
                    Coordinate oldPointMax = thisRing.Value.PointMax;
                    if (!ConnectNoIntersectRectan(thisRing, listOfHoles))
                    {
                        BruteforceConnect(thisRing, listOfHoles);
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
            else
            {
                BruteforceConnectIntersectionFrames(thisRing, listOfHoles);
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
    private int m = 0;
    private void BruteforceConnectIntersectionFrames(LinkedListNode<BoundingRing> thisRing, LinkedList<BoundingRing> listOfHoles)
    {
        var currentFrameNode = _intersectFrames.First;
        do
        {
            while (currentFrameNode is not null)
            {
                if (IntersectOrContainFramesCheck(currentFrameNode.Value.Value, thisRing.Value))
                {
                    break;
                }

                currentFrameNode = currentFrameNode.Next;
            }

            if (currentFrameNode is null)
                return;
            var currentFrame = currentFrameNode.Value;
            var startThisRing = thisRing.Value.Ring;
            var startCurrentFrame = currentFrame!.Value.Ring;
            bool flagFirstCycle = false;
            bool flagSecondCycle = false;
            var intersectOfFrames = getIntersectionFrames(thisRing.Value, currentFrame.Value);
            do
            {
                if (!flagFirstCycle)
                {
                    do
                    {
                        if (PointInsideFrameCheck(startThisRing.Elem, intersectOfFrames))
                        {
                            flagFirstCycle = true;
                            break;
                        }

                        startThisRing = startThisRing.Next;
                    } while (!ReferenceEquals(startThisRing, thisRing.Value.Ring));
                }

                if (!flagSecondCycle)
                {
                    int m = 0;
                    do
                    {
                        if (PointInsideFrameCheck(startCurrentFrame.Elem, intersectOfFrames))
                        {
                            flagSecondCycle = true;
                            break;
                        }
                        startCurrentFrame = startCurrentFrame.Next;
                    } while (!ReferenceEquals(startCurrentFrame, currentFrame.Value.Ring));
                }
                if (flagFirstCycle && flagSecondCycle)
                {
                    if (IntersectLBoundRingNotExtPoints(thisRing, startThisRing.Elem, startCurrentFrame.Elem))
                    {
                        flagFirstCycle = false;
                        startThisRing = startThisRing.Next;
                    }

                    if (IntersectLBoundRingNotExtPoints(currentFrameNode.Value, startThisRing.Elem,
                            startCurrentFrame.Elem))
                    {
                        flagSecondCycle = false;
                        startCurrentFrame = startCurrentFrame.Next;
                    }

                    if (flagFirstCycle && flagSecondCycle)
                    {
                        foreach (var frame in _intersectFrames)
                        {
                            if (!ReferenceEquals(currentFrameNode.Value, frame))
                            {
                                if (hasIntersectsFrame(frame.Value, startThisRing.Elem, startCurrentFrame.Elem)
                                    || PointInsideFrameCheck(startThisRing.Elem, frame.Value)
                                    || PointInsideFrameCheck(startCurrentFrame.Elem, frame.Value))
                                {
                                    if (IntersectLBoundRing(frame, startThisRing.Elem, startCurrentFrame.Elem))
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
                        foreach (var frame in _framesContainThis)
                        {
                            if (IntersectLBoundRing(frame, startThisRing.Elem, startCurrentFrame.Elem))
                            {
                                flagFirstCycle = false;
                                flagSecondCycle = false;
                                startThisRing = startThisRing.Next;
                                startCurrentFrame = startCurrentFrame.Next;
                            }
                        }

                        if (flagFirstCycle && flagSecondCycle)
                        {
                            BoundRingService.ConnectBoundRings(
                                thisRing.Value,
                                currentFrame.Value,
                                startThisRing,
                                startCurrentFrame);

                            listOfHoles.Remove(currentFrame);
                            return;
                        }
                    }
                }
            } while (!ReferenceEquals(startThisRing, thisRing.Value.Ring) &&
                     !ReferenceEquals(startCurrentFrame, currentFrame.Value.Ring));
            currentFrameNode = currentFrameNode.Next;
        } while (true);
    }
    //todo нахождение прямоугольника, соединение с которым не пересекает другие 
    private void BruteforceConnect(LinkedListNode<BoundingRing> thisRing, LinkedList<BoundingRing> listOfHoles)
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
        if (_nearABCSegmentintersect is not null)
        {
            bool flag = ConnectWithFrameWhoContainThisRing(_nearABCSegmentintersect, thisRing, listOfHoles, PartitioningZones.ABC);
            if (flag)
                return;
        }
        if (_nearCDESegmentintersect is not null)
        {
            bool flag = ConnectWithFrameWhoContainThisRing(_nearCDESegmentintersect, thisRing, listOfHoles, PartitioningZones.CDE);
            if (flag)
                return;
        }
        if (_nearEFGSegmentintersect is not null)
        {
            bool flag = ConnectWithFrameWhoContainThisRing(_nearEFGSegmentintersect, thisRing, listOfHoles, PartitioningZones.EFG);
            if (flag)
                return;
        }
        if (_nearAHGSegmentintersect is not null)
        {
            bool flag = ConnectWithFrameWhoContainThisRing(_nearAHGSegmentintersect, thisRing, listOfHoles, PartitioningZones.AHG);
            if (flag)
                return;
        }
    }
    //todo добавить проверку на пересечение соединения с прямоугольниками
    //todo рассмотреть ситуацию когда все ближайшие треугольники равны null (_nearAbc и другие подобные)
    private bool ConnectWithFrameWhoContainThisRing(
        (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? nearSegmentIntersect,
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles, PartitioningZones zones)
    {
        var coord = nearSegmentIntersect!.Value._start;
        coord = RearrangePoints(coord, zones, thisRing);
        //todo изменить nearSegmentIntersect так чтобы он содержал узел в списке _framesContainThis
        var startFrameContainThis = _framesContainThis.First;
        do
        {
            if (ReferenceEquals(startFrameContainThis!.Value.Value, nearSegmentIntersect.Value.boundRing.Value))
            {
                var buff = startFrameContainThis.Value;
                _framesContainThis.Remove(startFrameContainThis);
                _framesContainThis.AddFirst(buff);
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
        var correctNode = _framesContainThis.First!.Value;
        do
        {
            flag = false;
            foreach (var frame in _framesContainThis)
            {
                (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? intersectSegment;
                do
                {
                    intersectSegment =
                        CheckIntersectRingWithSegmentNotExtPoint(frame, connectCoordThisR.Elem, coord.Elem);
                    if (intersectSegment is not null)
                    {
                        flag = true;
                        coord = intersectSegment.Value._start;
                        coord = RearrangePoints(coord, zones, thisRing);

                        correctNode = frame;
                    }
                    
                    //todo найти причину почему зацикливается на kazanFail
                } while (intersectSegment is not null);
                
            }
            
        } while (flag);

        BoundRingService.ConnectBoundRings(thisRing.Value, correctNode.Value,
            connectCoordThisR, coord);
        listOfHoles.Remove(correctNode);
        return true;
    }

    private LinkedNode<Coordinate> RearrangePoints(LinkedNode<Coordinate> coord, PartitioningZones zones,  LinkedListNode<BoundingRing> thisRing)
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
    private (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? CheckIntersectRingWithSegmentNotExtPoint(
        LinkedListNode<BoundingRing> ring, Coordinate a, Coordinate b)
    {
        var start = ring.Value.Ring;
        do
        {
            if (HasIntersectedSegments(start.Elem, start.Next.Elem, a, b)
                && !ReferenceEquals(a, start.Elem)
                && !ReferenceEquals(b, start.Elem)
                && !ReferenceEquals(a, start.Next.Elem)
                && !ReferenceEquals(b, start.Next.Elem))
                return (ring, start);
            start = start.Next;
        } while (!ReferenceEquals(start, ring.Value.Ring));

        return null;
    }

    private LinkedListNode<BoundingRing>? CheckIntersectFramesWithoutThisFrame
    (LinkedList<(LinkedListNode<BoundingRing> boundRing, 
            List<PartitioningZones> zones)> frames, Coordinate a,
        Coordinate b, LinkedListNode<BoundingRing> thisFrame)
    {
        foreach (var frame in frames)
        {
            if (hasIntersectsFrame(frame.boundRing.Value, a, b) && !ReferenceEquals(frame.boundRing, thisFrame))
                return frame.boundRing;
        }

        return null;
    }
    private bool ConnectNoIntersectRectan(LinkedListNode<BoundingRing> thisRing, LinkedList<BoundingRing> listOfHoles)
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

        if (_nearABC is null)
            flagAbcCanConnect = false;
        else
        {
            bool flagA = false;
            bool flagB = false;
            bool flagC = false;
            firstCoordLineConnectNearAbc = thisRing.Value.PointUpNode.Elem;
            secondCoordLineConnectNearAbc = _nearABC!.Value.boundRing.Value.PointDownNode.Elem;
            foreach (var zone in _nearABC!.Value.zones)
            {
                if (!flagA && zone == PartitioningZones.A)
                    flagA = true;
                else if (!flagB && zone == PartitioningZones.B)
                    flagB = true;
                else flagC = true;
            }

            if (flagC)
            {
                foreach (var frame in _listC)
                {
                    if (!frame.zones.Contains(PartitioningZones.B)
                        && frame.boundRing.Value.PointMin.Y < thisRing.Value.PointMax.Y
                        && hasIntersectsFrame(frame.boundRing.Value, firstCoordLineConnectNearAbc,
                            secondCoordLineConnectNearAbc) &&
                        !ReferenceEquals(frame.boundRing.Value, _nearABC.Value.boundRing.Value))
                    {
                        _nearABCintersect = frame;
                        flagAbcCanConnect = false;
                    }
                }
            }

            if (flagA && flagAbcCanConnect)
            {
                foreach (var frame in _listA)
                {
                    if (!frame.zones.Contains(PartitioningZones.B)
                        && frame.boundRing.Value.PointMin.Y < thisRing.Value.PointMax.Y
                        && hasIntersectsFrame(frame.boundRing.Value, firstCoordLineConnectNearAbc,
                            secondCoordLineConnectNearAbc) &&
                        !ReferenceEquals(frame.boundRing.Value, _nearABC.Value.boundRing.Value))
                    {
                        _nearABCintersect = frame;
                        flagAbcCanConnect = false;
                    }
                }

            }
        }


        if (_nearCDE is null)
            flagCdeCanConnect = false;
        else
        {
            bool flagC = false;
            bool flagD = false;
            bool flagE = false;
            firstCoordLineConnectNearCde = thisRing.Value.PointLeftNode.Elem;
            secondCoordLineConnectNearCde = _nearCDE!.Value.boundRing.Value.PointRightNode.Elem;

            foreach (var zone in _nearCDE!.Value.zones)
            {
                if (!flagC && zone == PartitioningZones.C)
                    flagC = true;
                else if (!flagD && zone == PartitioningZones.D)
                    flagD = true;
                else flagE = true;
            }

            if (flagC)
            {
                foreach (var frame in _listC)
                {
                    if (!frame.zones.Contains(PartitioningZones.D)
                        && frame.boundRing.Value.PointMax.X > thisRing.Value.PointMin.X
                        && hasIntersectsFrame(frame.boundRing.Value, firstCoordLineConnectNearCde,
                            secondCoordLineConnectNearCde) &&
                        !ReferenceEquals(frame.boundRing.Value, _nearCDE.Value.boundRing.Value))
                    {
                        _nearCDEintersect = frame;
                        flagCdeCanConnect = false;
                    }
                }

            }

            if (flagE && flagCdeCanConnect)
            {
                foreach (var frame in _listE)
                {
                    if (!frame.zones.Contains(PartitioningZones.D)
                        && frame.boundRing.Value.PointMax.X > thisRing.Value.PointMin.X
                        && hasIntersectsFrame(frame.boundRing.Value, firstCoordLineConnectNearCde,
                            secondCoordLineConnectNearCde) &&
                        !ReferenceEquals(frame.boundRing.Value, _nearCDE.Value.boundRing.Value))
                    {
                        _nearCDEintersect = frame;
                        flagCdeCanConnect = false;
                    }
                }

            }
        }



        if (_nearEFG is null)
            flagEfgCanConnect = false;
        else
        {
            bool flagE = false;
            bool flagF = false;
            bool flagG = false;
            firstCoordLineConnectNearEfg = thisRing.Value.PointDownNode.Elem;
            secondCoordLineConnectNearEfg = _nearEFG!.Value.boundRing.Value.PointUpNode.Elem;
            foreach (var zone in _nearEFG!.Value.zones)
            {
                if (!flagE && zone == PartitioningZones.E)
                    flagE = true;
                else if (!flagF && zone == PartitioningZones.F)
                    flagF = true;
                else flagG = true;
            }

            if (flagE)
            {
                foreach (var frame in _listE)
                {
                    if (!frame.zones.Contains(PartitioningZones.F)
                        && frame.boundRing.Value.PointMax.Y > thisRing.Value.PointMin.Y
                        && hasIntersectsFrame(frame.boundRing.Value, firstCoordLineConnectNearEfg,
                            secondCoordLineConnectNearEfg) &&
                        !ReferenceEquals(frame.boundRing.Value, _nearEFG.Value.boundRing.Value))
                    {
                        _nearEFGintersect = frame;
                        flagEfgCanConnect = false;
                    }
                }
            }

            if (flagG && flagEfgCanConnect)
            {
                foreach (var frame in _listG)
                {
                    if (!frame.zones.Contains(PartitioningZones.F)
                        && frame.boundRing.Value.PointMax.Y > thisRing.Value.PointMin.Y
                        && hasIntersectsFrame(frame.boundRing.Value, firstCoordLineConnectNearEfg,
                            secondCoordLineConnectNearEfg) &&
                        !ReferenceEquals(frame.boundRing.Value, _nearEFG.Value.boundRing.Value))
                    {
                        _nearEFGintersect = frame;
                        flagEfgCanConnect = false;
                    }
                }
            }
        }



        if (_nearAHG is null)
            flagAhgCanConnect = false;
        else
        {
            bool flagA = false;
            bool flagH = false;
            bool flagG = false;
            firstCoordLineConnectNearAhg = thisRing.Value.PointRightNode.Elem;
            secondCoordLineConnectNearAhg = _nearAHG!.Value.boundRing.Value.PointLeftNode.Elem;
            ;
            foreach (var zone in _nearAHG!.Value.zones)
            {
                if (!flagA && zone == PartitioningZones.A)
                    flagA = true;
                else if (!flagH && zone == PartitioningZones.H)
                    flagH = true;
                else flagG = true;
            }

            if (flagA)
            {
                foreach (var frame in _listA)
                {
                    if (!frame.zones.Contains(PartitioningZones.H)
                        && frame.boundRing.Value.PointMin.X < thisRing.Value.PointMax.X
                        && hasIntersectsFrame(frame.boundRing.Value, firstCoordLineConnectNearAhg,
                            secondCoordLineConnectNearAhg) &&
                        !ReferenceEquals(frame.boundRing.Value, _nearAHG.Value.boundRing.Value))
                    {
                        _nearAHGintersect = frame;
                        flagAhgCanConnect = false;
                    }
                }
            }

            if (flagG && flagAhgCanConnect)
            {
                foreach (var frame in _listG)
                {
                    if (!frame.zones.Contains(PartitioningZones.H)
                        && frame.boundRing.Value.PointMin.X < thisRing.Value.PointMax.X
                        && hasIntersectsFrame(frame.boundRing.Value, firstCoordLineConnectNearAhg,
                            secondCoordLineConnectNearAhg) &&
                        !ReferenceEquals(frame.boundRing.Value, _nearAHG.Value.boundRing.Value))
                    {
                        _nearAHGintersect = frame;
                        flagAhgCanConnect = false;
                    }
                }
            }
        }
        
        
        var shell = _framesContainThis.First!.Value;
        _framesContainThis.Remove(_framesContainThis.First);
        _framesContainThis.AddLast(shell);
        foreach (var frameWhoContainThis in _framesContainThis)
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
                    if (HasIntersectedSegments(firstCoordLineConnectNearAbc!, secondCoordLineConnectNearAbc!, buffer.Elem,
                            buffer.Next.Elem))
                    {
                        _nearABCSegmentintersect = (frameWhoContainThis, buffer);
                        flagAbcCanConnect = false;

                    }
                }

                if (flagCdeCanConnect &&
                    (buffer.Elem.X < thisRing.Value.PointMin.X
                     || Math.Abs(buffer.Elem.X - thisRing.Value.PointMin.X) < 1e-9
                     || buffer.Next.Elem.X < thisRing.Value.PointMin.X
                     || Math.Abs(buffer.Next.Elem.X - thisRing.Value.PointMin.X) < 1e-9))
                {
                    if (HasIntersectedSegments(firstCoordLineConnectNearCde!, secondCoordLineConnectNearCde!, buffer.Elem,
                            buffer.Next.Elem))
                    {
                        _nearCDESegmentintersect = (frameWhoContainThis, buffer);
                        flagCdeCanConnect = false;
                    }
                }

                if (flagEfgCanConnect &&
                    (buffer.Elem.Y < thisRing.Value.PointMin.Y
                     || Math.Abs(buffer.Elem.Y - thisRing.Value.PointMin.Y) < 1e-9
                     || buffer.Next.Elem.Y < thisRing.Value.PointMin.Y
                     || Math.Abs(buffer.Next.Elem.Y - thisRing.Value.PointMin.Y) < 1e-9))
                {
                    if (HasIntersectedSegments(firstCoordLineConnectNearEfg!, secondCoordLineConnectNearEfg!, buffer.Elem,
                            buffer.Next.Elem))
                    {
                        _nearEFGSegmentintersect = (frameWhoContainThis, buffer);
                        flagEfgCanConnect = false;
                    }
                }

                if (flagAhgCanConnect &&
                    (buffer.Elem.X > thisRing.Value.PointMax.X
                     || Math.Abs(buffer.Elem.X - thisRing.Value.PointMax.X) < 1e-9
                     || buffer.Next.Elem.X > thisRing.Value.PointMax.X
                     || Math.Abs(buffer.Next.Elem.X - thisRing.Value.PointMax.X) < 1e-9))
                {
                    if (HasIntersectedSegments(firstCoordLineConnectNearAhg!, secondCoordLineConnectNearAhg!, buffer.Elem,
                            buffer.Next.Elem))
                    {
                        _nearAHGSegmentintersect = (frameWhoContainThis, buffer);
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
            thisRing.Value = BoundRingService.ConnectBoundRings(thisRing.Value,
                _nearABC!.Value.boundRing.Value,
                thisRing.Value.PointUpNode,
                _nearABC.Value.boundRing.Value.PointDownNode);
            
            listOfHoles.Remove(_nearABC.Value.boundRing);
        }

        if (flagCdeCanConnect && oldPointMin.Equals(thisRing.Value.PointMin))
        {
            thisRing.Value = BoundRingService.ConnectBoundRings(thisRing.Value,
                _nearCDE!.Value.boundRing.Value,
                thisRing.Value.PointLeftNode,
                _nearCDE.Value.boundRing.Value.PointRightNode);

            listOfHoles.Remove(_nearCDE.Value.boundRing);
        }

        if (flagEfgCanConnect && oldPointMin.Equals(thisRing.Value.PointMin))
        {
            thisRing.Value = BoundRingService.ConnectBoundRings(thisRing.Value,
                _nearEFG!.Value.boundRing.Value,
                thisRing.Value.PointDownNode,
                _nearEFG.Value.boundRing.Value.PointUpNode);
            listOfHoles.Remove(_nearEFG.Value.boundRing);
        }

        if (flagAhgCanConnect && oldPointMax.Equals(thisRing.Value.PointMax))
        {
            thisRing.Value = BoundRingService.ConnectBoundRings(thisRing.Value,
                _nearAHG!.Value.boundRing.Value,
                thisRing.Value.PointRightNode,
                _nearAHG.Value.boundRing.Value.PointLeftNode);
            listOfHoles.Remove(_nearAHG.Value.boundRing);
        }

        return flagAbcCanConnect || flagCdeCanConnect || flagEfgCanConnect || flagAhgCanConnect;
    }

    private bool IntersectLBoundRingNotExtPoints(
        LinkedListNode<BoundingRing> boundRing, 
        Coordinate a,
        Coordinate b)
    {
        LinkedNode<Coordinate> start = boundRing.Value.Ring;
        do
        {
            if (HasIntersectedSegments(start.Elem, start.Next.Elem, a, b)
                && !ReferenceEquals(a, start.Elem)
                && !ReferenceEquals(b, start.Elem)
                && !ReferenceEquals(a, start.Next.Elem)
                && !ReferenceEquals(b, start.Next.Elem))
                return true;
            start = start.Next;
        } while (!ReferenceEquals(start, boundRing.Value.Ring));

        return false;
    }
    private bool IntersectLBoundRing(
        LinkedListNode<BoundingRing> boundRing, 
        Coordinate a,
        Coordinate b)
    {
        LinkedNode<Coordinate> start = boundRing.Value.Ring;
        do
        {
            if (HasIntersectedSegments(start.Elem, start.Next.Elem, a, b))
                return true;
            start = start.Next;
        } while (!ReferenceEquals(start, boundRing.Value.Ring));

        return false;
    }

    //todo возможно улучшение
    private bool hasIntersectsFrame(BoundingRing ring, Coordinate a, Coordinate b)
    {
        LineSegment AB = new LineSegment(a, b);
        LineSegment[] sides = new LineSegment [4];
        sides[0] = new LineSegment(ring.PointMin, new Coordinate(ring.PointMin.X, ring.PointMax.Y));
        sides[1] = new LineSegment(new Coordinate(ring.PointMin.X, ring.PointMax.Y), ring.PointMax);
        sides[2] = new LineSegment(ring.PointMax, new Coordinate(ring.PointMax.X, ring.PointMin.Y));
        sides[3] = new LineSegment(new Coordinate(ring.PointMax.X, ring.PointMin.Y), ring.PointMin);
        foreach (var side in sides)
        {
            if (side.Intersection(AB) is not null)
                return true;
        }

        return false;
    }

    //todo нужно улучшить
    private RobustLineIntersector li = new RobustLineIntersector();
    private bool HasIntersectedSegments(Coordinate a1, Coordinate b1, Coordinate a2, Coordinate b2)
    {
        //return new LineSegment(a1, b1).Intersection(new LineSegment(a2, b2)) is not null;
        li.ComputeIntersection(a1, b1, a2, b2);
        return li.HasIntersection;
        //return li.GetIntersection(0);
    }
    private bool HasIntersectedSegmentsNotInExternalPoints(Coordinate a1, Coordinate b1, Coordinate a2, Coordinate b2)
    {
        //return new LineSegment(a1, b1).Intersection(new LineSegment(a2, b2)) is not null;
        li.ComputeIntersection(a1, b1, a2, b2);
        return (li.IsProper && (li.IntersectionNum == LineIntersector.PointIntersection));
        //return li.GetIntersection(0);
    }
    
    private bool ConnectContainsRingsInThis(LinkedListNode<BoundingRing> thisRing)
    {
        throw new AggregateException();
    }


    private IList<PartitioningZones> detectZonesTwoCoordinates(Coordinate a, Coordinate b,
        BoundingRing thisRing)
    {
        ISet<PartitioningZones> set = new HashSet<PartitioningZones>();
        if (a.Y > thisRing.PointMax.Y || Math.Abs(a.Y - thisRing.PointMax.Y) < 1e-9)
        {
            if (a.X < thisRing.PointMin.X) set.Add(PartitioningZones.C);
            else if (a.X > thisRing.PointMax.X) set.Add(PartitioningZones.A);
            else set.Add(PartitioningZones.B);
        }
        else if (a.X > thisRing.PointMin.X || Math.Abs(a.X - thisRing.PointMin.X) < 1e-9)
        {
            if (a.Y > thisRing.PointMax.Y) set.Add(PartitioningZones.C);
            else if (a.Y < thisRing.PointMin.Y) set.Add(PartitioningZones.E);
            else set.Add(PartitioningZones.D);
        }
        else if (a.Y < thisRing.PointMin.Y || Math.Abs(a.Y - thisRing.PointMin.Y) < 1e-9)
        {
            if (a.X < thisRing.PointMin.X) set.Add(PartitioningZones.E);
            else if (a.X > thisRing.PointMax.X) set.Add(PartitioningZones.G);
            else set.Add(PartitioningZones.F);
        }
        else
        {
            if (a.Y > thisRing.PointMax.Y) set.Add(PartitioningZones.A);
            else if (a.Y < thisRing.PointMin.Y) set.Add(PartitioningZones.G);
            else set.Add(PartitioningZones.H);
        }

        if (b.Y > thisRing.PointMax.Y || Math.Abs(b.Y - thisRing.PointMax.Y) < 1e-9)
        {
            if (b.X < thisRing.PointMin.X) set.Add(PartitioningZones.C);
            else if (b.X > thisRing.PointMax.X) set.Add(PartitioningZones.A);
            else set.Add(PartitioningZones.B);
        }
        else if (b.X > thisRing.PointMin.X || Math.Abs(b.X - thisRing.PointMin.X) < 1e-9)
        {
            if (b.Y > thisRing.PointMax.Y) set.Add(PartitioningZones.C);
            else if (b.Y < thisRing.PointMin.Y) set.Add(PartitioningZones.E);
            else set.Add(PartitioningZones.D);
        }
        else if (b.Y < thisRing.PointMin.Y || Math.Abs(b.Y - thisRing.PointMin.Y) < 1e-9)
        {
            if (b.X < thisRing.PointMin.X) set.Add(PartitioningZones.E);
            else if (b.X > thisRing.PointMax.X) set.Add(PartitioningZones.G);
            else set.Add(PartitioningZones.F);
        }
        else
        {
            if (b.Y > thisRing.PointMax.Y) set.Add(PartitioningZones.A);
            else if (b.Y < thisRing.PointMin.Y) set.Add(PartitioningZones.G);
            else set.Add(PartitioningZones.H);
        }

        return set.ToList();
    }

    private bool FillListsRelativeRing(LinkedListNode<BoundingRing> boundRing,
        LinkedList<BoundingRing> boundRings)
    {
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
                            _intersectFrames.AddFirst(thisRing);
                    }
                }
                else if (NotIntersectCheck(boundRing.Value, thisRing.Value))
                {
                    IntersectOrContainFrames(boundRing, thisRing);
                }
                else
                {
                    _intersectFrames.AddFirst(thisRing);
                }
            }

            thisRing = thisRing.Next;
        }
        return hasIntersectFrames;
    }

    //возращает false если одна рамка содержится в другой
    //true в противном случае(могут пересекаться и не пересекаться)
    private bool IntersectOrContainFrames(
        LinkedListNode<BoundingRing> relativeBoundRing,
        LinkedListNode<BoundingRing> thisBoundRing)
    {
        Coordinate pointMin = new Coordinate(
            Math.Min(relativeBoundRing.Value.PointMin.X, thisBoundRing.Value.PointMin.X),
            Math.Min(relativeBoundRing.Value.PointMin.Y, thisBoundRing.Value.PointMin.Y));
        Coordinate pointMax = new Coordinate(
            Math.Max(relativeBoundRing.Value.PointMax.X, thisBoundRing.Value.PointMax.X),
            Math.Max(relativeBoundRing.Value.PointMax.Y, thisBoundRing.Value.PointMax.Y));
        if (pointMin.Equals(relativeBoundRing.Value.PointMin) &&
            pointMax.Equals(relativeBoundRing.Value.PointMax))
        {
            _intersectFrames.AddFirst(thisBoundRing);
            return false;
        }

        if (pointMin.Equals(thisBoundRing.Value.PointMin) && pointMax.Equals(thisBoundRing.Value.PointMax))
        {
            _framesContainThis.AddFirst(thisBoundRing);
            return false;
        }

        //_intersectFrames.AddFirst(thisBoundRing);
        return true;
    }
    //возращает false если одна рамка содержится в другой
    //true в противном случае(могут пересекаться и не пересекаться)
    private bool IntersectOrContainFramesCheck(
        BoundingRing ring1,
        BoundingRing ring2)
    {
        Coordinate pointMin = new Coordinate(
            Math.Min(ring1.PointMin.X, ring2.PointMin.X),
            Math.Min(ring1.PointMin.Y, ring2.PointMin.Y));
        Coordinate pointMax = new Coordinate(
            Math.Max(ring1.PointMax.X, ring2.PointMax.X),
            Math.Max(ring1.PointMax.Y, ring2.PointMax.Y));
        if ((pointMin.Equals(ring1.PointMin) && pointMax.Equals(ring1.PointMax)) || 
            (pointMin.Equals(ring2.PointMin) && pointMax.Equals(ring2.PointMax)))
        {
            return false;
        }
        return true;
    }

    private (Coordinate pointMin, Coordinate pointMax) getIntersectionFrames(BoundingRing ring1, BoundingRing ring2)
    {
        Coordinate pointMin = new Coordinate(
            Math.Max(ring1.PointMin.X, ring2.PointMin.X),
            Math.Max(ring1.PointMin.Y, ring2.PointMin.Y));
        Coordinate pointMax = new Coordinate(
            Math.Min(ring1.PointMax.X, ring2.PointMax.X),
            Math.Min(ring1.PointMax.Y, ring2.PointMax.Y));
        return (pointMin, pointMax);
    }
    private (Coordinate pointMin, Coordinate pointMax) getIntersectionFrames
        (BoundingRing ring1, (Coordinate pointMin, Coordinate pointMax) frame)
    {
        Coordinate pointMin = new Coordinate(
            Math.Max(ring1.PointMin.X, frame.pointMin.X),
            Math.Max(ring1.PointMin.Y, frame.pointMin.Y));
        Coordinate pointMax = new Coordinate(
            Math.Min(ring1.PointMax.X, frame.pointMax.X),
            Math.Min(ring1.PointMax.Y, frame.pointMax.Y));
        return (pointMin, pointMax);
    }
    //возращает false если рамки пересекаются(не важно как)
    private bool DetectPartitingZone(LinkedListNode<BoundingRing> relativeBoundRing,
        LinkedListNode<BoundingRing> boundingRing)
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
        if (boundingRing.Value.PointMin.Y > relativeBoundRing.Value.PointMax.Y
            || Math.Abs(boundingRing.Value.PointMin.Y - relativeBoundRing.Value.PointMax.Y) < 1e-9)
        {
            if (boundingRing.Value.PointMin.X < relativeBoundRing.Value.PointMin.X)
            {
                list.Add(PartitioningZones.C);
                _listC.AddFirst((boundingRing, list));
                flagC = true;
            }

            if (boundingRing.Value.PointMax.X > relativeBoundRing.Value.PointMax.X)
            {
                list.Add(PartitioningZones.A);
                _listA.AddFirst((boundingRing, list));
                flagA = true;
            }

            if (flagA == flagC
                || SegmentContainAtLeastOneNumber
                (relativeBoundRing.Value.PointMin.X,
                    relativeBoundRing.Value.PointMax.X,
                    new[] { boundingRing.Value.PointMin.X, boundingRing.Value.PointMax.X }))
            {
                list.Add(PartitioningZones.B);
                _listB.AddFirst((boundingRing, list));
            }

            flagABC = true;

        }

        else if (boundingRing.Value.PointMax.X < relativeBoundRing.Value.PointMin.X
            || Math.Abs(boundingRing.Value.PointMax.X - relativeBoundRing.Value.PointMin.X) < 1e-9)
        {
            if (boundingRing.Value.PointMax.Y > relativeBoundRing.Value.PointMax.Y)
            {
                list.Add(PartitioningZones.C);
                _listC.AddFirst((boundingRing, list));
                flagC = true;
            }

            if (boundingRing.Value.PointMin.Y < relativeBoundRing.Value.PointMin.Y)
            {
                list.Add(PartitioningZones.E);
                _listE.AddFirst((boundingRing, list));
                flagE = true;
            }

            if (flagE == flagC
                || SegmentContainAtLeastOneNumber
                (relativeBoundRing.Value.PointMin.Y,
                    relativeBoundRing.Value.PointMax.Y,
                    new[] { boundingRing.Value.PointMin.Y, boundingRing.Value.PointMax.Y }))
            {
                list.Add(PartitioningZones.D);
                _listD.AddFirst((boundingRing, list));
            }

            flagCDE = true;

        }

        else if (boundingRing.Value.PointMax.Y < relativeBoundRing.Value.PointMin.Y
            || Math.Abs(boundingRing.Value.PointMax.Y - relativeBoundRing.Value.PointMin.Y) < 1e-9)
        {
            if (boundingRing.Value.PointMax.X > relativeBoundRing.Value.PointMax.X)
            {
                list.Add(PartitioningZones.G);
                _listG.AddFirst((boundingRing, list));
                flagG = true;
            }

            if (boundingRing.Value.PointMin.X < relativeBoundRing.Value.PointMin.X)
            {
                list.Add(PartitioningZones.E);
                _listE.AddFirst((boundingRing, list));
                flagE = true;
            }

            if (flagE == flagG
                || SegmentContainAtLeastOneNumber
                (relativeBoundRing.Value.PointMin.X,
                    relativeBoundRing.Value.PointMax.X,
                    new[] { boundingRing.Value.PointMin.X, boundingRing.Value.PointMax.X }))
            {
                list.Add(PartitioningZones.F);
                _listF.AddFirst((boundingRing, list));
            }

            flagEFG = true;
        }

        else if (boundingRing.Value.PointMin.X > relativeBoundRing.Value.PointMax.X
            || Math.Abs(boundingRing.Value.PointMin.X - relativeBoundRing.Value.PointMax.X) < 1e-9)
        {
            if (boundingRing.Value.PointMin.Y < relativeBoundRing.Value.PointMin.Y)
            {
                list.Add(PartitioningZones.G);
                _listG.AddFirst((boundingRing, list));
                flagG = true;
            }

            if (boundingRing.Value.PointMax.Y > relativeBoundRing.Value.PointMax.Y)
            {
                list.Add(PartitioningZones.A);
                _listA.AddFirst((boundingRing, list));
                flagA = true;
            }

            if (flagA == flagG
                || SegmentContainAtLeastOneNumber
                (relativeBoundRing.Value.PointMin.Y,
                    relativeBoundRing.Value.PointMax.Y,
                    new[] { boundingRing.Value.PointMin.Y, boundingRing.Value.PointMax.Y }))
            {
                list.Add(PartitioningZones.H);
                _listH.AddFirst((boundingRing, list));
            }

            flagAHG = true;
        }
        else return false;
        
        if ((_nearABC is null ||
             boundingRing.Value.PointMin.Y < _nearABC.Value.boundRing.Value.PointMin.Y) &&
            (flagABC || (list.Count == 1 && (flagA || flagC))))
        {
            _nearABC = (boundingRing, list);
        }
        if ((_nearCDE is null ||
             boundingRing.Value.PointMax.X > _nearCDE.Value.boundRing.Value.PointMax.X) &&
            (flagCDE || (list.Count == 1 && (flagC || flagE))))
        {
            _nearCDE = (boundingRing, list);
        }

        if ((_nearEFG is null ||
             boundingRing.Value.PointMax.Y > _nearEFG.Value.boundRing.Value.PointMax.Y) &&
            (flagEFG || (list.Count == 1 && (flagE || flagG))))
        {
            _nearEFG = (boundingRing, list);
        }

        if ((_nearAHG is null ||
             boundingRing.Value.PointMin.X < _nearAHG.Value.boundRing.Value.PointMin.X) &&
            (flagAHG || (list.Count == 1 && (flagA || flagG))))
        {
            _nearAHG = (boundingRing, list);
        }
        
        return true;
    }

    private bool NotIntersectCheck(BoundingRing relativeBoundRing, BoundingRing boundingRing)
    {
        return boundingRing.PointMin.Y > relativeBoundRing.PointMax.Y
               || Math.Abs(boundingRing.PointMin.Y - relativeBoundRing.PointMax.Y) < 1e-9 ||


               boundingRing.PointMax.X < relativeBoundRing.PointMin.X
               || Math.Abs(boundingRing.PointMax.X - relativeBoundRing.PointMin.X) < 1e-9 ||

               boundingRing.PointMax.Y < relativeBoundRing.PointMin.Y
               || Math.Abs(boundingRing.PointMax.Y - relativeBoundRing.PointMin.Y) < 1e-9 ||


               boundingRing.PointMin.X > relativeBoundRing.PointMax.X
               || Math.Abs(boundingRing.PointMin.X - relativeBoundRing.PointMax.X) < 1e-9;
    }
    private bool NotIntersectCheck(BoundingRing relativeBoundRing, (Coordinate pointMin, Coordinate pointMax) frame)
    {
        return frame.pointMin.Y > relativeBoundRing.PointMax.Y
               || Math.Abs(frame.pointMin.Y - relativeBoundRing.PointMax.Y) < 1e-9 ||


               frame.pointMax.X < relativeBoundRing.PointMin.X
               || Math.Abs(frame.pointMax.X - relativeBoundRing.PointMin.X) < 1e-9 ||

               frame.pointMax.Y < relativeBoundRing.PointMin.Y
               || Math.Abs(frame.pointMax.Y - relativeBoundRing.PointMin.Y) < 1e-9 ||


               frame.pointMin.X > relativeBoundRing.PointMax.X
               || Math.Abs(frame.pointMin.X - relativeBoundRing.PointMax.X) < 1e-9;
    }
    private bool SegmentContainAtLeastOneNumber(double a, double b, double[] arr)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            if ((arr[i] > a && arr[i] < b) || Math.Abs(a - arr[i]) < 1e-9 || Math.Abs(b - arr[i]) < 1e-9)
                return true;
        }
        
        return false;
    }

    private bool PointInsideFrameCheck(Coordinate point, BoundingRing ring)
    {
        var pointMin = ring.PointMin;
        var pointMax = ring.PointMax;
        return point.X < pointMax.X && point.X > pointMin.X && point.Y < pointMax.Y && point.Y > pointMin.Y;
    }
    private bool PointInsideFrameCheck(Coordinate point, (Coordinate pointMin, Coordinate pointMax) frame)
    {
        return point.X < frame.pointMax.X && point.X > frame.pointMin.X && point.Y < frame.pointMax.Y && point.Y > frame.pointMin.Y;
    }
}

            
        