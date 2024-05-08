using System.Collections.Generic;
using GeoSlicer.HoleDeleters.BoundHoleDelDetails.Connectors.NoIntersectRectanglesDetails;
using GeoSlicer.Utils;
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
        public double Epsilon;
    }

    private LineService _lineService;
    private readonly Data _data = new ();

    public NoIntersectRectangles(double epsilon, LineService lineService)
    {
        _lineService = lineService;
        _data.Epsilon = epsilon;
    }

    internal bool Connect(
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles,
        Cache cache, 
        IntersectsChecker intersectsChecker)
    {
        DataInitializer.Initialize(_data, thisRing, cache, intersectsChecker);
        
        FramesContainThisIntersectsChecker.Check(_data, thisRing, cache, intersectsChecker);

        Coordinate oldPointMin = thisRing.Value.PointMin;
        Coordinate oldPointMax = thisRing.Value.PointMax;
        
        if (_data.AbcCanConnect)
        {
            Connector.Connect(
                thisRing,
                cache.NearRing[Zones.Abc].BoundRing, 
                thisRing.Value.PointUpNode,
                cache.NearRing[Zones.Abc].BoundRing.Value.PointDownNode,
                listOfHoles, 
                Zones.Abc, _data.Epsilon, _lineService);
        }

        if (_data.CdeCanConnect && oldPointMin.Equals(thisRing.Value.PointMin))
        {
            Connector.Connect(
                thisRing, 
                cache.NearRing[Zones.Cde].BoundRing, 
                thisRing.Value.PointLeftNode, 
                cache.NearRing[Zones.Cde].BoundRing.Value.PointRightNode,
                listOfHoles, 
                Zones.Cde, _data.Epsilon, _lineService);
        }

        if (_data.EfgCanConnect && oldPointMin.Equals(thisRing.Value.PointMin))
        {
            Connector.Connect(
                thisRing, 
                cache.NearRing[Zones.Efg].BoundRing, 
                thisRing.Value.PointDownNode, 
                cache.NearRing[Zones.Efg].BoundRing.Value.PointUpNode,
                listOfHoles, 
                Zones.Efg, _data.Epsilon, _lineService);
        }

        if (_data.AhgCanConnect && oldPointMax.Equals(thisRing.Value.PointMax))
        {
            Connector.Connect(
                thisRing, 
                cache.NearRing[Zones.Ahg].BoundRing, 
                thisRing.Value.PointRightNode, 
                cache.NearRing[Zones.Ahg].BoundRing.Value.PointLeftNode,
                listOfHoles, 
                Zones.Ahg, _data.Epsilon, _lineService);
        }

        return _data.AbcCanConnect || _data.CdeCanConnect || _data.EfgCanConnect || _data.AhgCanConnect;
    }
}