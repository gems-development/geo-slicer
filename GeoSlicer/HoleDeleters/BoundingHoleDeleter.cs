using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using GeoSlicer.Utils.BoundHoleDelDependency;

namespace GeoSlicer.HoleDeleters;

public class BoundingHoleDeleter
{
    private LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> _listA;
    private LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> _listB;
    private LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> _listC;
    private LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> _listD;
    private LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> _listE;
    private LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> _listF;
    private LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> _listG;
    private LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> _listH;
    private LinkedList<LinkedListNode<BoundingRing>> _intersectFrames;
    private LinkedList<LinkedListNode<BoundingRing>> _framesContainThis;
    private (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? _nearABC;
    private (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? _nearCDE;
    private (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? _nearEFG;
    private (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? _nearAHG;


    private (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? _nearABCintersect;
    private (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? _nearCDEintersect;
    private (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? _nearEFGintersect;
    private (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? _nearAHGintersect;
    private (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? _nearABCSegmentintersect;
    private (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? _nearCDESegmentintersect;
    private (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? _nearEFGSegmentintersect;
    private (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? _nearAHGSegmentintersect;


    public BoundingHoleDeleter()
    {
        _listA = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        _listB = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        _listC = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        _listD = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        _listE = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        _listF = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        _listG = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        _listH = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        _intersectFrames = new LinkedList<LinkedListNode<BoundingRing>>();
        _framesContainThis = new LinkedList<LinkedListNode<BoundingRing>>();

    }

    private void Clear()
    {
        _listA.Clear();
        _listB.Clear();
        _listC.Clear();
        _listD.Clear();
        _listE.Clear();
        _listF.Clear();
        _listG.Clear();
        _listH.Clear();
        _intersectFrames.Clear();
        _framesContainThis.Clear();
        _nearABC = null;
        _nearCDE = null;
        _nearEFG = null;
        _nearAHG = null;
        _nearABCintersect = null;
        _nearCDEintersect = null;
        _nearEFGintersect = null;
        _nearAHGintersect = null;
        _nearABCSegmentintersect = null;
        _nearCDESegmentintersect = null;
        _nearEFGSegmentintersect = null;
        _nearAHGSegmentintersect = null;
    }

    public static /*LinearRing*/ Polygon DeleteHoles(Polygon polygon)
    {
        LinkedList<BoundingRing> list = BoundRingService.PolygonToBoundRings(polygon);
        new BoundingHoleDeleter().DeleteHoles(list);
        //return BoundRingService.BoundRingsToPolygon(list).Shell;
        return BoundRingService.BoundRingsToPolygon(list);
    }
    private void DeleteHoles(LinkedList<BoundingRing> listOfHoles)
    {
        var thisRing = listOfHoles.First;
        var pointMinShell = thisRing!.Value.PointMin;
        var pointMaxShell = thisRing.Value.PointMax;

        int i = 0;
        int count = listOfHoles.Count;
        while (listOfHoles.First!.Next is not null)
        {
            i++;
            if (count != listOfHoles.Count)
            {
                count = listOfHoles.Count;
                i = 0;
            }
            else if (i == count) return;
            if (_framesContainThis.Any())
            {
                LinkedList<BoundingRing> list = new LinkedList<BoundingRing>();
                foreach (var ring in _framesContainThis)
                {
                    list.AddLast(ring.Value);
                }
                
            }
            
            if (thisRing.Next is null)
                thisRing = listOfHoles.First.Next;
            else thisRing = thisRing.Next;
            Clear();
            bool hasIntersectFrames = FillListsRelativeRing(thisRing, listOfHoles);
            if (!hasIntersectFrames)
            {
                bool frameOfThisChanged = false;
                if (_intersectFrames.Any()){}
                    //frameOfThisChanged = ConnectContainsRingsInThis(thisRing);
                if (frameOfThisChanged)
                {
                    if (thisRing.Value.PointMin.Equals(pointMinShell) && thisRing.Value.PointMax.Equals(pointMaxShell))
                    {
                        var buff = thisRing.Value;
                        listOfHoles.Remove(thisRing);
                        listOfHoles.AddFirst(buff);
                        thisRing = listOfHoles.First;
                    }
                }
                else
                {
                    
                    Coordinate oldPointMin = thisRing.Value.PointMin;
                    Coordinate oldPointMax = thisRing.Value.PointMax;
                    bool connectABC = false;
                    bool connectCDE = false;
                    bool connectEFG = false;
                    bool connectAHG = false;
                    connectABC = ConnectABC(thisRing, listOfHoles);
                    if (oldPointMin.Equals(thisRing.Value.PointMin))
                        connectCDE = ConnectCDE(thisRing, listOfHoles);
                    if (oldPointMin.Equals(thisRing.Value.PointMin))
                        connectEFG = ConnectEFG(thisRing, listOfHoles);
                    if (oldPointMax.Equals(thisRing.Value.PointMax))
                        connectAHG = ConnectAHG(thisRing, listOfHoles);
                    if (!(connectABC || connectCDE || connectEFG || connectAHG)){}
                        //BruteforceConnect(thisRing);
                    if (thisRing.Value.PointMin.Equals(pointMinShell) && thisRing.Value.PointMax.Equals(pointMaxShell))
                    {
                        var buff = thisRing.Value;
                        listOfHoles.Remove(thisRing);
                        listOfHoles.AddFirst(buff);
                        thisRing = listOfHoles.First;
                    }
                }
            }
            else
            {
                //BruteforceConnectIntersectionFrames(thisRing);
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

    private void BruteforceConnectIntersectionFrames(LinkedListNode<BoundingRing> thisRing)
    {
        throw new AggregateException();
    }

    private void BruteforceConnect(LinkedListNode<BoundingRing> thisRing)
    {
        throw new AggregateException();
    }

    private bool ConnectABC(LinkedListNode<BoundingRing> thisRing, LinkedList<BoundingRing> listOfHoles)
    {
        if (_nearABC is null)
            return false;
        bool flagA = false;
        bool flagB = false;
        bool flagC = false;
        Coordinate a = thisRing.Value.PointUpNode.Elem;
        Coordinate b = _nearABC!.Value.boundRing.Value.PointDownNode.Elem;
        foreach (var zone in _nearABC!.Value.zones)
        {
            if (!flagA && zone == PartitioningZones.A)
                flagA = true;
            else if (!flagB && zone == PartitioningZones.B)
                flagB = true;
            else flagC = true;
        }

        if (flagC)
        {
            foreach (var frame in _listC)
            {
                if (!frame.zones.Contains(PartitioningZones.B)
                    && frame.boundRing.Value.PointMin.Y < thisRing.Value.PointMax.Y
                    && hasIntersectsFrame(frame.boundRing.Value, a, b) &&
                    !ReferenceEquals(frame.boundRing.Value, _nearABC.Value.boundRing.Value))
                {
                    _nearABCintersect = frame;
                    return false;
                }
            }
        }

        if (flagA)
        {
            foreach (var frame in _listA)
            {
                if (!frame.zones.Contains(PartitioningZones.B)
                    && frame.boundRing.Value.PointMin.Y < thisRing.Value.PointMax.Y
                    && hasIntersectsFrame(frame.boundRing.Value, a, b) &&
                    !ReferenceEquals(frame.boundRing.Value, _nearABC.Value.boundRing.Value))
                {
                    _nearABCintersect = frame;
                    return false;
                }
            }

        }

        //возможно улучшение
        foreach (var frameWhoContainThis in _framesContainThis)
        {
            var start = frameWhoContainThis.Value.PointUpNode;
            var buffer = start;
            do
            {
                if (
                    buffer.Elem.Y > thisRing.Value.PointMax.Y
                    || Math.Abs(buffer.Elem.Y - thisRing.Value.PointMax.Y) < 1e-9
                    || buffer.Next.Elem.Y > thisRing.Value.PointMax.Y
                    || Math.Abs(buffer.Next.Elem.Y - thisRing.Value.PointMax.Y) < 1e-9)
                {
                    if (hasIntersectedSegments(a, b,buffer.Elem, buffer.Next.Elem))
                    {
                        _nearABCSegmentintersect = (frameWhoContainThis, buffer);
                        return false;
                        
                    }
                }

                buffer = buffer.Next;
            } while (!ReferenceEquals(buffer, start));
        }

        thisRing.Value = BoundRingService.ConnectBoundRings(thisRing.Value,
            _nearABC.Value.boundRing.Value,
            thisRing.Value.PointUpNode,
            _nearABC.Value.boundRing.Value.PointDownNode);
        listOfHoles.Remove(_nearABC.Value.boundRing);
        return true;
    }

    private bool hasIntersectsFragment((
        LinkedListNode<BoundingRing> boundRing,
        LinkedNode<Coordinate> start,
        LinkedNode<Coordinate> end
        ) fragment, Coordinate a, Coordinate b)
    {
        LinkedNode<Coordinate> start = fragment.start;
        LinkedNode<Coordinate> end = fragment.end;
        while (ReferenceEquals(start.Previous, end))
        {
            if (hasIntersectedSegments(start.Elem, start.Next.Elem, a, b))
                return true;
            start = start.Next;
        }

        return false;
    }

    //возможно улучшение
    private bool hasIntersectsFrame(BoundingRing ring, Coordinate a, Coordinate b)
    {
        LineSegment AB = new LineSegment(a, b);
        LineSegment[] sides = new LineSegment [4];
        sides[0] = new LineSegment(ring.PointMin, new Coordinate(ring.PointMin.X, ring.PointMax.Y));
        sides[1] = new LineSegment(new Coordinate(ring.PointMin.X, ring.PointMax.Y), ring.PointMax);
        sides[2] = new LineSegment(ring.PointMax, new Coordinate(ring.PointMax.X, ring.PointMin.Y));
        sides[3] = new LineSegment(new Coordinate(ring.PointMax.X, ring.PointMin.Y), ring.PointMin);
        foreach (var side in sides)
        {
            if (side.Intersection(AB) is not null)
                return true;
        }

        return false;
    }

    //нужно улучшить
    private bool hasIntersectedSegments(Coordinate a1, Coordinate b1, Coordinate a2, Coordinate b2)
    {
        return new LineSegment(a1, b1).Intersection(new LineSegment(a2, b2)) is not null;
    }

    private bool ConnectCDE(LinkedListNode<BoundingRing> thisRing, LinkedList<BoundingRing> listOfHoles)
    {
        if (_nearCDE is null)
            return false;
        bool flagC = false;
        bool flagD = false;
        bool flagE = false;

        Coordinate a = thisRing.Value.PointLeftNode.Elem;
        Coordinate b = _nearCDE!.Value.boundRing.Value.PointRightNode.Elem;
        foreach (var zone in _nearCDE!.Value.zones)
        {
            if (!flagC && zone == PartitioningZones.C)
                flagC = true;
            else if (!flagD && zone == PartitioningZones.D)
                flagD = true;
            else flagE = true;
        }

        if (flagC)
        {
            foreach (var frame in _listC)
            {
                if (!frame.zones.Contains(PartitioningZones.D)
                    && frame.boundRing.Value.PointMax.X > thisRing.Value.PointMin.X
                    && hasIntersectsFrame(frame.boundRing.Value, a, b) &&
                    !ReferenceEquals(frame.boundRing.Value, _nearCDE.Value.boundRing.Value))
                {
                    _nearCDEintersect = frame;
                    return false;
                }
            }

        }

        if (flagE)
        {
            foreach (var frame in _listE)
            {
                if (!frame.zones.Contains(PartitioningZones.D)
                    && frame.boundRing.Value.PointMax.X > thisRing.Value.PointMin.X
                    && hasIntersectsFrame(frame.boundRing.Value, a, b) &&
                    !ReferenceEquals(frame.boundRing.Value, _nearCDE.Value.boundRing.Value))
                {
                    _nearCDEintersect = frame;
                    return false;
                }
            }

        }

        //возможно улучшение
        foreach (var frameWhoContainThis in _framesContainThis)
        {
            var start = frameWhoContainThis.Value.PointLeftNode;
            var buffer = start;
            do
            {
                if (
                    buffer.Elem.X < thisRing.Value.PointMin.X
                    || Math.Abs(buffer.Elem.X - thisRing.Value.PointMin.X) < 1e-9
                    || buffer.Next.Elem.X < thisRing.Value.PointMin.X
                    || Math.Abs(buffer.Next.Elem.X - thisRing.Value.PointMin.X) < 1e-9)
                {
                    if (hasIntersectedSegments(a, b,buffer.Elem, buffer.Next.Elem))
                    {
                        _nearCDESegmentintersect = (frameWhoContainThis, buffer);
                        return false;
                    }
                }

                buffer = buffer.Next;
            } while (!ReferenceEquals(buffer, start));
        }

        thisRing.Value = BoundRingService.ConnectBoundRings(thisRing.Value,
            _nearCDE.Value.boundRing.Value,
            thisRing.Value.PointLeftNode,
            _nearCDE.Value.boundRing.Value.PointRightNode);
        listOfHoles.Remove(_nearCDE.Value.boundRing);
        return true;
    }

    private bool ConnectEFG(LinkedListNode<BoundingRing> thisRing, LinkedList<BoundingRing> listOfHoles)
    {
        if (_nearEFG is null)
            return false;
        bool flagE = false;
        bool flagF = false;
        bool flagG = false;
        Coordinate a = thisRing.Value.PointDownNode.Elem;
        Coordinate b = _nearEFG!.Value.boundRing.Value.PointUpNode.Elem;
        foreach (var zone in _nearEFG!.Value.zones)
        {
            if (!flagE && zone == PartitioningZones.E)
                flagE = true;
            else if (!flagF && zone == PartitioningZones.F)
                flagF = true;
            else flagG = true;
        }

        if (flagE)
        {
            foreach (var frame in _listE)
            {
                if (!frame.zones.Contains(PartitioningZones.F)
                    && frame.boundRing.Value.PointMax.Y > thisRing.Value.PointMin.Y
                    && hasIntersectsFrame(frame.boundRing.Value, a, b) &&
                    !ReferenceEquals(frame.boundRing.Value, _nearEFG.Value.boundRing.Value))
                {
                    _nearEFGintersect = frame;
                    return false;
                }
            }
        }

        if (flagG)
        {
            foreach (var frame in _listG)
            {
                if (!frame.zones.Contains(PartitioningZones.F)
                    && frame.boundRing.Value.PointMax.Y > thisRing.Value.PointMin.Y
                    && hasIntersectsFrame(frame.boundRing.Value, a, b) &&
                    !ReferenceEquals(frame.boundRing.Value, _nearEFG.Value.boundRing.Value))
                {
                    _nearEFGintersect = frame;
                    return false;
                }
            }
        }

        //возможно улучшение
        foreach (var frameWhoContainThis in _framesContainThis)
        {
            var start = frameWhoContainThis.Value.PointDownNode;
            var buffer = start;
            do
            {
                if (
                    buffer.Elem.Y < thisRing.Value.PointMin.Y
                    || Math.Abs(buffer.Elem.Y - thisRing.Value.PointMin.Y) < 1e-9
                    || buffer.Next.Elem.Y < thisRing.Value.PointMin.Y
                    || Math.Abs(buffer.Next.Elem.Y - thisRing.Value.PointMin.Y) < 1e-9)
                {
                    if (hasIntersectedSegments(a, b,buffer.Elem, buffer.Next.Elem))
                    {
                        _nearEFGSegmentintersect = (frameWhoContainThis, buffer);
                        return false;
                    }
                }

                buffer = buffer.Next;
            } while (!ReferenceEquals(buffer, start));
        }

        thisRing.Value = BoundRingService.ConnectBoundRings(thisRing.Value,
            _nearEFG.Value.boundRing.Value,
            thisRing.Value.PointDownNode,
            _nearEFG.Value.boundRing.Value.PointUpNode);
        listOfHoles.Remove(_nearEFG.Value.boundRing);
        return true;
    }

    private bool ConnectAHG(LinkedListNode<BoundingRing> thisRing, LinkedList<BoundingRing> listOfHoles)
    {
        if (_nearAHG is null)
            return false;
        bool flagA = false;
        bool flagH = false;
        bool flagG = false;
        int i = 0;
        GeoJsonFileService.GeoJsonFileService.WriteGeometryToFile(BoundRingService.BoundRingsToPolygon(new LinkedList<BoundingRing>(new []{_nearAHG.Value.boundRing.Value})), "C:\\Users\\Данил\\Downloads\\holes\\hole" + i + ".geojson");
        Coordinate a = thisRing.Value.PointRightNode.Elem;
        Coordinate b = _nearAHG!.Value.boundRing.Value.PointLeftNode.Elem;
        foreach (var zone in _nearAHG!.Value.zones)
        {
            if (!flagA && zone == PartitioningZones.A)
                flagA = true;
            else if (!flagH && zone == PartitioningZones.H)
                flagH = true;
            else flagG = true;
        }

        if (flagA)
        {
            foreach (var frame in _listA)
            {
                if (!frame.zones.Contains(PartitioningZones.H)
                    && frame.boundRing.Value.PointMin.X < thisRing.Value.PointMax.X
                    && hasIntersectsFrame(frame.boundRing.Value, a, b) &&
                    !ReferenceEquals(frame.boundRing.Value, _nearAHG.Value.boundRing.Value))
                {
                    _nearAHGintersect = frame;
                    return false;
                }
            }
        }

        if (flagG)
        {
            foreach (var frame in _listG)
            {
                if (!frame.zones.Contains(PartitioningZones.H)
                    && frame.boundRing.Value.PointMin.X < thisRing.Value.PointMax.X
                    && hasIntersectsFrame(frame.boundRing.Value, a, b) &&
                    !ReferenceEquals(frame.boundRing.Value, _nearAHG.Value.boundRing.Value))
                {
                    _nearAHGintersect = frame;
                    return false;
                }
            }
        }

        //возможно улучшение
        foreach (var frameWhoContainThis in _framesContainThis)
        {
            var start = frameWhoContainThis.Value.PointRightNode;
            var buffer = start;
            do
            {
                if (
                    buffer.Elem.X > thisRing.Value.PointMax.X
                    || Math.Abs(buffer.Elem.X - thisRing.Value.PointMax.X) < 1e-9
                    || buffer.Next.Elem.X > thisRing.Value.PointMax.X
                    || Math.Abs(buffer.Next.Elem.X - thisRing.Value.PointMax.X) < 1e-9)
                {
                    if (hasIntersectedSegments(a, b,buffer.Elem, buffer.Next.Elem))
                    {
                        _nearAHGSegmentintersect = (frameWhoContainThis, buffer);
                        return false;
                    }
                }

                buffer = buffer.Next;
            } while (!ReferenceEquals(buffer, start));
        }

        thisRing.Value = BoundRingService.ConnectBoundRings(thisRing.Value,
            _nearAHG.Value.boundRing.Value,
            thisRing.Value.PointRightNode,
            _nearAHG.Value.boundRing.Value.PointLeftNode);
        listOfHoles.Remove(_nearAHG.Value.boundRing);
        return true;
    }

    private bool ConnectContainsRingsInThis(LinkedListNode<BoundingRing> thisRing)
    {
        throw new AggregateException();
    }


    private IList<PartitioningZones> detectZonesTwoCoordinates(Coordinate a, Coordinate b,
        BoundingRing thisRing)
    {
        ISet<PartitioningZones> set = new HashSet<PartitioningZones>();
        if (a.Y > thisRing.PointMax.Y || Math.Abs(a.Y - thisRing.PointMax.Y) < 1e-9)
        {
            if (a.X < thisRing.PointMin.X) set.Add(PartitioningZones.C);
            else if (a.X > thisRing.PointMax.X) set.Add(PartitioningZones.A);
            else set.Add(PartitioningZones.B);
        }
        else if (a.X > thisRing.PointMin.X || Math.Abs(a.X - thisRing.PointMin.X) < 1e-9)
        {
            if (a.Y > thisRing.PointMax.Y) set.Add(PartitioningZones.C);
            else if (a.Y < thisRing.PointMin.Y) set.Add(PartitioningZones.E);
            else set.Add(PartitioningZones.D);
        }
        else if (a.Y < thisRing.PointMin.Y || Math.Abs(a.Y - thisRing.PointMin.Y) < 1e-9)
        {
            if (a.X < thisRing.PointMin.X) set.Add(PartitioningZones.E);
            else if (a.X > thisRing.PointMax.X) set.Add(PartitioningZones.G);
            else set.Add(PartitioningZones.F);
        }
        else
        {
            if (a.Y > thisRing.PointMax.Y) set.Add(PartitioningZones.A);
            else if (a.Y < thisRing.PointMin.Y) set.Add(PartitioningZones.G);
            else set.Add(PartitioningZones.H);
        }

        if (b.Y > thisRing.PointMax.Y || Math.Abs(b.Y - thisRing.PointMax.Y) < 1e-9)
        {
            if (b.X < thisRing.PointMin.X) set.Add(PartitioningZones.C);
            else if (b.X > thisRing.PointMax.X) set.Add(PartitioningZones.A);
            else set.Add(PartitioningZones.B);
        }
        else if (b.X > thisRing.PointMin.X || Math.Abs(b.X - thisRing.PointMin.X) < 1e-9)
        {
            if (b.Y > thisRing.PointMax.Y) set.Add(PartitioningZones.C);
            else if (b.Y < thisRing.PointMin.Y) set.Add(PartitioningZones.E);
            else set.Add(PartitioningZones.D);
        }
        else if (b.Y < thisRing.PointMin.Y || Math.Abs(b.Y - thisRing.PointMin.Y) < 1e-9)
        {
            if (b.X < thisRing.PointMin.X) set.Add(PartitioningZones.E);
            else if (b.X > thisRing.PointMax.X) set.Add(PartitioningZones.G);
            else set.Add(PartitioningZones.F);
        }
        else
        {
            if (b.Y > thisRing.PointMax.Y) set.Add(PartitioningZones.A);
            else if (b.Y < thisRing.PointMin.Y) set.Add(PartitioningZones.G);
            else set.Add(PartitioningZones.H);
        }

        return set.ToList();
    }

    private bool FillListsRelativeRing(LinkedListNode<BoundingRing> boundRing,
        LinkedList<BoundingRing> boundRings)
    {
        Boolean hasIntersectFrames = false;
        LinkedListNode<BoundingRing>? thisRing = boundRings.First;
        while (thisRing is not null)
        {
            if (!ReferenceEquals(thisRing, boundRing))
            {
                if (!hasIntersectFrames)
                {
                    if (!DetectPartitingZone(boundRing, thisRing))
                    {
                        hasIntersectFrames = IntersectOrContainFramesCheck(boundRing, thisRing);
                    }
                }
                else if (NotIntersectCheck(boundRing.Value, thisRing.Value))
                    IntersectOrContainFramesCheck(boundRing, thisRing);
                else
                {
                    _intersectFrames.AddFirst(thisRing);
                }
            }

            thisRing = thisRing.Next;
        }

        return hasIntersectFrames;
    }

    //возращает false если одна рамка содержится в другой
    //true в противном случае(могут пересекаться и не пересекаться)
    private bool IntersectOrContainFramesCheck(
        LinkedListNode<BoundingRing> relativeBoundRing,
        LinkedListNode<BoundingRing> thisBoundRing)
    {
        Coordinate pointMin = new Coordinate(
            Math.Min(relativeBoundRing.Value.PointMin.X, thisBoundRing.Value.PointMin.X),
            Math.Min(relativeBoundRing.Value.PointMin.Y, thisBoundRing.Value.PointMin.Y));
        Coordinate pointMax = new Coordinate(
            Math.Max(relativeBoundRing.Value.PointMax.X, thisBoundRing.Value.PointMax.X),
            Math.Max(relativeBoundRing.Value.PointMax.Y, thisBoundRing.Value.PointMax.Y));
        if (pointMin.Equals(relativeBoundRing.Value.PointMin) &&
            pointMax.Equals(relativeBoundRing.Value.PointMax))
        {
            _intersectFrames.AddFirst(thisBoundRing);
            return false;
        }

        if (pointMin.Equals(thisBoundRing.Value.PointMin) && pointMax.Equals(thisBoundRing.Value.PointMax))
        {
            _framesContainThis.AddFirst(thisBoundRing);
            return false;
        }

        _intersectFrames.AddFirst(thisBoundRing);
        return true;
    }

    
    //возращает false если рамки пересекаются(не важно как)
    private bool DetectPartitingZone(LinkedListNode<BoundingRing> relativeBoundRing,
        LinkedListNode<BoundingRing> boundingRing)
    {
        List<PartitioningZones> list = new List<PartitioningZones>(3);
        bool flagA = false;
        bool flagC = false;
        bool flagE = false;
        bool flagG = false;
        bool flagABC = false;
        bool flagCDE = false;
        bool flagEFG = false;
        bool flagAHG = false;
        if (boundingRing.Value.PointMin.Y > relativeBoundRing.Value.PointMax.Y
            || Math.Abs(boundingRing.Value.PointMin.Y - relativeBoundRing.Value.PointMax.Y) < 1e-9)
        {
            if (boundingRing.Value.PointMin.X < relativeBoundRing.Value.PointMin.X)
            {
                list.Add(PartitioningZones.C);
                _listC.AddFirst((boundingRing, list));
                flagC = true;
            }

            if (boundingRing.Value.PointMax.X > relativeBoundRing.Value.PointMax.X)
            {
                list.Add(PartitioningZones.A);
                _listA.AddFirst((boundingRing, list));
                flagA = true;
            }

            if (flagA == flagC
                || SegmentContainAtLeastOneNumber
                (relativeBoundRing.Value.PointMin.X,
                    relativeBoundRing.Value.PointMax.X,
                    new[] { boundingRing.Value.PointMin.X, boundingRing.Value.PointMax.X }))
            {
                list.Add(PartitioningZones.B);
                _listB.AddFirst((boundingRing, list));
            }

            flagABC = true;

        }

        else if (boundingRing.Value.PointMax.X < relativeBoundRing.Value.PointMin.X
            || Math.Abs(boundingRing.Value.PointMax.X - relativeBoundRing.Value.PointMin.X) < 1e-9)
        {
            if (boundingRing.Value.PointMax.Y > relativeBoundRing.Value.PointMax.Y)
            {
                list.Add(PartitioningZones.C);
                _listC.AddFirst((boundingRing, list));
                flagC = true;
            }

            if (boundingRing.Value.PointMin.Y < relativeBoundRing.Value.PointMin.Y)
            {
                list.Add(PartitioningZones.E);
                _listE.AddFirst((boundingRing, list));
                flagE = true;
            }

            if (flagE == flagC
                || SegmentContainAtLeastOneNumber
                (relativeBoundRing.Value.PointMin.Y,
                    relativeBoundRing.Value.PointMax.Y,
                    new[] { boundingRing.Value.PointMin.Y, boundingRing.Value.PointMax.Y }))
            {
                list.Add(PartitioningZones.D);
                _listD.AddFirst((boundingRing, list));
            }

            flagCDE = true;

        }

        else if (boundingRing.Value.PointMax.Y < relativeBoundRing.Value.PointMin.Y
            || Math.Abs(boundingRing.Value.PointMax.Y - relativeBoundRing.Value.PointMin.Y) < 1e-9)
        {
            if (boundingRing.Value.PointMax.X > relativeBoundRing.Value.PointMax.Y)
            {
                list.Add(PartitioningZones.G);
                _listG.AddFirst((boundingRing, list));
                flagG = true;
            }

            if (boundingRing.Value.PointMin.X < relativeBoundRing.Value.PointMin.X)
            {
                list.Add(PartitioningZones.E);
                _listE.AddFirst((boundingRing, list));
                flagE = true;
            }

            if (flagE == flagG
                || SegmentContainAtLeastOneNumber
                (relativeBoundRing.Value.PointMin.X,
                    relativeBoundRing.Value.PointMax.X,
                    new[] { boundingRing.Value.PointMin.X, boundingRing.Value.PointMax.X }))
            {
                list.Add(PartitioningZones.F);
                _listF.AddFirst((boundingRing, list));
            }

            flagEFG = true;
        }

        else if (boundingRing.Value.PointMin.X > relativeBoundRing.Value.PointMax.X
            || Math.Abs(boundingRing.Value.PointMin.X - relativeBoundRing.Value.PointMax.X) < 1e-9)
        {
            if (boundingRing.Value.PointMin.Y < relativeBoundRing.Value.PointMin.Y)
            {
                list.Add(PartitioningZones.G);
                _listG.AddFirst((boundingRing, list));
                flagG = true;
            }

            if (boundingRing.Value.PointMax.Y > relativeBoundRing.Value.PointMax.Y)
            {
                list.Add(PartitioningZones.A);
                _listA.AddFirst((boundingRing, list));
                flagA = true;
            }

            if (flagA == flagG
                || SegmentContainAtLeastOneNumber
                (relativeBoundRing.Value.PointMin.Y,
                    relativeBoundRing.Value.PointMax.Y,
                    new[] { boundingRing.Value.PointMin.Y, boundingRing.Value.PointMax.Y }))
            {
                list.Add(PartitioningZones.H);
                _listH.AddFirst((boundingRing, list));
            }

            flagAHG = true;
        }
        else return false;
        
        if ((_nearABC is null ||
             boundingRing.Value.PointMin.Y < _nearABC.Value.boundRing.Value.PointMin.Y) &&
            (flagABC || (list.Count == 1 && (flagA || flagC))))
        {
            _nearABC = (boundingRing, list);
        }

        if ((_nearCDE is null ||
             boundingRing.Value.PointMax.X > _nearCDE.Value.boundRing.Value.PointMax.X) &&
            (flagCDE || (list.Count == 1 && (flagC || flagE))))
        {
            _nearCDE = (boundingRing, list);
        }

        if ((_nearEFG is null ||
             boundingRing.Value.PointMax.Y > _nearEFG.Value.boundRing.Value.PointMax.Y) &&
            (flagEFG || (list.Count == 1 && (flagE || flagG))))
        {
            _nearEFG = (boundingRing, list);
        }

        if ((_nearAHG is null ||
             boundingRing.Value.PointMin.X < _nearAHG.Value.boundRing.Value.PointMin.X) &&
            (flagAHG || (list.Count == 1 && (flagA || flagG))))
        {
            _nearAHG = (boundingRing, list);
        }
        
        return true;
    }

    private bool NotIntersectCheck(BoundingRing relativeBoundRing, BoundingRing boundingRing)
    {
        return boundingRing.PointMin.Y > relativeBoundRing.PointMax.Y
               || Math.Abs(boundingRing.PointMin.Y - relativeBoundRing.PointMax.Y) < 1e-9 ||


               boundingRing.PointMax.X < relativeBoundRing.PointMin.X
               || Math.Abs(boundingRing.PointMax.X - relativeBoundRing.PointMin.X) < 1e-9 ||

               boundingRing.PointMax.Y < relativeBoundRing.PointMin.Y
               || Math.Abs(boundingRing.PointMax.Y - relativeBoundRing.PointMin.Y) < 1e-9 ||


               boundingRing.PointMin.X > relativeBoundRing.PointMax.X
               || Math.Abs(boundingRing.PointMin.X - relativeBoundRing.PointMax.X) < 1e-9;
    }

    private bool SegmentContainAtLeastOneNumber(double a, double b, double[] arr)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            if ((arr[i] > a && arr[i] < b) || Math.Abs(a - arr[i]) < 1e-9 || Math.Abs(b - arr[i]) < 1e-9)
                return true;
        }

        return false;
    }
}

            
        