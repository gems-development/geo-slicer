using System.Collections.Generic;
using GeoSlicer.HoleDeleters.BoundHoleDelDetails;
using GeoSlicer.HoleDeleters.BoundHoleDelDetails.Algorithms;
using GeoSlicer.Utils;
using NetTopologySuite.Geometries;
using GeoSlicer.Utils.BoundRing;

namespace GeoSlicer.HoleDeleters;

public class BoundingHoleDeleter
{
    private readonly TraverseDirection _direction;
    private readonly Cache _cache;
    private readonly double _epsilon;

    public BoundingHoleDeleter(TraverseDirection direction, double epsilon)
    {
        _direction = direction;
        _cache = new Cache(epsilon);
        _epsilon = epsilon;
    }

    public Polygon DeleteHoles(Polygon polygon)
    {
        LinkedList<BoundingRing> list = BoundingRing.PolygonToBoundRings(polygon, _direction);
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
                    NoIntersectRectangles.Connect(thisRing, listOfHoles, _cache, _epsilon) ||
                    WithIntersectRing.BruteforceConnect(thisRing, listOfHoles, _cache);
            }
            else
            {
                isConnected = IntersectionBoundRFrames.BruteforceConnect(thisRing, listOfHoles, _cache);
            }

            if (!isConnected)
            {
                BruteforceConnector.Connect(thisRing, listOfHoles, _cache);
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

            
        