using System.Collections.Generic;
using System.Linq;
using GeoSlicer.HoleDeleters.BoundHoleDelDetails.Structures;
using GeoSlicer.Utils.BoundRing;
using NetTopologySuite.Geometries;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails.Algorithms;

internal static class WithIntersectRing
{
    //todo нахождение прямоугольника, соединение с которым не пересекает другие 
    internal static bool BruteforceConnect(
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
        if (cache.NearSegmentIntersect.TryGetValue(Zones.Abc, out var ringAndPoint))
        {
            bool flag = ConnectWithBoundRFrameWhoContainThisRing(ringAndPoint, thisRing, listOfHoles, Zones.Abc, cache);
            if (flag)
                return true;
        }
        if (cache.NearSegmentIntersect.TryGetValue(Zones.Cde, out ringAndPoint))
        {
            bool flag = ConnectWithBoundRFrameWhoContainThisRing(ringAndPoint, thisRing, listOfHoles, Zones.Cde, cache);
            if (flag)
                return true;
        }
        if (cache.NearSegmentIntersect.TryGetValue(Zones.Efg, out ringAndPoint))
        {
            bool flag = ConnectWithBoundRFrameWhoContainThisRing(ringAndPoint, thisRing, listOfHoles, Zones.Efg, cache);
            if (flag)
                return true;
        }
        if (cache.NearSegmentIntersect.TryGetValue(Zones.Ahg, out ringAndPoint))
        {
            bool flag = ConnectWithBoundRFrameWhoContainThisRing(ringAndPoint, thisRing, listOfHoles, Zones.Ahg, cache);
            if (flag)
                return true;
        }

        return false;
    }
    
    
    
    //todo добавить проверку на пересечение соединения с прямоугольниками
    //todo рассмотреть ситуацию когда все ближайшие треугольники равны null (_nearAbc и другие подобные)
    private static bool ConnectWithBoundRFrameWhoContainThisRing(
        RingAndPoint? nearSegmentIntersect,
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles, 
        Zones zones, 
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
        if (zones == Zones.Abc)
            connectCoordThisR = thisRing.Value.PointUpNode;
        else if (zones == Zones.Cde)
            connectCoordThisR = thisRing.Value.PointLeftNode;
        else if (zones == Zones.Efg)
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
}