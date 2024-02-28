using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GeoSlicer.Utils.SegmentService;

namespace GeoSlicer.GridSlicer;

public class GridSlicer
{
    private readonly GeometryFactory _gf;
    private SegmentService _segmentService;
    private LineIntersector _lineIntersector;
    private EpsilonCoordinateComparator _epsComparator;

    public GridSlicer()
    {
        _gf = NtsGeometryServices.Instance.CreateGeometryFactory(4326);
        _segmentService = new SegmentService();
        _epsComparator = new EpsilonCoordinateComparator();
        _lineIntersector = new LineIntersector(_epsComparator);
    }

    public bool IsPointInRectangle(Coordinate point, List<Coordinate> rectangle)
    {
        for(int i = 0; i < rectangle.Count; i++)
        {
            if (rectangle[(i + rectangle.Count) % rectangle.Count].X < point.X && rectangle[(i + rectangle.Count) % rectangle.Count].Y < point.Y &&
                rectangle[(i + 1 + rectangle.Count) % rectangle.Count].X < point.X && rectangle[(i + 1 + rectangle.Count) % rectangle.Count].Y > point.Y &&
                rectangle[(i + 2 + rectangle.Count) % rectangle.Count].X > point.X && rectangle[(i + 2 + rectangle.Count) % rectangle.Count].Y > point.Y &&
                rectangle[(i + 3 + rectangle.Count) % rectangle.Count].X > point.X && rectangle[(i + 3 + rectangle.Count) % rectangle.Count].Y < point.Y)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsPointOnSegment(Coordinate point, Coordinate l1, Coordinate l2, double epsilon = 1e-9)
    {
        Coordinate vec1 = new Coordinate(l2.X - point.X, l2.Y - point.Y);
        Coordinate vec2 = new Coordinate(point.X - l1.X, point.Y - l1.Y);

        if (_segmentService.VectorProduct(vec1, vec2) == 0 && 
            point.X <= Math.Max(l1.X, l2.X) &&
            point.X >= Math.Min(l1.X, l2.X) &&
            point.Y <= Math.Max(l1.Y, l2.Y) &&
            point.Y >= Math.Min(l1.Y, l2.Y)) return true;
        return false;
    }

    public bool IsIntersectHoryzontalRayWithSegment(Coordinate point, Coordinate l1, Coordinate l2, double epsilon = 1e-9)
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

        double product = _segmentService.VectorProduct(vec1, vec2);

        if ((minCoord.Y < point.Y - epsilon && maxCoord.Y >= point.Y) && (product < 0)) return true;
        return false;
    }

    public bool IsPointInPolygon(Coordinate point, List<Coordinate> ring)
    {
        //метод трассировки луча
        int count = 0;
        for(int i = 0; i < ring.Count - 1; i++)
        {
            if(IsPointOnSegment(point, ring[(i + ring.Count) % ring.Count], ring[(i + 1 + ring.Count) % ring.Count])) 
            { 
                count = 1;
                break;
            }
            else if(IsIntersectHoryzontalRayWithSegment(point, ring[(i + ring.Count) % ring.Count], ring[(i + 1 + ring.Count) % ring.Count]))
            {
                count++;
            }
        }

        if (count % 2 == 0) return false;
        return true;
    }

    public LinkedList<CoordinateSupport> CoordinateToCoordinateSupport(List<Coordinate> list)
    {
        LinkedList<CoordinateSupport> result = new LinkedList<CoordinateSupport> ();

        foreach (Coordinate coord in list)
        {
            result.AddLast(new CoordinateSupport(coord));
        }
        return result;
    }
 
    public List<List<Coordinate>> WeilerAtherton(List<Coordinate> _clipped, List<Coordinate> _cutting, double epsilon = 1e-9)
    {
        //проверяем, находятся ли вершины каждого многоугольника внутри другого   ///это нужно будет для определения entering и living точек
        LinkedList<CoordinateSupport> clipped = CoordinateToCoordinateSupport(_clipped);
        LinkedList<CoordinateSupport> cutting = CoordinateToCoordinateSupport(_cutting);
        
        //создание двух списков с помеченными точками
        for (LinkedListNode<CoordinateSupport>? node_i = clipped.First!; node_i != null; node_i = node_i.Next)
        {
            for (LinkedListNode<CoordinateSupport>? node_j = cutting.First!; node_j != null; node_j = node_j.Next) {
                (IntersectionType, Coordinate?) intersection;
                if (node_i.Next != null && node_j.Next != null)
                {
                    intersection = _lineIntersector.GetIntersection(node_i!.Value, node_i.Next!.Value, node_j!.Value, node_j.Next!.Value);
                }
                else if (node_i.Next != null)
                {
                    intersection = _lineIntersector.GetIntersection(node_i!.Value, node_i.Next!.Value, node_j!.Value, cutting.First!.Value);
                }
                else if (node_j.Next != null)
                {
                    intersection = _lineIntersector.GetIntersection(node_i!.Value, clipped.First!.Value, node_j!.Value, node_j.Next!.Value);
                }
                else
                {
                    intersection = _lineIntersector.GetIntersection(node_i!.Value, clipped.First!.Value, node_j!.Value, cutting.First!.Value);

                }
                if (intersection.Item2 != null && intersection.Item1 is IntersectionType.Inner)
                {
                    CoordinateSupport pointIntersection = new CoordinateSupport(intersection.Item2);
                    LinkedListNode<CoordinateSupport> intersectionNodeInClip = new LinkedListNode<CoordinateSupport>(pointIntersection);
                    LinkedListNode<CoordinateSupport> intersectionNodeInCut = new LinkedListNode<CoordinateSupport>(pointIntersection);
                    LinkedListNode<CoordinateSupport> clippedNode = new LinkedListNode<CoordinateSupport>(node_i.Value);
                    LinkedListNode<CoordinateSupport> cuttingNode = new LinkedListNode<CoordinateSupport>(node_j.Value);

                    clipped.AddAfter(clippedNode, intersectionNodeInClip);
                    cutting.AddAfter(cuttingNode, intersectionNodeInCut);

                    if (IsPointInPolygon(node_i.Value, _cutting))
                    {
                        intersectionNodeInClip.Value.Type = PointType.Living;
                        intersectionNodeInClip.Value.Coord = intersectionNodeInCut;
                    }
                    else
                    {
                        intersectionNodeInCut.Value.Type = PointType.Entering;
                        intersectionNodeInCut.Value.Coord = intersectionNodeInClip;
                    }
                }
            }
        }

        List<List<Coordinate>> result = new();
        
        for(LinkedListNode<CoordinateSupport>? node_in_clipped = clipped.First; node_in_clipped != null; node_in_clipped = node_in_clipped.Next)
        {
            if (node_in_clipped.Value.Type == PointType.Entering)
            {
                List<Coordinate> figure = new();

                LinkedListNode<CoordinateSupport>? start_in_cutting = new LinkedListNode<CoordinateSupport>(cutting.First!.Value);

                for (LinkedListNode<CoordinateSupport>? node_from_e_to_l = node_in_clipped; node_from_e_to_l!.Value.Type != PointType.Living; node_from_e_to_l = node_from_e_to_l!.Next)
                {
                    figure.Add(node_from_e_to_l.Value);

                    if (node_from_e_to_l.Next == null)
                    {
                        node_from_e_to_l = clipped.First;

                        if(node_from_e_to_l!.Value.Type == PointType.Living)
                        {
                            start_in_cutting = node_from_e_to_l.Value.Coord;
                        }
                    }
                    
                    if (node_from_e_to_l!.Next!.Value.Type == PointType.Living)
                    {
                        start_in_cutting = node_from_e_to_l.Next.Value.Coord;
                    }
                }

                for(LinkedListNode<CoordinateSupport>? node_cutting = start_in_cutting; node_cutting!.Value.Coord!=node_in_clipped; node_cutting = node_cutting.Next)
                {
                    figure.Add(node_cutting.Value);
                }

                result.Add(figure);
            }
        }

        return result;
    }
}
 