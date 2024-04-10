using System.Collections.Generic;
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
    private static bool ConnectWithBoundRFrameWhoContainThisRing(
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
    
    
    
    private static LinkedNode<Coordinate> RearrangePoints(
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
}