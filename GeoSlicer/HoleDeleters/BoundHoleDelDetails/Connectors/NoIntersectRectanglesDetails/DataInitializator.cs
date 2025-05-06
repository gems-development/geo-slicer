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
        if (!cache.NearRing.TryGetValue(Zones.Abc, out var valueAbc))
            data.AbcCanConnect = false;
        else
        {
            data.LineConnectNearAbcFrame = new LineSegment(
                thisRing.Value.PointUpNode.Elem,
                valueAbc.BoundRing.Value.PointDownNode.Elem);
            
            data.AbcCanConnect = CheckLineIntersectFrame(
                data.LineConnectNearAbcFrame.P0, data.LineConnectNearAbcFrame.P1,
                Zones.Abc,
                Zones.A, Zones.B, Zones.C,
                cache,
                thisRing, 
                intersectsChecker);
        }
        
        if (!cache.NearRing.TryGetValue(Zones.Cde, out var valueCde))
            data.CdeCanConnect = false;
        else
        {
            data.LineConnectNearCdeFrame = new LineSegment(
                thisRing.Value.PointLeftNode.Elem,
                valueCde.BoundRing.Value.PointRightNode.Elem);
            
            data.CdeCanConnect = CheckLineIntersectFrame(
                data.LineConnectNearCdeFrame.P0, data.LineConnectNearCdeFrame.P1,
                Zones.Cde,
                Zones.C, Zones.D, Zones.E,
                cache,
                thisRing, 
                intersectsChecker);
        }
        
        if (!cache.NearRing.TryGetValue(Zones.Efg, out var valueEfg))
            data.EfgCanConnect = false;
        else
        {
            data.LineConnectNearEfgFrame = new LineSegment(
                thisRing.Value.PointDownNode.Elem,
                valueEfg.BoundRing.Value.PointUpNode.Elem);
            
            data.EfgCanConnect = CheckLineIntersectFrame(
                data.LineConnectNearEfgFrame.P0, data.LineConnectNearEfgFrame.P1,
                Zones.Efg,
                Zones.E, Zones.F, Zones.G,
                cache,
                thisRing, 
                intersectsChecker);
        }
        
        if (!cache.NearRing.TryGetValue(Zones.Ahg, out var valueAhg))
            data.AhgCanConnect = false;
        else
        {
            data.LineConnectNearAhgFrame = new LineSegment(
                thisRing.Value.PointRightNode.Elem,
                valueAhg.BoundRing.Value.PointLeftNode.Elem);
            
            data.AhgCanConnect = CheckLineIntersectFrame(
                data.LineConnectNearAhgFrame.P0, data.LineConnectNearAhgFrame.P1,
                Zones.Ahg,
                Zones.A, Zones.H, Zones.G,
                cache, 
                thisRing, 
                intersectsChecker);
        }
    }
    

    
    /// <summary>
    /// Метод проверяет, не пересекает ли линия (firstCoordLineConnect, secondCoordLineConnect)
    /// какой-нибудь прямоугольник, пересекающий объединение зон - zonesUnion
    /// (например если прямоугольник лежит в зонах С и D, и zonesUnion = Abc, то
    /// этот прямоугольник пересекает zonesUnion и он проверяется на пересечение с линией.)
    ///
    /// Предполагается, что zonesUnion равно объединению firstZone, arrangeZone, secondZone.
    /// Прямоугольники, лежащие только в zonesUnion, не учитываются.
    /// </summary>
    /// <returns>True, если искомое пересечение есть, false иначе</returns>
    private static bool CheckLineIntersectFrame(
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
    
    /// <summary>
    ///  Проверяет, выходит ли рамка <paramref name="frame"/> за пределы объединения зон <paramref name="zonesUnion"/>
    /// (то-есть <paramref name="frame"/> либо пересекает прямую, накладываемую на
    /// сторону рамки <paramref name="thisRing"/>, либо лежит ниже ее).
    /// </summary>
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