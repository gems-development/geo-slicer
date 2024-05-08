using System.Collections.Generic;
using GeoSlicer.Utils.BoundRing;
using NetTopologySuite.Geometries;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails.Structures;

internal class RingAndPoint
{
    internal LinkedListNode<LinkedListNode<BoundingRing>> BoundRing;
    internal readonly LinkedNode<Coordinate> Start;

    public RingAndPoint(
        LinkedListNode<LinkedListNode<BoundingRing>> boundRing,
        LinkedNode<Coordinate> start)
    {
        BoundRing = boundRing;
        Start = start;
    }
}