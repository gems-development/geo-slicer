using System;
using System.Collections.Generic;
using GeoSlicer.Utils;
using GeoSlicer.Utils.BoundRing;
using NetTopologySuite.Geometries;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails.Connectors;

internal static class Connector
{
    internal static void Connect(
        LinkedListNode<BoundingRing> ring1, LinkedListNode<BoundingRing> ring2,
        LinkedNode<Coordinate> ring1Coord, LinkedNode<Coordinate> ring2Coord,
        LinkedList<BoundingRing> listOfHoles,
        Zones? zonesUnion =  null,
        double epsilon = 0, LineService? lineService = null)
    {
        var connectCoordRing1 = ring1Coord;
        var connectCoordRing2 = ring2Coord;

        if (zonesUnion is not null)
        {
            FindFramesSidesCoordinates(
                ring1, ring2, 
                zonesUnion, 
                out var firstSideCoordRing1, out var secondSideCoordRing1,
                out var firstSideCoordRing2, out var secondSideCoordRing2);
            
            if(VectorService.AngleIsZeroOr180Degrees(
                   firstSideCoordRing1, secondSideCoordRing1, ring1Coord.Elem, ring2Coord.Elem, epsilon))
            {
                var coordsInRing1FrameSide =
                    FindCoordinatesInFrameSide(
                        firstSideCoordRing1, secondSideCoordRing1, ring1Coord, lineService!);
                var coordsInRing2FrameSide =
                    FindCoordinatesInFrameSide(
                        firstSideCoordRing2, secondSideCoordRing2, ring2Coord, lineService!);
                
                FindCorrectConnectCoord(coordsInRing1FrameSide, lineService!, ref connectCoordRing1, connectCoordRing2);
                
                FindCorrectConnectCoord(
                    coordsInRing2FrameSide,
                    lineService!,
                    ref connectCoordRing2, connectCoordRing1);
            }
        }

        ring1.Value.ConnectBoundRings(
            ring2.Value,
            connectCoordRing1, connectCoordRing2);
        listOfHoles.Remove(ring2);
    }

    private static List<LinkedNode<Coordinate>> FindCoordinatesInFrameSide(
        Coordinate frameSideCoord1, Coordinate frameSideCoord2, 
        LinkedNode<Coordinate> ringFirstCoord, LineService lineService)
    {
        var currentRingCoord = ringFirstCoord;
        List<LinkedNode<Coordinate>> coordinatesInFrameSide = new List<LinkedNode<Coordinate>>();
        do
        {
            if (lineService.IsCoordinateAtLine(currentRingCoord.Elem, frameSideCoord1, frameSideCoord2))
            {
                coordinatesInFrameSide.Add(currentRingCoord);
            }
            currentRingCoord = currentRingCoord.Next;
            
        } while (!ReferenceEquals(currentRingCoord, ringFirstCoord));

        return coordinatesInFrameSide;
    }

    private static void FindFramesSidesCoordinates(
        LinkedListNode<BoundingRing> ring1, LinkedListNode<BoundingRing> ring2,
        Zones? zonesUnion,
        out Coordinate firstSideCoordRing1, out Coordinate secondSideCoordRing1,
        out Coordinate firstSideCoordRing2, out Coordinate secondSideCoordRing2)
    {
        if (zonesUnion == Zones.Abc)
        {
            firstSideCoordRing1 = ring1.Value.PointMax;
            secondSideCoordRing1 = new Coordinate(ring1.Value.PointMin.X, ring1.Value.PointMax.Y);
            firstSideCoordRing2 = ring2.Value.PointMin;
            secondSideCoordRing2 = new Coordinate(ring2.Value.PointMax.X, ring2.Value.PointMin.Y);
        }
        else if (zonesUnion == Zones.Cde)
        {
            firstSideCoordRing1 = ring1.Value.PointMin;
            secondSideCoordRing1 = new Coordinate(ring1.Value.PointMin.X, ring1.Value.PointMax.Y);
            firstSideCoordRing2 = ring2.Value.PointMax;
            secondSideCoordRing2 = new Coordinate(ring2.Value.PointMax.X, ring2.Value.PointMin.Y);
        }
        else if (zonesUnion == Zones.Efg)
        {
            firstSideCoordRing1 = ring1.Value.PointMin;
            secondSideCoordRing1 = new Coordinate(ring1.Value.PointMax.X, ring1.Value.PointMin.Y);
            firstSideCoordRing2 = ring2.Value.PointMax;
            secondSideCoordRing2 = new Coordinate(ring2.Value.PointMin.X, ring2.Value.PointMax.Y);
        }
        else if (zonesUnion == Zones.Ahg)
        {
            firstSideCoordRing1 = ring1.Value.PointMax;
            secondSideCoordRing1 = new Coordinate(ring1.Value.PointMax.X, ring1.Value.PointMin.Y);
            firstSideCoordRing2 = ring2.Value.PointMin;
            secondSideCoordRing2 = new Coordinate(ring2.Value.PointMin.X, ring2.Value.PointMax.Y);
        }

        else throw new ArgumentException("incorrect zonesUnion");
    }
    
    private static void FindCorrectConnectCoord(
        List<LinkedNode<Coordinate>> coordinatesInFrameSide,
        LineService lineService,
        ref LinkedNode<Coordinate> connectCoordRing1,
        LinkedNode<Coordinate> connectCoordRing2)
    {
        bool findNewConnectCoord = true;
        while (findNewConnectCoord)
        {
            findNewConnectCoord = false;
            foreach (var coordInFrameSide in coordinatesInFrameSide)
            {
                if (!ReferenceEquals(coordInFrameSide, connectCoordRing1) && 
                    lineService.IsCoordinateInSegment(
                        coordInFrameSide.Elem, connectCoordRing1.Elem, connectCoordRing2.Elem))
                {
                    findNewConnectCoord = true;
                    connectCoordRing1 = coordInFrameSide;
                    break;
                }
            }
        }
    }
}