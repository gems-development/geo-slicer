using System;
using System.Collections.Generic;
using GeoSlicer.HoleDeleters.BoundHoleDelDetails;
using GeoSlicer.HoleDeleters.BoundHoleDelDetails.Connectors;
using GeoSlicer.Utils;
using NetTopologySuite.Geometries;
using GeoSlicer.Utils.BoundRing;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;

namespace GeoSlicer.HoleDeleters;

public class BoundingHoleDeleter
{
    private readonly TraverseDirection _direction;
    private readonly Cache _cache;
    private readonly double _epsilon;
    private readonly NoIntersectRectangles _noIntersectRectangles;
    private readonly IntersectionBoundRFrames _intersectionBoundRFrames = new();
    private readonly IntersectsChecker _intersectChecker;
    private readonly LineService _lineService;

    public BoundingHoleDeleter(
        TraverseDirection direction, double epsilon, IntersectsChecker? checker = null, LineService? lineService = null)
    {
        if (checker is null)
        {
            _intersectChecker =
                new IntersectsChecker(
                    new LinesIntersector(new EpsilonCoordinateComparator(epsilon), new LineService(epsilon), epsilon));
        }
        else
            _intersectChecker = checker;

        if (lineService is null)
        {
            _lineService = new LineService(epsilon);
        }
        else
            _lineService = lineService;
        _direction = direction;
        _noIntersectRectangles = new NoIntersectRectangles(epsilon, _lineService);
        _cache = new Cache(epsilon, _intersectChecker);
        _epsilon = epsilon;
    }

    public Polygon DeleteHoles(Polygon polygon)
    {
        LinkedList<BoundingRing> list = BoundingRing.PolygonToBoundRings(polygon, _direction, new LineService(_epsilon));
        DeleteHoles(list);
        return BoundingRing.BoundRingsToPolygon(list);
    }
    private void DeleteHoles(LinkedList<BoundingRing> listOfHoles)
    {
        var thisRing = listOfHoles.First;
        var pointMinShell = thisRing!.Value.PointMin;
        var pointMaxShell = thisRing.Value.PointMax;

        while (listOfHoles.First!.Next is not null)
        {
            if (thisRing.Next is null)
                thisRing = listOfHoles.First.Next;
            else thisRing = thisRing.Next;
            
            bool isConnected;
            
            if (!_cache.FillListsRelativeRing(thisRing, listOfHoles))
            {
                isConnected = 
                    _noIntersectRectangles.Connect(thisRing, listOfHoles, _cache, _intersectChecker) ||
                    WithIntersectRing.BruteforceConnect(thisRing, listOfHoles, _cache, _intersectChecker);
            }
            else
            {
                isConnected = _intersectionBoundRFrames.BruteforceConnect(thisRing, listOfHoles, _cache, _intersectChecker);
            }

            if (!isConnected)
            {
                Bruteforce.Connect(thisRing, listOfHoles, _cache, _intersectChecker, _lineService, _epsilon);
            }
            
            if (thisRing.Value.PointMin.Equals(pointMinShell) && thisRing.Value.PointMax.Equals(pointMaxShell))
            {
                var buff = thisRing.Value;
                listOfHoles.Remove(thisRing);
                listOfHoles.AddFirst(buff);
                thisRing = listOfHoles.First;
            }
        }
    }
}

            
        