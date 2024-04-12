using System.Collections.Generic;
using GeoSlicer.HoleDeleters.BoundHoleDelDetails.Connectors.NoIntersectRectanglesDetails;
using GeoSlicer.Utils.BoundRing;
using NetTopologySuite.Geometries;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails.Connectors;

internal class NoIntersectRectangles
{
    internal class Data
    {
        public bool AbcCanConnect;
        public bool CdeCanConnect;
        public bool EfgCanConnect;
        public bool AhgCanConnect;
        public LineSegment? LineConnectNearAbcFrame;
        public LineSegment? LineConnectNearCdeFrame;
        public LineSegment? LineConnectNearEfgFrame;
        public LineSegment? LineConnectNearAhgFrame;
    }

    private readonly Data _data = new ();
    internal bool Connect(
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles,
        Cache cache,
        double epsilon)
    {
        DataInitializer.Initialize(_data, thisRing, cache, epsilon);
        
        FramesContainThisIntersectsChecker.Check(_data, thisRing, cache, epsilon);

        Coordinate oldPointMin = thisRing.Value.PointMin;
        Coordinate oldPointMax = thisRing.Value.PointMax;
        
        if (_data.AbcCanConnect)
        {
            thisRing.Value.ConnectBoundRings(
                cache.NearRing[Zones.Abc].BoundRing.Value,
                thisRing.Value.PointUpNode,
                cache.NearRing[Zones.Abc].BoundRing.Value.PointDownNode);
            
            listOfHoles.Remove(cache.NearRing[Zones.Abc].BoundRing);
        }

        if (_data.CdeCanConnect && oldPointMin.Equals(thisRing.Value.PointMin))
        {
            thisRing.Value.ConnectBoundRings(
                cache.NearRing[Zones.Cde].BoundRing.Value,
                thisRing.Value.PointLeftNode,
                cache.NearRing[Zones.Cde].BoundRing.Value.PointRightNode);

            listOfHoles.Remove(cache.NearRing[Zones.Cde].BoundRing);
        }

        if (_data.EfgCanConnect && oldPointMin.Equals(thisRing.Value.PointMin))
        {
            thisRing.Value.ConnectBoundRings(
                cache.NearRing[Zones.Efg].BoundRing.Value,
                thisRing.Value.PointDownNode,
                cache.NearRing[Zones.Efg].BoundRing.Value.PointUpNode);
            listOfHoles.Remove(cache.NearRing[Zones.Efg].BoundRing);
        }

        if (_data.AhgCanConnect && oldPointMax.Equals(thisRing.Value.PointMax))
        {
            thisRing.Value.ConnectBoundRings(
                cache.NearRing[Zones.Ahg].BoundRing.Value,
                thisRing.Value.PointRightNode,
                cache.NearRing[Zones.Ahg].BoundRing.Value.PointLeftNode);
            listOfHoles.Remove(cache.NearRing[Zones.Ahg].BoundRing);
        }

        return _data.AbcCanConnect || _data.CdeCanConnect || _data.EfgCanConnect || _data.AhgCanConnect;
    }
}