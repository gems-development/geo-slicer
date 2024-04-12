using System;
using System.Collections.Generic;
using GeoSlicer.HoleDeleters.BoundHoleDelDetails.Structures;
using GeoSlicer.Utils.BoundRing;
using NetTopologySuite.Geometries;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails.Connectors.NoIntersectRectanglesDetails;

internal static class FramesContainThisIntersectsChecker
{
    private class Values
    {
        public LinkedNode<Coordinate>? CurrentCoord;
        public LinkedListNode<BoundingRing>? FrameWhoContainsThis;
        public readonly LinkedListNode<BoundingRing> ThisRing;
        public readonly Cache Cache;
        public readonly double Epsilon;

        public Values(LinkedListNode<BoundingRing> thisRing, Cache cache, double epsilon)
        {
            ThisRing = thisRing;
            Cache = cache;
            Epsilon = epsilon;
        }
    }
    //Метод проверяет, пересекают ли линии LineConnectNearAbcFrame, ... , LineConnectNearAhgFrame из data
    //какое-либо кольцо из cache.FramesContainThis(кольца, рамки которых включают в себя рамку кольца thisRing).
    //При проверке может поменять флаги в data и некоторые значения в cache.
    internal static void Check(
        NoIntersectRectangles.Data data,
        LinkedListNode<BoundingRing> thisRing,
        Cache cache,
        double epsilon)
    {
        var shell = cache.FramesContainThis.First!.Value;
        cache.FramesContainThis.Remove(cache.FramesContainThis.First);
        cache.FramesContainThis.AddLast(shell);

        Values values = new Values(thisRing, cache, epsilon);
        foreach (var frameWhoContainThis in cache.FramesContainThis)
        {
            values.FrameWhoContainsThis = frameWhoContainThis;
            
            var startCoord = frameWhoContainThis.Value.PointUpNode;
            values.CurrentCoord = startCoord;
            do
            {
                ConnectsLineIntersectsCheck(
                    Zones.Abc,
                    ref data.AbcCanConnect,
                    data.LineConnectNearAbcFrame,
                    values);
                
                ConnectsLineIntersectsCheck(
                    Zones.Cde,
                    ref data.CdeCanConnect,
                    data.LineConnectNearCdeFrame,
                    values);
                
                ConnectsLineIntersectsCheck(
                    Zones.Efg,
                    ref data.EfgCanConnect,
                    data.LineConnectNearEfgFrame,
                    values);
                
                ConnectsLineIntersectsCheck(
                    Zones.Ahg,
                    ref data.AhgCanConnect,
                    data.LineConnectNearAhgFrame,
                    values);
                
                values.CurrentCoord = values.CurrentCoord.Next;
            } while (!ReferenceEquals(values.CurrentCoord, startCoord)
                     && (data.AbcCanConnect || data.CdeCanConnect || data.EfgCanConnect || data.AhgCanConnect));
        }
    }

    //Метод проверяет, пересекает ли линия (values.CurrentCoord,  values.CurrentCoord.Next)
    //линию lineConnectNearZonesUnion. Сначала метод проверяет - пересекает ли
    //(values.CurrentCoord,  values.CurrentCoord.Next) объединение зон zonesUnion. Если да, то метод уже проверяет
    //пересечение с линией lineConnectNearZonesUnion. Если пересекает, то он добавляет в values.Cache.NearSegmentIntersect
    //линию (values.CurrentCoord,  values.CurrentCoord.Next) и кольцо values.FrameWhoContainsThis, стороной которого
    //является  эта линия.
    private static void ConnectsLineIntersectsCheck(
        Zones zonesUnion,
        ref bool zonesUnionCanConnect,
        LineSegment? lineConnectNearZonesUnion,
        Values values)
    {
        if (zonesUnionCanConnect &&
            CurrentCoordLineIntersectsZonesUnion(zonesUnion, values.CurrentCoord!, values.ThisRing, values.Epsilon))
        {
            if (IntersectsChecker.HasIntersectedSegments(
                    lineConnectNearZonesUnion!.P0, lineConnectNearZonesUnion!.P1,
                    values.CurrentCoord!.Elem, values.CurrentCoord!.Next.Elem))
            {
                values.Cache.NearSegmentIntersect.Remove(zonesUnion);
                
                values.Cache.NearSegmentIntersect.Add(
                    zonesUnion, new RingAndPoint(values.FrameWhoContainsThis!, values.CurrentCoord!));
                
                zonesUnionCanConnect = false;
            }
        }
    }

    private static bool CurrentCoordLineIntersectsZonesUnion(
        Zones zonesUnion,
        LinkedNode<Coordinate> currentCoord,
        LinkedListNode<BoundingRing> thisRing,
        double epsilon)
    {
        if (zonesUnion == Zones.Abc)
            return CompareWithEpsilon(currentCoord.Elem.Y, thisRing.Value.PointMax.Y, epsilon) ||
                   CompareWithEpsilon(currentCoord.Next.Elem.Y, thisRing.Value.PointMax.Y, epsilon);
        
        if (zonesUnion == Zones.Cde)
            return CompareWithEpsilon(thisRing.Value.PointMin.X, currentCoord.Elem.X, epsilon) ||
                   CompareWithEpsilon(thisRing.Value.PointMin.X, currentCoord.Next.Elem.X, epsilon);
        
        if (zonesUnion == Zones.Efg)
            return CompareWithEpsilon(thisRing.Value.PointMin.Y, currentCoord.Elem.Y, epsilon) ||
                   CompareWithEpsilon(thisRing.Value.PointMin.Y, currentCoord.Next.Elem.Y, epsilon);
        
        //zonesUnion == Zones.Ahg
        return CompareWithEpsilon(currentCoord.Elem.X, thisRing.Value.PointMax.X, epsilon) ||
               CompareWithEpsilon(currentCoord.Next.Elem.X, thisRing.Value.PointMax.X, epsilon);
    }

    private static bool CompareWithEpsilon(double a, double b, double epsilon)
    {
        return a > b || Math.Abs(a - b) < epsilon;
    }
}