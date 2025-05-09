﻿using System.Collections.Generic;
using GeoSlicer.HoleDeleters.BoundHoleDelDetails.Structures;
using GeoSlicer.Utils.BoundRing;
using NetTopologySuite.Geometries;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails.Connectors.NoIntersectRectanglesDetails;

internal static class FramesContainThisIntersectsChecker
{
    private class Values
    {
        public LinkedNode<Coordinate>? CurrentCoord;
        public LinkedListNode<LinkedListNode<BoundingRing>>? FrameWhoContainsThis;
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

    /// <summary>
    /// Метод проверяет, пересекают ли линии LineConnectNearAbcFrame, ... , LineConnectNearAhgFrame
    /// из <paramref name="data"/> какое-либо кольцо из <paramref name="cache.FramesContainThis"/>
    /// (кольца, рамки которых включают в себя рамку кольца <paramref name="thisRing"/>).
    /// При проверке может поменять флаги в data и некоторые значения в <paramref name="cache"/>.
    /// </summary>
    internal static void Check(
        NoIntersectRectangles.Data data,
        LinkedListNode<BoundingRing> thisRing,
        Cache cache,
        IntersectsChecker intersectsChecker)
    {
        var shell = cache.FramesContainThis.First!.Value;
        cache.FramesContainThis.Remove(cache.FramesContainThis.First);
        cache.FramesContainThis.AddLast(shell);

        Values values = new Values(thisRing, cache, data.Epsilon);
        var frameWhoContainThis = cache.FramesContainThis.First;
        while (frameWhoContainThis is not null)
        {
            values.FrameWhoContainsThis = frameWhoContainThis;

            var startCoord = frameWhoContainThis.Value.Value.PointUpNode;
            values.CurrentCoord = startCoord;
            do
            {
                CheckConnectsLineIntersectsCheck(
                    Zones.Abc,
                    ref data.AbcCanConnect,
                    data.LineConnectNearAbcFrame,
                    values,
                    intersectsChecker);

                CheckConnectsLineIntersectsCheck(
                    Zones.Cde,
                    ref data.CdeCanConnect,
                    data.LineConnectNearCdeFrame,
                    values,
                    intersectsChecker);

                CheckConnectsLineIntersectsCheck(
                    Zones.Efg,
                    ref data.EfgCanConnect,
                    data.LineConnectNearEfgFrame,
                    values,
                    intersectsChecker);

                CheckConnectsLineIntersectsCheck(
                    Zones.Ahg,
                    ref data.AhgCanConnect,
                    data.LineConnectNearAhgFrame,
                    values,
                    intersectsChecker);

                values.CurrentCoord = values.CurrentCoord.Next;
            } while (!ReferenceEquals(values.CurrentCoord, startCoord)
                     && (data.AbcCanConnect || data.CdeCanConnect || data.EfgCanConnect || data.AhgCanConnect));

            frameWhoContainThis = frameWhoContainThis.Next;
        }
    }

    /// <summary>
    /// Метод проверяет, пересекает ли линия (<paramref name="values.CurrentCoord"/>,
    /// <paramref name="values.CurrentCoord.Next"/>)
    /// линию <paramref name="lineConnectNearZonesUnion"/>. Сначала метод проверяет - пересекает ли
    /// (<paramref name="values.CurrentCoord"/>, <paramref name="values.CurrentCoord.Next"/>)
    /// объединение зон <paramref name="zonesUnion"/>. Если да, то метод уже проверяет
    /// пересечение с линией <paramref name="lineConnectNearZonesUnion"/>. Если пересекает,
    /// то он добавляет в <paramref name="values.Cache.NearSegmentIntersect"/>
    /// линию (<paramref name="values.CurrentCoord"/>, <paramref name="values.CurrentCoord.Next"/>)
    /// и кольцо <paramref name="values.FrameWhoContainsThis"/>, стороной которого является эта линия.
    /// </summary>
    private static void CheckConnectsLineIntersectsCheck(
        Zones zonesUnion,
        ref bool zonesUnionCanConnect,
        LineSegment? lineConnectNearZonesUnion,
        Values values,
        IntersectsChecker intersectsChecker)
    {
        if (zonesUnionCanConnect &&
            CheckCurrentCoordLineIntersectsZonesUnion(zonesUnion, values.CurrentCoord!, values.ThisRing,
                values.Epsilon))
        {
            if (intersectsChecker.HasIntersectedSegments(
                    lineConnectNearZonesUnion!.P0, lineConnectNearZonesUnion.P1,
                    values.CurrentCoord!.Elem, values.CurrentCoord!.Next.Elem))
            {
                values.Cache.NearSegmentIntersect.Remove(zonesUnion);

                values.Cache.NearSegmentIntersect.Add(
                    zonesUnion, new RingAndPoint(values.FrameWhoContainsThis!, values.CurrentCoord!));

                zonesUnionCanConnect = false;
            }
        }
    }

    private static bool CheckCurrentCoordLineIntersectsZonesUnion(
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

        // zonesUnion == Zones.Ahg
        return CompareWithEpsilon(currentCoord.Elem.X, thisRing.Value.PointMax.X, epsilon) ||
               CompareWithEpsilon(currentCoord.Next.Elem.X, thisRing.Value.PointMax.X, epsilon);
    }

    private static bool CompareWithEpsilon(double a, double b, double epsilon)
    {
        return a > b || b - a > epsilon;
    }
}