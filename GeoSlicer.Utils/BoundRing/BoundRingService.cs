using System;
using System.Collections.Generic;
using System.IO;
using NetTopologySuite.Geometries;



namespace GeoSlicer.Utils.BoundRing;

public static class BoundRingService
{
    private static readonly GeometryFactory DefaultGeometryFactory =
        NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
    public static LinkedList<BoundingRing> PolygonToBoundRings(Polygon polygon)
    {
        LinkedList<BoundingRing> boundRings = new LinkedList<BoundingRing>();
        boundRings.AddFirst(LinearRingToBoundingRing(polygon.Shell));
        foreach(LinearRing ring in polygon.Holes)
        {
            boundRings.AddLast(LinearRingToBoundingRing(ring));
        }
        return boundRings;
    }
    public static Polygon BoundRingsToPolygon(LinkedList<BoundingRing> boundRings, GeometryFactory? factory = null)
    {
        if (factory is null)
            factory = DefaultGeometryFactory;
        LinearRing shell = BoundRingToLinearRing(boundRings.First!.Value);
        LinearRing[] holes = new LinearRing[boundRings.Count - 1];
        using(var enumerator = boundRings.GetEnumerator())
        {
            enumerator.MoveNext();
            int i = 0;
            while (enumerator.MoveNext())
            {
                holes[i] = BoundRingToLinearRing(enumerator.Current);
                i++;
            }
        }
        return new Polygon(shell, holes, factory);
    }
    
    private static LinearRing BoundRingToLinearRing(BoundingRing boundRing)
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

    private static BoundingRing LinearRingToBoundingRing(LinearRing ring)
    {
        Coordinate[] coordinates = ring.Coordinates;
        LinkedNode <Coordinate> ringNode = new LinkedNode<Coordinate>(coordinates[0]);
        LinkedNode<Coordinate> pointLeftNode = ringNode;
        LinkedNode<Coordinate> pointRightNode = ringNode;
        LinkedNode<Coordinate> pointUpNode = ringNode;
        LinkedNode<Coordinate> pointDownNode = ringNode;
        for (int i = 1; i < coordinates.Length - 1; i++)
        {
            ringNode = new LinkedNode<Coordinate>(coordinates[i], ringNode);
            pointLeftNode = CoordinateNodeService.MinByX(pointLeftNode, ringNode);
            pointRightNode = CoordinateNodeService.MaxByX(pointRightNode, ringNode);
            pointUpNode = CoordinateNodeService.MaxByY(pointUpNode, ringNode);
            pointDownNode = CoordinateNodeService.MinByY(pointDownNode, ringNode);
        }
        
        Coordinate pointMax = new Coordinate(
            Math.Max(pointUpNode.Elem.X, pointRightNode.Elem.X),
            Math.Max(pointUpNode.Elem.Y, pointRightNode.Elem.Y));
        Coordinate pointMin = new Coordinate(
            Math.Min(pointDownNode.Elem.X, pointLeftNode.Elem.X),
            Math.Min(pointDownNode.Elem.Y, pointLeftNode.Elem.Y));
        
        return new BoundingRing(pointMin, pointMax, pointLeftNode, pointRightNode,
            pointUpNode, pointDownNode, ringNode.Next, coordinates.Length - 1);
    }
    public static BoundingRing ConnectBoundRings(
        BoundingRing boundRing1,
        BoundingRing boundRing2,
        LinkedNode<Coordinate> point1Node, LinkedNode<Coordinate> point2Node)
    {

        boundRing1.Ring = ConnectRingsNodes(point1Node, point2Node);

        boundRing1.PointRightNode = CoordinateNodeService.MaxByX(
            boundRing1.PointRightNode,
            boundRing2.PointRightNode);
        
        boundRing1.PointLeftNode = CoordinateNodeService.MinByX(
            boundRing1.PointLeftNode,
            boundRing2.PointLeftNode);
        
        boundRing1.PointUpNode = CoordinateNodeService.MaxByY(
            boundRing1.PointUpNode,
            boundRing2.PointUpNode);
        
        boundRing1.PointDownNode = CoordinateNodeService.MinByY(
            boundRing1.PointDownNode,
            boundRing2.PointDownNode);
        
        boundRing1.PointMax = new Coordinate(
            boundRing1.PointRightNode.Elem.X,
            boundRing1.PointUpNode.Elem.Y);
        
        boundRing1.PointMin = new Coordinate(
            boundRing1.PointLeftNode.Elem.X,
            boundRing1.PointDownNode.Elem.Y);
        
        boundRing1.PointsCount = boundRing1.PointsCount + boundRing2.PointsCount + 2;
        
        return boundRing1;
    }
    private static LinkedNode<Coordinate> ConnectRingsNodes(
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
        return ring1Node;
    }

    public static LinkedNode<Coordinate> FindRingNode(LinkedNode<Coordinate> ringNode, Coordinate point)
    {
        LinkedNode<Coordinate> bufferRingNode = ringNode;
        do
        {
            if (bufferRingNode.Elem.Equals(point))
                return bufferRingNode;
            bufferRingNode = bufferRingNode.Next;
        } while (!ReferenceEquals(bufferRingNode, ringNode));

        throw new InvalidDataException("point not found in ringNode");
    }


}