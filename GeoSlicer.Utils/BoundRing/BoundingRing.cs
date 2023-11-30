using System;
using NetTopologySuite.Geometries;
namespace GeoSlicer.Utils.BoundRing;

public class BoundingRing
{
    
    public Coordinate PointMin { get; set; }
    public Coordinate PointMax { get; set; }

    public LinkedNode<Coordinate> PointLeftNode { get; set; }
    public LinkedNode<Coordinate> PointRightNode { get; set; }
    public LinkedNode<Coordinate> PointUpNode { get; set; }
    public LinkedNode<Coordinate> PointDownNode { get; set; }

    public LinkedNode<Coordinate> Ring { get; set; }
    public int PointsCount { get; set; }

    public BoundingRing(Coordinate pointMin,
        Coordinate pointMax,
        LinkedNode<Coordinate> pointLeftNode,
        LinkedNode<Coordinate> pointRightNode,
        LinkedNode<Coordinate> pointUpNode,
        LinkedNode<Coordinate> pointDownNode,
        LinkedNode<Coordinate> ring, int pointsCount)
    {
        PointMin = pointMin;
        PointMax = pointMax;
        PointLeftNode = pointLeftNode;
        PointRightNode = pointRightNode;
        PointUpNode = pointUpNode;
        PointDownNode = pointDownNode;
        Ring = ring;
        PointsCount = pointsCount;
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
}