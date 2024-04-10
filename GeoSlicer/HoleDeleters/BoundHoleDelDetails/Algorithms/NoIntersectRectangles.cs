using System;
using System.Collections.Generic;
using GeoSlicer.HoleDeleters.BoundHoleDelDetails.Structures;
using GeoSlicer.Utils.BoundRing;
using NetTopologySuite.Geometries;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails.Algorithms;

internal class NoIntersectRectangles
{
    private bool _abcCanConnect;
    private bool _cdeCanConnect;
    private bool _efgCanConnect;
    private bool _ahgCanConnect;
    private Coordinate? _firstLineCoordConnectNearAbc;
    private Coordinate? _secondLineCoordConnectNearAbc;
    private Coordinate? _firstLineCoordConnectNearCde;
    private Coordinate? _secondLineCoordConnectNearCde;
    private Coordinate? _firstLineCoordConnectNearEfg;
    private Coordinate? _secondLineCoordConnectNearEfg;
    private Coordinate? _firstLineCoordConnectNearAhg;
    private Coordinate? _secondLineCoordConnectNearAhg;

    private void Clear()
    {
        _firstLineCoordConnectNearAbc = null;
        _secondLineCoordConnectNearAbc = null;
        _firstLineCoordConnectNearCde = null;
        _secondLineCoordConnectNearCde = null;
        _firstLineCoordConnectNearEfg = null;
        _secondLineCoordConnectNearEfg = null;
        _firstLineCoordConnectNearAhg = null;
        _secondLineCoordConnectNearAhg = null;
    }

    private void FillDetails(
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles,
        Cache cache,
        double epsilon)
    {
        Clear();
        if (!cache.NearRing.ContainsKey(Zones.Abc))
            _abcCanConnect = false;
        else
        {
            _firstLineCoordConnectNearAbc = thisRing.Value.PointUpNode.Elem;
            _secondLineCoordConnectNearAbc = cache.NearRing[Zones.Abc].BoundRing.Value.PointDownNode.Elem;
            _abcCanConnect = LineIntersectFrameCheck(
                _firstLineCoordConnectNearAbc, _secondLineCoordConnectNearAbc,
                Zones.Abc,
                Zones.A, Zones.B, Zones.C,
                cache,
                thisRing);
        }
        
        if (!cache.NearRing.ContainsKey(Zones.Cde))
            _cdeCanConnect = false;
        else
        {
            _firstLineCoordConnectNearCde = thisRing.Value.PointLeftNode.Elem;
            _secondLineCoordConnectNearCde = cache.NearRing[Zones.Cde].BoundRing.Value.PointRightNode.Elem;
            _cdeCanConnect = LineIntersectFrameCheck(
                _firstLineCoordConnectNearCde, _secondLineCoordConnectNearCde,
                Zones.Cde,
                Zones.C, Zones.D, Zones.E,
                cache,
                thisRing);
        }
        
        if (!cache.NearRing.ContainsKey(Zones.Efg))
            _efgCanConnect = false;
        else
        {
            _firstLineCoordConnectNearEfg = thisRing.Value.PointDownNode.Elem;
            _secondLineCoordConnectNearEfg = cache.NearRing[Zones.Efg].BoundRing.Value.PointUpNode.Elem;
            _efgCanConnect = LineIntersectFrameCheck(
                _firstLineCoordConnectNearEfg, _secondLineCoordConnectNearEfg,
                Zones.Efg,
                Zones.E, Zones.F, Zones.G,
                cache,
                thisRing);
        }
        
        if (!cache.NearRing.ContainsKey(Zones.Ahg))
            _ahgCanConnect = false;
        else
        {
            _firstLineCoordConnectNearAhg = thisRing.Value.PointRightNode.Elem;
            _secondLineCoordConnectNearAhg = cache.NearRing[Zones.Ahg].BoundRing.Value.PointLeftNode.Elem;
            _ahgCanConnect = LineIntersectFrameCheck(
                _firstLineCoordConnectNearAhg, _secondLineCoordConnectNearAhg,
                Zones.Ahg,
                Zones.A, Zones.H, Zones.G,
                cache, 
                thisRing);
        }
    }
    
    internal bool Connect(
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles,
        Cache cache,
        double epsilon)
    {
        FillDetails(thisRing, listOfHoles, cache, epsilon);
        
        var shell = cache.FramesContainThis.First!.Value;
        cache.FramesContainThis.Remove(cache.FramesContainThis.First);
        cache.FramesContainThis.AddLast(shell);
        foreach (var frameWhoContainThis in cache.FramesContainThis)
        {
            var start = frameWhoContainThis.Value.PointUpNode;
            var buffer = start;
            do
            {
                if (_abcCanConnect &&
                    (buffer.Elem.Y > thisRing.Value.PointMax.Y
                     || Math.Abs(buffer.Elem.Y - thisRing.Value.PointMax.Y) < 1e-9
                     || buffer.Next.Elem.Y > thisRing.Value.PointMax.Y
                     || Math.Abs(buffer.Next.Elem.Y - thisRing.Value.PointMax.Y) < 1e-9))
                {
                    if (IntersectsChecker.HasIntersectedSegments(_firstLineCoordConnectNearAbc!, _secondLineCoordConnectNearAbc!, buffer.Elem,
                            buffer.Next.Elem))
                    {
                        cache.NearSegmentIntersect.Remove(Zones.Abc);
                        cache.NearSegmentIntersect.Add(Zones.Abc, new RingAndPoint(frameWhoContainThis, buffer));
                        _abcCanConnect = false;

                    }
                }

                if (_cdeCanConnect &&
                    (buffer.Elem.X < thisRing.Value.PointMin.X
                     || Math.Abs(buffer.Elem.X - thisRing.Value.PointMin.X) < 1e-9
                     || buffer.Next.Elem.X < thisRing.Value.PointMin.X
                     || Math.Abs(buffer.Next.Elem.X - thisRing.Value.PointMin.X) < 1e-9))
                {
                    if (IntersectsChecker.HasIntersectedSegments(_firstLineCoordConnectNearCde!, _secondLineCoordConnectNearCde!, buffer.Elem,
                            buffer.Next.Elem))
                    {
                        cache.NearSegmentIntersect.Remove(Zones.Cde);
                        cache.NearSegmentIntersect.Add(Zones.Cde, new RingAndPoint(frameWhoContainThis, buffer));
                        _cdeCanConnect = false;
                    }
                }

                if (_efgCanConnect &&
                    (buffer.Elem.Y < thisRing.Value.PointMin.Y
                     || Math.Abs(buffer.Elem.Y - thisRing.Value.PointMin.Y) < 1e-9
                     || buffer.Next.Elem.Y < thisRing.Value.PointMin.Y
                     || Math.Abs(buffer.Next.Elem.Y - thisRing.Value.PointMin.Y) < 1e-9))
                {
                    if (IntersectsChecker.HasIntersectedSegments(_firstLineCoordConnectNearEfg!, _secondLineCoordConnectNearEfg!, buffer.Elem,
                            buffer.Next.Elem))
                    {
                        cache.NearSegmentIntersect.Remove(Zones.Efg);
                        cache.NearSegmentIntersect.Add(Zones.Efg, new RingAndPoint(frameWhoContainThis, buffer));
                        _efgCanConnect = false;
                    }
                }

                if (_ahgCanConnect &&
                    (buffer.Elem.X > thisRing.Value.PointMax.X
                     || Math.Abs(buffer.Elem.X - thisRing.Value.PointMax.X) < 1e-9
                     || buffer.Next.Elem.X > thisRing.Value.PointMax.X
                     || Math.Abs(buffer.Next.Elem.X - thisRing.Value.PointMax.X) < 1e-9))
                {
                    if (IntersectsChecker.HasIntersectedSegments(_firstLineCoordConnectNearAhg!, _secondLineCoordConnectNearAhg!, buffer.Elem,
                            buffer.Next.Elem))
                    {
                        cache.NearSegmentIntersect.Remove(Zones.Ahg);
                        cache.NearSegmentIntersect.Add(Zones.Ahg, new RingAndPoint(frameWhoContainThis, buffer));
                        _ahgCanConnect = false;
                    }
                }

                buffer = buffer.Next;
            } while (!ReferenceEquals(buffer, start)
                     && (_abcCanConnect || _cdeCanConnect || _efgCanConnect || _ahgCanConnect));
        }

        Coordinate oldPointMin = thisRing.Value.PointMin;
        Coordinate oldPointMax = thisRing.Value.PointMax;
        if (_abcCanConnect)
        {
            thisRing.Value.ConnectBoundRings(
                cache.NearRing[Zones.Abc].BoundRing.Value,
                thisRing.Value.PointUpNode,
                cache.NearRing[Zones.Abc].BoundRing.Value.PointDownNode);
            
            listOfHoles.Remove(cache.NearRing[Zones.Abc].BoundRing);
        }

        if (_cdeCanConnect && oldPointMin.Equals(thisRing.Value.PointMin))
        {
            thisRing.Value.ConnectBoundRings(
                cache.NearRing[Zones.Cde].BoundRing.Value,
                thisRing.Value.PointLeftNode,
                cache.NearRing[Zones.Cde].BoundRing.Value.PointRightNode);

            listOfHoles.Remove(cache.NearRing[Zones.Cde].BoundRing);
        }

        if (_efgCanConnect && oldPointMin.Equals(thisRing.Value.PointMin))
        {
            thisRing.Value.ConnectBoundRings(
                cache.NearRing[Zones.Efg].BoundRing.Value,
                thisRing.Value.PointDownNode,
                cache.NearRing[Zones.Efg].BoundRing.Value.PointUpNode);
            listOfHoles.Remove(cache.NearRing[Zones.Efg].BoundRing);
        }

        if (_ahgCanConnect && oldPointMax.Equals(thisRing.Value.PointMax))
        {
            thisRing.Value.ConnectBoundRings(
                cache.NearRing[Zones.Ahg].BoundRing.Value,
                thisRing.Value.PointRightNode,
                cache.NearRing[Zones.Ahg].BoundRing.Value.PointLeftNode);
            listOfHoles.Remove(cache.NearRing[Zones.Ahg].BoundRing);
        }

        return _abcCanConnect || _cdeCanConnect || _efgCanConnect || _ahgCanConnect;
    }
    
    //Метод проверяет, не пересекает ли линия (firstCoordLineConnect, secondCoordLineConnect)
    //какой-нибудь прямоугольник, пересекающий объединение зон - zonesUnion
    //(например если прямоугольник лежит в зонах С и D, и zonesUnion = Abc, то
    //этот прямоугольник пересекает zonesUnion и он проверяется на пересечение с линией.)
    
    //Предполагается, что zonesUnion равно объединению firstZone, arrangeZone, secondZone.
    //Прямоугольники, лежащие только в zonesUnion, не учитываются.
    //true - если такого пересечения нет, false - иначе.
    private static bool LineIntersectFrameCheck(
        Coordinate firstCoordLineConnect,
        Coordinate secondCoordLineConnect,
        Zones zonesUnion,
        Zones firstZone,
        Zones arrangeZone,
        Zones secondZone,
        Cache cache,
        LinkedListNode<BoundingRing> thisRing)
    {
        bool flag = true;
        if (cache.NearRing[zonesUnion].Zones.Contains(secondZone))
        {
            flag = CheckIntersectInCurrentZone(
                firstCoordLineConnect, secondCoordLineConnect,
                zonesUnion, arrangeZone, secondZone,
                cache,
                thisRing);
        }

        if (cache.NearRing[zonesUnion].Zones.Contains(firstZone) && flag)
        {
            flag = CheckIntersectInCurrentZone(
                firstCoordLineConnect, secondCoordLineConnect,
                zonesUnion, arrangeZone, firstZone,
                cache,
                thisRing);
        }

        return flag;
    }

    private static bool CheckIntersectInCurrentZone(
        Coordinate firstCoordLineConnect,
        Coordinate secondCoordLineConnect,
        Zones zonesUnion,
        Zones arrangeZone,
        Zones currentZone,
        Cache cache,
        LinkedListNode<BoundingRing> thisRing)
    {
        foreach (var frame in cache.RingsInZone[currentZone])
        {
            if (!frame.Zones.Contains(arrangeZone)
                && CheckIntersectFrameWithZonesUnion(frame, thisRing, zonesUnion)
                && IntersectsChecker.HasIntersectsBoundRFrame(frame.BoundRing.Value, firstCoordLineConnect,
                    secondCoordLineConnect) &&
                !ReferenceEquals(frame.BoundRing.Value, cache.NearRing[zonesUnion].BoundRing.Value))
            {
                return false;
            }
        }

        return true;
    }
    
    //todo разобраться с вычислениями по epsilon
    
    //Если рамка frame выходит за пределы объединения зон zonesUnion
    //(то есть frame либо пересекает прямую, накладываемую на сторону рамки thisRing, либо лежит ниже ее),
    //то функция возвращает true. False - иначе.
    private static bool CheckIntersectFrameWithZonesUnion
        (RingAndZones frame, LinkedListNode<BoundingRing> thisRing, Zones zonesUnion)
    {
        if (zonesUnion == Zones.Abc)
            return frame.BoundRing.Value.PointMin.Y < thisRing.Value.PointMax.Y;
        
        if (zonesUnion == Zones.Cde)
            return frame.BoundRing.Value.PointMax.X > thisRing.Value.PointMin.X;
        
        if (zonesUnion == Zones.Efg)
            return frame.BoundRing.Value.PointMax.Y > thisRing.Value.PointMin.Y;
        
        return frame.BoundRing.Value.PointMin.X < thisRing.Value.PointMax.X;
    }
}