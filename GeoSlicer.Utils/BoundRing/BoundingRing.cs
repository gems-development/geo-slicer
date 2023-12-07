using System;
using System.Collections.Generic;
using System.IO;
using NetTopologySuite.Geometries;
namespace GeoSlicer.Utils.BoundRing;

public class BoundingRing
{
    
    public Coordinate PointMin { get; private set; }
    public Coordinate PointMax { get; private set; }

    public LinkedNode<Coordinate> PointLeftNode { get; private set; }
    public LinkedNode<Coordinate> PointRightNode { get; private set; }
    public LinkedNode<Coordinate> PointUpNode { get; private set; }
    public LinkedNode<Coordinate> PointDownNode { get; private set; }

    public LinkedNode<Coordinate> Ring { get; private set; }
    public int PointsCount { get; private set; }
    public bool counterClockwiseBypass;

    public BoundingRing(Coordinate pointMin,
        Coordinate pointMax,
        LinkedNode<Coordinate> pointLeftNode,
        LinkedNode<Coordinate> pointRightNode,
        LinkedNode<Coordinate> pointUpNode,
        LinkedNode<Coordinate> pointDownNode,
        LinkedNode<Coordinate> ring, int pointsCount, bool counterClockwiseBypass)
    {
        PointMin = pointMin;
        PointMax = pointMax;
        PointLeftNode = pointLeftNode;
        PointRightNode = pointRightNode;
        PointUpNode = pointUpNode;
        PointDownNode = pointDownNode;
        Ring = ring;
        PointsCount = pointsCount;
        this.counterClockwiseBypass = counterClockwiseBypass;
    }

    protected bool Equals(BoundingRing other)
    {
        return PointMin.Equals(other.PointMin)
               && PointMax.Equals(other.PointMax)
               && PointLeftNode.Equals(other.PointLeftNode)
               && PointRightNode.Equals(other.PointRightNode)
               && PointUpNode.Equals(other.PointUpNode)
               && PointDownNode.Equals(other.PointDownNode)
               && Ring.Equals(other.Ring) && PointsCount == other.PointsCount;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((BoundingRing)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PointMin, PointMax, PointLeftNode, PointRightNode,
            PointUpNode, PointDownNode, Ring, PointsCount);
    }
    
    private static readonly GeometryFactory DefaultGeometryFactory =
        NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
    public static LinkedList<BoundingRing> PolygonToBoundRings(Polygon polygon)
    {
        LinkedList<BoundingRing> boundRings = new LinkedList<BoundingRing>();
        boundRings.AddFirst(LinearRingToBoundingRing(polygon.Shell, true));
        foreach(LinearRing ring in polygon.Holes)
        {
            boundRings.AddLast(LinearRingToBoundingRing(ring, false));
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

    private static BoundingRing LinearRingToBoundingRing(LinearRing ring, bool counterClockwiseBypass)
    {
        Coordinate[] coordinates = ring.Coordinates;
        LinkedNode <Coordinate> ringNode = new LinkedNode<Coordinate>(coordinates[0]);
        LinkedNode<Coordinate> pointLeftNode = ringNode;
        LinkedNode<Coordinate> pointRightNode = ringNode;
        LinkedNode<Coordinate> pointUpNode = ringNode;
        LinkedNode<Coordinate> pointDownNode = ringNode;
        bool clockwise = TraverseDirection.IsClockwiseBypass(ring);
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
    public void ConnectBoundRings(
        BoundingRing boundRing2,
        LinkedNode<Coordinate> point1Node, LinkedNode<Coordinate> point2Node)
    {
        point1Node = FindCorrectLinkedCoord(point1Node, point2Node.Elem, this.counterClockwiseBypass);
        point2Node = FindCorrectLinkedCoord(point2Node, point1Node.Elem, boundRing2.counterClockwiseBypass);
        Ring = ConnectRingsNodes(point1Node, point2Node);

        PointRightNode = CoordinateNodeService.MaxByX(
            PointRightNode,
            boundRing2.PointRightNode);
        
        PointLeftNode = CoordinateNodeService.MinByX(
            PointLeftNode,
            boundRing2.PointLeftNode);
        
        PointUpNode = CoordinateNodeService.MaxByY(
            PointUpNode,
            boundRing2.PointUpNode);
        
        PointDownNode = CoordinateNodeService.MinByY(
            PointDownNode,
            boundRing2.PointDownNode);
        
        PointMax = new Coordinate(
            PointRightNode.Elem.X,
            PointUpNode.Elem.Y);
        
        PointMin = new Coordinate(
            PointLeftNode.Elem.X,
            PointDownNode.Elem.Y);
        
        PointsCount = PointsCount + boundRing2.PointsCount + 2;
    }

    private LinkedNode<Coordinate> FindCorrectLinkedCoord(
        LinkedNode<Coordinate> point1Node,
        Coordinate point2,
        bool thisCounterClockwiseBypass)
    {
        if (point1Node.Next2 is null)
            return point1Node;
        if (thisCounterClockwiseBypass)
        {
            while (ReferenceEquals(point1Node.Previous2!.Elem, point1Node.Elem))
            {
                point1Node = point1Node.Previous2;
            }
            while (true)
            {
                if (!ReferenceEquals(point1Node.Next2!.Elem, point1Node.Elem))
                {
                    return point1Node;
                }

                Coordinate b1 = point1Node.Previous.Elem;
                Coordinate b2 = point1Node.Elem;
                Coordinate b3 = point1Node.Next.Elem;
                if (SegmentService.InsideTheAngle(point1Node.Elem, point2, b1, b2, b3))
                {
                    return point1Node;
                }

                point1Node = point1Node.Next2;
            }
        }
        while (ReferenceEquals(point1Node.Previous2!.Elem, point1Node.Elem))
        {
            point1Node = point1Node.Previous2;
        }
        while (true)
        {
            if (!ReferenceEquals(point1Node.Next2!.Elem, point1Node.Elem))
            {
                return point1Node;
            }

            Coordinate b1 = point1Node.Next.Elem;
            Coordinate b2 = point1Node.Elem;
            Coordinate b3 = point1Node.Previous.Elem;
            if (SegmentService.InsideTheAngle(point1Node.Elem, point2, b1, b2, b3))
            {
                return point1Node;
            }

            point1Node = point1Node.Next2;
        }
    }
    private LinkedNode<Coordinate> ConnectRingsNodes(
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
        
        ring1Node.Previous2 = ring1Node.Previous;
        ring1Node.Next2 = ring1NodeNext.Previous;
        ring1NodeNext.Previous.Previous2 = ring1Node;
        ring1NodeNext.Previous.Next2 = ring1NodeNext.Previous.Next;

        ring2Node.Next2 = ring2Node.Next;
        ring2Node.Previous2 = ring2NodePrevious.Next;
        ring2NodePrevious.Next.Next2 = ring2Node;
        ring2NodePrevious.Next.Previous2 = ring2NodePrevious.Next.Previous;
        
        return ring1Node;
    }
    
}