using System.Collections.Generic;
using GeoSlicer.Utils.BoundRing;
using NetTopologySuite.Geometries;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails.Connectors;

internal static class Connector
{
    internal static void Connect(
        LinkedListNode<BoundingRing> ring1, LinkedListNode<BoundingRing> ring2,
        LinkedNode<Coordinate> ring1Coord, LinkedNode<Coordinate> ring2Coord,
        LinkedList<BoundingRing> listOfHoles)
    {
        ring1.Value.ConnectBoundRings(
            ring2.Value,
            ring1Coord, ring2Coord);
        listOfHoles.Remove(ring2);
    }
}