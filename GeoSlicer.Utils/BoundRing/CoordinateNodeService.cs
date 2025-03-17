using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.BoundRing;

public static class CoordinateNodeService
{
    public static LinkedNode<Coordinate> GetMinByX(LinkedNode<Coordinate> point1Node, LinkedNode<Coordinate> point2Node)
    {
        return point1Node.Elem.X < point2Node.Elem.X ? point1Node : point2Node;
    }

    public static LinkedNode<Coordinate> GetMaxByX(LinkedNode<Coordinate> point1Node, LinkedNode<Coordinate> point2Node)
    {
        return point1Node.Elem.X < point2Node.Elem.X ? point2Node : point1Node;
    }

    public static LinkedNode<Coordinate> GetMinByY(LinkedNode<Coordinate> point1Node, LinkedNode<Coordinate> point2Node)
    {
        return point1Node.Elem.Y < point2Node.Elem.Y ? point1Node : point2Node;
    }

    public static LinkedNode<Coordinate> GetMaxByY(LinkedNode<Coordinate> point1Node, LinkedNode<Coordinate> point2Node)
    {
        return point1Node.Elem.Y < point2Node.Elem.Y ? point2Node : point1Node;
    }
}