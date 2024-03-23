using System;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.BoundRing;

internal class BoundRService
{
    internal static LinearRing BoundRingToLinearRing(BoundingRing boundRing)
    {
        var points = new Coordinate[boundRing.PointsCount + 1];
        var ringNode = boundRing.Ring;
        var ringNodeBuff = ringNode;
        int i = 0;
        do
        {
            points[i] = ringNodeBuff.Elem;
            ringNodeBuff = ringNodeBuff.Next;
            i++;
        } while (!ReferenceEquals(ringNodeBuff, ringNode));

        points[i] = ringNode.Elem;

        return new LinearRing(points);
    }

    internal static BoundingRing
        LinearRingToBoundingRing(LinearRing ring, bool counterClockwiseBypass, TraverseDirection direction)
    {
        Coordinate[] coordinates = ring.Coordinates;
        LinkedNode <Coordinate> ringNode = new LinkedNode<Coordinate>(coordinates[0]);
        LinkedNode<Coordinate> pointLeftNode = ringNode;
        LinkedNode<Coordinate> pointRightNode = ringNode;
        LinkedNode<Coordinate> pointUpNode = ringNode;
        LinkedNode<Coordinate> pointDownNode = ringNode;
        bool clockwise = direction.IsClockwiseBypass(ring);
        bool counterClockwiseBypassBuff = counterClockwiseBypass;
        
        if (!counterClockwiseBypass) 
        {
            clockwise = !clockwise;
        }
        
        if (clockwise)
        {
            for (int i = 1; i < coordinates.Length - 1; i++)
            {
                ringNode = new LinkedNode<Coordinate>(coordinates[i], ringNode);
                pointLeftNode = CoordinateNodeService.MinByX(pointLeftNode, ringNode);
                pointRightNode = CoordinateNodeService.MaxByX(pointRightNode, ringNode);
                pointUpNode = CoordinateNodeService.MaxByY(pointUpNode, ringNode);
                pointDownNode = CoordinateNodeService.MinByY(pointDownNode, ringNode);
            }
        }
        else
        {
            for (int i = coordinates.Length - 2; i >= 1; i--)
            {
                ringNode = new LinkedNode<Coordinate>(coordinates[i], ringNode);
                pointLeftNode = CoordinateNodeService.MinByX(pointLeftNode, ringNode);
                pointRightNode = CoordinateNodeService.MaxByX(pointRightNode, ringNode);
                pointUpNode = CoordinateNodeService.MaxByY(pointUpNode, ringNode);
                pointDownNode = CoordinateNodeService.MinByY(pointDownNode, ringNode);
            }
        }
        
        Coordinate pointMax = new Coordinate(
            Math.Max(pointUpNode.Elem.X, pointRightNode.Elem.X),
            Math.Max(pointUpNode.Elem.Y, pointRightNode.Elem.Y));
        
        Coordinate pointMin = new Coordinate(
            Math.Min(pointDownNode.Elem.X, pointLeftNode.Elem.X),
            Math.Min(pointDownNode.Elem.Y, pointLeftNode.Elem.Y));
        
        return new BoundingRing(pointMin, pointMax, pointLeftNode, pointRightNode,
            pointUpNode, pointDownNode, ringNode.Next, coordinates.Length - 1, counterClockwiseBypassBuff);
    }

    //Ищет точку, которая совпадает с point1Node, но соединение с ней не будет пересекать другие нулевые туннели.
    internal static LinkedNode<Coordinate> FindCorrectLinkedCoord(
        LinkedNode<Coordinate> point1Node, Coordinate point2, bool thisCounterClockwiseBypass)
    {
        if (point1Node.AdditionalNext is null)
            return point1Node;
        
        while (ReferenceEquals(point1Node.AdditionalPrevious!.Elem, point1Node.Elem))
        {
            point1Node = point1Node.AdditionalPrevious;
        }
        while (true)
        {
            if (!ReferenceEquals(point1Node.AdditionalNext!.Elem, point1Node.Elem))
            {
                return point1Node;
            }

            Coordinate b1 = point1Node.Next.Elem;
            Coordinate b2 = point1Node.Elem;
            Coordinate b3 = point1Node.Previous.Elem;

            bool res;
            if (thisCounterClockwiseBypass)
            {
                res = SegmentService.InsideTheAngle(
                    point1Node.Elem, point2, b3, b2, b1);
            }
            else
            {
                res = SegmentService.InsideTheAngle(
                    point1Node.Elem, point2, b1, b2, b3);
            }
            
            if (res)
            {
                return point1Node;
            }

            point1Node = point1Node.AdditionalNext;
        }
    }

    internal static LinkedNode<Coordinate> ConnectRingsNodes(
        LinkedNode<Coordinate> ring1Node,
        LinkedNode<Coordinate> ring2Node)
    {
        LinkedNode<Coordinate> ring1NodeNext = ring1Node.Next;
        LinkedNode<Coordinate> ring2NodePrevious = ring2Node.Previous;
        
        ring1Node.Next = ring2Node;
        ring2Node.Previous = ring1Node;

        ring2NodePrevious.Next = ring1NodeNext;
        ring1NodeNext.Previous = ring2NodePrevious;
        
        new LinkedNode<Coordinate>(
            ring1Node.Elem,
            new LinkedNode<Coordinate>(ring2Node.Elem, ring2NodePrevious, ring1NodeNext),
            ring1NodeNext);
        
        //меняем ссылки у первого кольца
        if (ring1Node.AdditionalPrevious == null)
            ring1Node.AdditionalPrevious = ring1Node.Previous;
        var oldRing1NodeAdditionalNext = ring1Node.AdditionalNext;
        ring1Node.AdditionalNext = ring1NodeNext.Previous;
        
        ring1NodeNext.Previous.AdditionalPrevious = ring1Node;
        if (oldRing1NodeAdditionalNext == null)
        {
            ring1NodeNext.Previous.AdditionalNext = ring1NodeNext.Previous.Next;
        }
        else
        {
            ring1NodeNext.Previous.AdditionalNext = oldRing1NodeAdditionalNext;
            if (ReferenceEquals(oldRing1NodeAdditionalNext.Elem, ring1Node.Elem)) 
                oldRing1NodeAdditionalNext.AdditionalPrevious = ring1NodeNext.Previous;
        }
        
        //меняем ссылки у второго кольца
        if(ring2Node.AdditionalNext == null)
            ring2Node.AdditionalNext = ring2Node.Next;
        var oldRing2NodeAdditionalPrevious = ring2Node.AdditionalPrevious;
        ring2Node.AdditionalPrevious = ring2NodePrevious.Next;
        
        ring2NodePrevious.Next.AdditionalNext = ring2Node;
        if (oldRing2NodeAdditionalPrevious == null)
        {
            ring2NodePrevious.Next.AdditionalPrevious = ring2NodePrevious.Next.Previous;
        }
        else
        {
            ring2NodePrevious.Next.AdditionalPrevious = oldRing2NodeAdditionalPrevious;
            if (ReferenceEquals(oldRing2NodeAdditionalPrevious.Elem, ring2Node.Elem))
                oldRing2NodeAdditionalPrevious.AdditionalNext = ring2NodePrevious.Next;
        }

        return ring1Node;
    }
}