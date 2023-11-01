
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.BoundHoleDelDependency;

public static class CoordinateNodeService
{
    public static LinkedNode<Coordinate> MinByX(LinkedNode<Coordinate> point1Node, LinkedNode<Coordinate> point2Node)
    {
        if (point1Node.Elem.X < point2Node.Elem.X)
            return point1Node;
        return point2Node;
    }
    public static LinkedNode<Coordinate> MaxByX(LinkedNode<Coordinate> point1Node, LinkedNode<Coordinate> point2Node)
    {
        if (point1Node.Elem.X < point2Node.Elem.X)
            return point2Node;
        return point1Node;
    }
    public static LinkedNode<Coordinate> MinByY(LinkedNode<Coordinate> point1Node, LinkedNode<Coordinate> point2Node)
    {
        if (point1Node.Elem.Y < point2Node.Elem.Y)
            return point1Node;
        return point2Node;
    }
    public static LinkedNode<Coordinate> MaxByY(LinkedNode<Coordinate> point1Node, LinkedNode<Coordinate> point2Node)
    {
        if (point1Node.Elem.Y < point2Node.Elem.Y)
            return point2Node;
        return point1Node;
    }
}