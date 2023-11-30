using System.Collections.Generic;
using GeoSlicer.Utils.BoundHoleDelDependency;
using NetTopologySuite.Geometries;

namespace GeoSlicer.HoleDeleters.BoundingHoleDeleterDetails;

public class PartitionBoundRingsCache
{
    public LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> ListA { get; set; }
    public LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> ListB { get; set; }
    public LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> ListC{ get; set; }
    public LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> ListD{ get; set; }
    public LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> ListE{ get; set; }
    public LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> ListF{ get; set; }
    public LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> ListG{ get; set; }
    public LinkedList<(LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)> ListH{ get; set; }
    public LinkedList<LinkedListNode<BoundingRing>> IntersectFrames { get; set; }
    public LinkedList<LinkedListNode<BoundingRing>> FramesContainThis { get; set; }
    public (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearABC { get; set; }
    public (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearCDE { get; set; }
    public (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearEFG { get; set; }
    public (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearAHG { get; set; }


    public (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearABCintersect { get; set; }
    public (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearCDEintersect { get; set; }
    public (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearEFGintersect { get; set; }
    public (LinkedListNode<BoundingRing> boundRing, List<PartitioningZones> zones)? NearAHGintersect { get; set; }
    public (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? NearABCSegmentintersect { get; set; }
    public (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? NearCDESegmentintersect { get; set; }
    public (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? NearEFGSegmentintersect { get; set; }
    public (LinkedListNode<BoundingRing> boundRing, LinkedNode<Coordinate> _start)? NearAHGSegmentintersect { get; set; }
    public PartitionBoundRingsCache()
    {
        ListA = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        ListB = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        ListC = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        ListD = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        ListE = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        ListF = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        ListG = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        ListH = new LinkedList<(LinkedListNode<BoundingRing>, List<PartitioningZones>)>();
        IntersectFrames = new LinkedList<LinkedListNode<BoundingRing>>();
        FramesContainThis = new LinkedList<LinkedListNode<BoundingRing>>();
    }
    public void Clear()
    {
        ListA.Clear();
        ListB.Clear();
        ListC.Clear();
        ListD.Clear();
        ListE.Clear();
        ListF.Clear();
        ListG.Clear();
        ListH.Clear();
        IntersectFrames.Clear();
        FramesContainThis.Clear();
        NearABC = null;
        NearCDE = null;
        NearEFG = null;
        NearAHG = null;
        NearABCintersect = null;
        NearCDEintersect = null;
        NearEFGintersect = null;
        NearAHGintersect = null;
        NearABCSegmentintersect = null;
        NearCDESegmentintersect = null;
        NearEFGSegmentintersect = null;
        NearAHGSegmentintersect = null;
    }
}