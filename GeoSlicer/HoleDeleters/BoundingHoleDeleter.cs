using System;
using System.Collections.Generic;
using GeoSlicer.HoleDeleters.BoundHoleDelDetails;
using GeoSlicer.HoleDeleters.BoundHoleDelDetails.Connectors;
using GeoSlicer.Utils;
using NetTopologySuite.Geometries;
using GeoSlicer.Utils.BoundRing;

namespace GeoSlicer.HoleDeleters;

public class BoundingHoleDeleter
{
    private readonly TraverseDirection _direction;
    private readonly Cache _cache;
    private readonly double _epsilon;
    private readonly NoIntersectRectangles _noIntersectRectangles;
    private readonly IntersectionBoundRFrames _intersectionBoundRFrames = new();

    public BoundingHoleDeleter(TraverseDirection direction, double epsilon)
    {
        _direction = direction;
        _noIntersectRectangles = new NoIntersectRectangles(epsilon);
        _cache = new Cache(epsilon);
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

        int m = 0;
        while (listOfHoles.First!.Next is not null)
        {
            if (m == 2)
                Console.WriteLine("error");
            if (thisRing.Next is null)
                thisRing = listOfHoles.First.Next;
            else thisRing = thisRing.Next;
            
            bool isConnected;
            
            if (!_cache.FillListsRelativeRing(thisRing, listOfHoles))
            {
                isConnected = 
                    _noIntersectRectangles.Connect(thisRing, listOfHoles, _cache) ||
                    WithIntersectRing.BruteforceConnect(thisRing, listOfHoles, _cache);
            }
            else
            {
                isConnected = _intersectionBoundRFrames.BruteforceConnect(thisRing, listOfHoles, _cache);
            }

            if (!isConnected)
            {
                Bruteforce.Connect(thisRing, listOfHoles, _cache);
            }
            
            if (thisRing.Value.PointMin.Equals(pointMinShell) && thisRing.Value.PointMax.Equals(pointMaxShell))
            {
                var buff = thisRing.Value;
                listOfHoles.Remove(thisRing);
                listOfHoles.AddFirst(buff);
                thisRing = listOfHoles.First;
            }
            string user = "User";
            string fileName = "C:\\Users\\" + user + "\\Downloads\\Telegram Desktop\\";
            GeoJsonFileService.WriteGeometryToFile(BoundingRing.BoundRingsToPolygon(listOfHoles),
                fileName + "step" + m);
            m++;
        }
    }
}

            
        