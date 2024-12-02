using System.Collections.Generic;
using GeoSlicer.HoleDeleters.BoundHoleDelDetails.Structures;
using GeoSlicer.Utils.BoundRing;
using NetTopologySuite.Geometries;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails.Connectors.NoIntersectRectanglesDetails;

internal static class DataInitializer
{
    private static void Clear(NoIntersectRectangles.Data data)
    {
        data.LineConnectNearAbcFrame = null;
        data.LineConnectNearCdeFrame = null;
        data.LineConnectNearEfgFrame = null;
        data.LineConnectNearAhgFrame = null;
    }
    internal static void Initialize(
        NoIntersectRectangles.Data data,
        LinkedListNode<BoundingRing> thisRing,
        Cache cache, 
        IntersectsChecker intersectsChecker)
    {
        Clear(data);
        if (!cache.NearRing.ContainsKey(Zones.Abc))
            data.AbcCanConnect = false;
        else
        {
            data.LineConnectNearAbcFrame = new LineSegment(
                thisRing.Value.PointUpNode.Elem,
                cache.NearRing[Zones.Abc].BoundRing.Value.PointDownNode.Elem);
            
            data.AbcCanConnect = LineIntersectFrameCheck(
                data.LineConnectNearAbcFrame.P0, data.LineConnectNearAbcFrame.P1,
                Zones.Abc,
                Zones.A, Zones.B, Zones.C,
                cache,
                thisRing, 
                intersectsChecker);
        }
        
        if (!cache.NearRing.ContainsKey(Zones.Cde))
            data.CdeCanConnect = false;
        else
        {
            data.LineConnectNearCdeFrame = new LineSegment(
                thisRing.Value.PointLeftNode.Elem,
                cache.NearRing[Zones.Cde].BoundRing.Value.PointRightNode.Elem);
            
            data.CdeCanConnect = LineIntersectFrameCheck(
                data.LineConnectNearCdeFrame.P0, data.LineConnectNearCdeFrame.P1,
                Zones.Cde,
                Zones.C, Zones.D, Zones.E,
                cache,
                thisRing, 
                intersectsChecker);
        }
        
        if (!cache.NearRing.ContainsKey(Zones.Efg))
            data.EfgCanConnect = false;
        else
        {
            data.LineConnectNearEfgFrame = new LineSegment(
                thisRing.Value.PointDownNode.Elem,
                cache.NearRing[Zones.Efg].BoundRing.Value.PointUpNode.Elem);
            
            data.EfgCanConnect = LineIntersectFrameCheck(
                data.LineConnectNearEfgFrame.P0, data.LineConnectNearEfgFrame.P1,
                Zones.Efg,
                Zones.E, Zones.F, Zones.G,
                cache,
                thisRing, 
                intersectsChecker);
        }
        
        if (!cache.NearRing.ContainsKey(Zones.Ahg))
            data.AhgCanConnect = false;
        else
        {
            data.LineConnectNearAhgFrame = new LineSegment(
                thisRing.Value.PointRightNode.Elem,
                cache.NearRing[Zones.Ahg].BoundRing.Value.PointLeftNode.Elem);
            
            data.AhgCanConnect = LineIntersectFrameCheck(
                data.LineConnectNearAhgFrame.P0, data.LineConnectNearAhgFrame.P1,
                Zones.Ahg,
                Zones.A, Zones.H, Zones.G,
                cache, 
                thisRing, 
                intersectsChecker);
        }
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
        LinkedListNode<BoundingRing> thisRing,
        IntersectsChecker intersectsChecker)
    {
        bool flag = true;
        if (cache.NearRing[zonesUnion].Zones.Contains(secondZone))
        {
            flag = CheckIntersectInCurrentZone(
                firstCoordLineConnect, secondCoordLineConnect,
                zonesUnion, arrangeZone, secondZone,
                cache,
                thisRing, 
                intersectsChecker);
        }

        if (cache.NearRing[zonesUnion].Zones.Contains(firstZone) && flag)
        {
            flag = CheckIntersectInCurrentZone(
                firstCoordLineConnect, secondCoordLineConnect,
                zonesUnion, arrangeZone, firstZone,
                cache,
                thisRing, 
                intersectsChecker);
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
        LinkedListNode<BoundingRing> thisRing,
        IntersectsChecker intersectsChecker)
    {
        foreach (var frame in cache.RingsInZone[currentZone])
        {
            if (!frame.Zones.Contains(arrangeZone)
                && CheckIntersectFrameWithZonesUnion(frame, thisRing, zonesUnion)
                && intersectsChecker.HasIntersectsBoundRFrame(frame.BoundRing.Value, firstCoordLineConnect,
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