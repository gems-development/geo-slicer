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
            Connector.Connect(
                thisRing,
                cache.NearRing[Zones.Abc].BoundRing, 
                thisRing.Value.PointUpNode,
                cache.NearRing[Zones.Abc].BoundRing.Value.PointDownNode,
                listOfHoles);
        }

        if (_data.CdeCanConnect && oldPointMin.Equals(thisRing.Value.PointMin))
        {
            Connector.Connect(
                thisRing, 
                cache.NearRing[Zones.Cde].BoundRing, 
                thisRing.Value.PointLeftNode, 
                cache.NearRing[Zones.Cde].BoundRing.Value.PointRightNode,
                listOfHoles);
        }

        if (_data.EfgCanConnect && oldPointMin.Equals(thisRing.Value.PointMin))
        {
            Connector.Connect(
                thisRing, 
                cache.NearRing[Zones.Efg].BoundRing, 
                thisRing.Value.PointDownNode, 
                cache.NearRing[Zones.Efg].BoundRing.Value.PointUpNode,
                listOfHoles);
        }

        if (_data.AhgCanConnect && oldPointMax.Equals(thisRing.Value.PointMax))
        {
            Connector.Connect(
                thisRing, 
                cache.NearRing[Zones.Ahg].BoundRing, 
                thisRing.Value.PointRightNode, 
                cache.NearRing[Zones.Ahg].BoundRing.Value.PointLeftNode,
                listOfHoles);
        }

        return _data.AbcCanConnect || _data.CdeCanConnect || _data.EfgCanConnect || _data.AhgCanConnect;
    }
}