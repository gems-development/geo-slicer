using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace GeoSlicer.GridSlicer;

public class GridSlicer
{
    private readonly LineIntersector _lineIntersector;
    private readonly double _epsilon;
    private readonly LineService _lineService;

    public GridSlicer(LineIntersector lineIntersector, double epsilon, LineService lineService)
    {
        _lineIntersector = lineIntersector;
        _epsilon = epsilon;
        _lineService = lineService;
    }

    private bool IsIntersectHorizontalRayWithSegment(Coordinate point, Coordinate l1, Coordinate l2)
    {
        Coordinate maxCoord;
        Coordinate minCoord;

        if (l1.Y > l2.Y)
        {
            maxCoord = new Coordinate(l1.X, l1.Y);
            minCoord = new Coordinate(l2.X, l2.Y);
        }
        else
        {
            maxCoord = new Coordinate(l2.X, l2.Y);
            minCoord = new Coordinate(l1.X, l1.Y);
        }

        Coordinate vec1 = new Coordinate(minCoord.X - point.X, minCoord.Y - point.Y);
        Coordinate vec2 = new Coordinate(point.X - maxCoord.X, point.Y - maxCoord.Y);

        double product = _lineService.VectorProduct(vec1, vec2);

        return minCoord.Y < point.Y - _epsilon && maxCoord.Y >= point.Y && product < 0;
    }

    public bool IsPointInPolygon(Coordinate point, List<Coordinate> ring)
    {
        // Метод трассировки луча
        int count = 0;
        for (int i = 0; i < ring.Count - 1; i++)
        {
            if (_lineService.IsCoordinateInSegment(point, ring[(i + ring.Count) % ring.Count],
                    ring[(i + 1 + ring.Count) % ring.Count]))
            {
                count = 1;
                break;
            }

            if (IsIntersectHorizontalRayWithSegment(point, ring[(i + ring.Count) % ring.Count],
                    ring[(i + 1 + ring.Count) % ring.Count]))
            {
                count++;
            }
        }

        return count % 2 != 0;
    }

    // todo Переделать на массивы
    private LinkedList<CoordinateSupport> CoordinateToCoordinateSupport(List<Coordinate> list)
    {
        LinkedList<CoordinateSupport> result = new LinkedList<CoordinateSupport>();

        foreach (Coordinate coord in list)
        {
            result.AddLast(new CoordinateSupport(coord));
        }

        return result;
    }

    public List<List<Coordinate>> WeilerAtherton(
        List<Coordinate> clippedCoordinates, List<Coordinate> cuttingCoordinates)
    {
        LinkedList<CoordinateSupport> clipped = CoordinateToCoordinateSupport(clippedCoordinates);
        LinkedList<CoordinateSupport> cutting = CoordinateToCoordinateSupport(cuttingCoordinates);

        // Создание двух списков с помеченными точками
        for (LinkedListNode<CoordinateSupport>? nodeI = clipped.First!; nodeI != null; nodeI = nodeI.Next)
        {
            for (LinkedListNode<CoordinateSupport>? nodeJ = cutting.First!; nodeJ != null; nodeJ = nodeJ.Next)
            {
                (LineIntersectionType, Coordinate?) intersection;
                if (nodeI.Next != null && nodeJ.Next != null)
                {
                    intersection = _lineIntersector.GetIntersection(
                        nodeI.Value, nodeI.Next!.Value, nodeJ.Value, nodeJ.Next!.Value);
                }
                else if (nodeI.Next != null)
                {
                    intersection = _lineIntersector.GetIntersection(
                        nodeI.Value, nodeI.Next!.Value, nodeJ.Value, cutting.First!.Next!.Value);
                }
                else if (nodeJ.Next != null)
                {
                    intersection = _lineIntersector.GetIntersection(
                        nodeI.Value, clipped.First!.Next!.Value, nodeJ.Value, nodeJ.Next!.Value);
                }
                else
                {
                    intersection = _lineIntersector.GetIntersection(
                        nodeI.Value, clipped.First!.Next!.Value, nodeJ.Value, cutting.First!.Next!.Value);
                }

                if (intersection is { Item2: not null, Item1: LineIntersectionType.Inner })
                {
                    LinkedListNode<CoordinateSupport> intersectionNodeInClip =
                        new LinkedListNode<CoordinateSupport>(new CoordinateSupport(intersection.Item2));
                    LinkedListNode<CoordinateSupport> intersectionNodeInCut =
                        new LinkedListNode<CoordinateSupport>(new CoordinateSupport(intersection.Item2));

                    intersectionNodeInClip.Value.Coord = intersectionNodeInCut;
                    intersectionNodeInCut.Value.Coord = intersectionNodeInClip;

                    clipped.AddAfter(nodeI, intersectionNodeInClip);
                    cutting.AddAfter(nodeJ, intersectionNodeInCut);

                    if (IsPointInPolygon(nodeI.Value, cuttingCoordinates))
                    {
                        intersectionNodeInClip.Value.Type = PointType.Living;
                        intersectionNodeInCut.Value.Type = PointType.Living;
                    }
                    else
                    {
                        intersectionNodeInCut.Value.Type = PointType.Entering;
                        intersectionNodeInClip.Value.Type = PointType.Entering;
                    }
                }
            }
        }

        List<List<Coordinate>> result = new();

        for (LinkedListNode<CoordinateSupport>? nodeInClipped = clipped.First;
             nodeInClipped != null;
             nodeInClipped = nodeInClipped.Next)
        {
            if (nodeInClipped.Value.Type == PointType.Entering)
            {
                List<Coordinate> figure = new();

                LinkedListNode<CoordinateSupport>? startInCutting =
                    new LinkedListNode<CoordinateSupport>(nodeInClipped.Value);

                for (LinkedListNode<CoordinateSupport>? nodeFromEToL = nodeInClipped;
                     nodeFromEToL!.Value.Type != PointType.Living;
                     nodeFromEToL = nodeFromEToL.Next)
                {
                    figure.Add(nodeFromEToL.Value);

                    if (nodeFromEToL.Next == null)
                    {
                        nodeFromEToL = clipped.First;
                    }

                    if (nodeFromEToL!.Next!.Value.Type == PointType.Living)
                    {
                        startInCutting = nodeFromEToL.Next.Value.Coord;
                    }
                }

                for (LinkedListNode<CoordinateSupport>? nodeCutting = startInCutting;
                     nodeCutting!.Value.Coord != nodeInClipped;
                     nodeCutting = nodeCutting!.Next)
                {
                    figure.Add(nodeCutting.Value);

                    if (nodeCutting.Next == null)
                    {
                        nodeCutting = cutting.First;
                    }
                }

                figure.Add(nodeInClipped.Value);
                result.Add(figure);
            }
        }

        return result;
    }
}