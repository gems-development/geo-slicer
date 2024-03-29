using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GeoSlicer.GridSlicer;

public class GridSlicerHelper
{
    private readonly LinesIntersector _linesIntersector;
    private readonly double _epsilon;
    private readonly LineService _lineService;
    private readonly ICoordinateComparator _coordinateComparator;
    private readonly TraverseDirection _traverseDirection;

    public GridSlicerHelper(LinesIntersector linesIntersector, double epsilon, LineService lineService,
        ICoordinateComparator coordinateComparator)
    {
        _linesIntersector = linesIntersector;
        _epsilon = epsilon;
        _lineService = lineService;
        _coordinateComparator = coordinateComparator;
        _traverseDirection = new TraverseDirection(_lineService);
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

    public bool IsPointInPolygon(Coordinate point, LinearRing ring)
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
    private LinkedList<CoordinateSupport> CoordinateToCoordinateSupport(LinearRing ring)
    {
        LinkedList<CoordinateSupport> result = new LinkedList<CoordinateSupport>();

        Coordinate[] coordinates = ring.Coordinates;

        foreach (Coordinate coord in coordinates)
        {
            result.AddLast(new CoordinateSupport(coord));
        }

        result.RemoveLast();

        return result;
        //Вернёт LinkedList с координатами всех точек кольца без последней (равной первой)
    }

    public IntersectionType WeilerAtherton(
        LinearRing clipped, double xDown, double xUp, double yDown, double yUp, out IEnumerable<LinearRing> result)
    {
        Coordinate[] boxCoordinated =
        {
            new(xDown, yDown),
            new(xDown, yUp),
            new(xUp, yUp),
            new(xUp, yDown),
            new(xDown, yDown)
        };
        LinearRing boxLinearRing = new LinearRing(boxCoordinated);
        
        result = Array.Empty<LinearRing>();

        
        if (boxCoordinated.All(coordinate => IsPointInPolygon(coordinate, clipped)) 
            && !boxLinearRing.Intersects(clipped))
        {
            return IntersectionType.BoxInGeometry;
        }

        if (clipped.Coordinates.All(coordinate => IsPointInPolygon(coordinate, boxLinearRing)))
        {
            return IntersectionType.GeometryInBox;
        }

        
        var intersection = new Polygon(clipped).Intersection(new Polygon(boxLinearRing));

        if (intersection is null || intersection.IsEmpty)
        {
            return IntersectionType.BoxOutsideGeometry;
        }

        if (intersection is Polygon polygon)
        {
            result = new[] { polygon.Shell };
        }

        if (intersection is MultiPolygon multiPolygon)
        {
            result = multiPolygon.Select(geometry => ((Polygon)geometry).Shell);
        }

        return IntersectionType.IntersectionWithEdge;
    }

    //На вход передаются координаты колец
    public IEnumerable<LinearRing> WeilerAtherton(
        LinearRing clippedCoordinates, LinearRing cuttingCoordinates)
    {
        //нужно, чтобы обход clipped и cutting был по часовой

        if (!_traverseDirection.IsClockwiseBypass(clippedCoordinates))
        {
            _traverseDirection.ChangeDirection(clippedCoordinates);
        }

        if (!_traverseDirection.IsClockwiseBypass(cuttingCoordinates))
        {
            _traverseDirection.ChangeDirection(cuttingCoordinates);
        }

        LinkedList<CoordinateSupport> clipped = CoordinateToCoordinateSupport(clippedCoordinates);
        LinkedList<CoordinateSupport> cutting = CoordinateToCoordinateSupport(cuttingCoordinates);

        bool flagWereIntersection = false;
        // Создание двух списков с помеченными точками
        for (LinkedListNode<CoordinateSupport>? nodeI = clipped.First!; nodeI != null; nodeI = nodeI.Next)
        {
            for (LinkedListNode<CoordinateSupport>? nodeJ = cutting.First!; nodeJ != null; nodeJ = nodeJ.Next)
            {
                LinkedListNode<CoordinateSupport> numberOne = nodeI;
                LinkedListNode<CoordinateSupport>? numberTwo = nodeI.Next;
                LinkedListNode<CoordinateSupport> numberThree = nodeJ;
                LinkedListNode<CoordinateSupport>? numberFour = nodeJ.Next;

                (LinesIntersectionType, Coordinate?) intersection;
                if (nodeI.Next != null && nodeJ.Next != null)
                {
                    intersection = _linesIntersector.GetIntersection(
                        nodeI.Value, nodeI.Next!.Value, nodeJ.Value, nodeJ.Next!.Value);
                }
                else if (nodeI.Next != null)
                {
                    intersection = _linesIntersector.GetIntersection(
                        nodeI.Value, nodeI.Next!.Value, nodeJ.Value, cutting.First!.Value);

                    numberFour = cutting.First;
                }
                else if (nodeJ.Next != null)
                {
                    intersection = _linesIntersector.GetIntersection(
                        nodeI.Value, clipped.First!.Value, nodeJ.Value, nodeJ.Next!.Value);

                    numberTwo = clipped.First;
                }
                else
                {
                    intersection = _linesIntersector.GetIntersection(
                        nodeI.Value, clipped.First!.Value, nodeJ.Value, cutting.First!.Value);

                    numberTwo = clipped.First;
                    numberFour = cutting.First;
                }
/*
                //Overlay

                if (intersection is { Item1: LinesIntersectionType.Overlay })
                {
                    flagWereIntersection = true;

                    LinkedListNode<CoordinateSupport>? intersectionNodeInClip = null;
                    LinkedListNode<CoordinateSupport>? intersectionNodeInCut = null;

                    //первый-четвёртый случаи
                    if(_lineService.IsCoordinateInSegmentBorders(numberThree.Value, numberOne.Value, numberTwo!.Value) &&
                        _lineService.IsCoordinateInSegmentBorders(numberTwo.Value, numberThree.Value, numberFour!.Value))
                    {
                        intersectionNodeInClip = new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberThree.Value));
                        intersectionNodeInCut = new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberTwo.Value));

                        intersectionNodeInClip.Value.Coord = numberThree;
                        numberThree.Value.Coord = intersectionNodeInClip;

                        intersectionNodeInCut.Value.Coord = numberTwo;
                        numberTwo.Value.Coord = intersectionNodeInCut;

                        intersectionNodeInClip.Value.Type = PointType.Entering;
                        numberTwo.Value.Type = PointType.Living;

                    }
                    else if (_lineService.IsCoordinateInSegmentBorders(numberFour!.Value, numberOne.Value, numberTwo.Value) &&
                                _lineService.IsCoordinateInSegmentBorders(numberTwo.Value, numberThree.Value, numberFour.Value))
                    {
                        intersectionNodeInClip = new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberFour.Value));
                        intersectionNodeInCut = new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberTwo.Value));

                        intersectionNodeInClip.Value.Coord = numberFour;
                        numberFour.Value.Coord = intersectionNodeInClip;

                        intersectionNodeInCut.Value.Coord = numberTwo;
                        numberTwo.Value.Coord = intersectionNodeInCut;

                        intersectionNodeInClip.Value.Type = PointType.Entering;
                        numberTwo.Value.Type = PointType.Living;
                    }
                    else if (_lineService.IsCoordinateInSegmentBorders(numberThree.Value, numberOne.Value, numberTwo.Value) &&
                                _lineService.IsCoordinateInSegmentBorders(numberOne.Value, numberThree.Value, numberFour.Value))
                    {
                        intersectionNodeInClip = new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberThree.Value));
                        intersectionNodeInCut = new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberOne.Value));

                        intersectionNodeInClip.Value.Coord = numberThree;
                        numberThree.Value.Coord = intersectionNodeInClip;

                        intersectionNodeInCut.Value.Coord = numberOne;
                        numberOne.Value.Coord = intersectionNodeInCut;

                        numberOne.Value.Type = PointType.Entering;
                        intersectionNodeInClip.Value.Type = PointType.Living;
                    }
                    else if (_lineService.IsCoordinateInSegmentBorders(numberFour.Value, numberOne.Value, numberTwo.Value) &&
                             _lineService.IsCoordinateInSegmentBorders(numberOne.Value, numberThree.Value, numberFour.Value))
                    {
                        intersectionNodeInClip = new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberFour.Value));
                        intersectionNodeInCut = new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberOne.Value));

                        intersectionNodeInClip.Value.Coord = numberFour;
                        numberFour.Value.Coord = intersectionNodeInClip;

                        intersectionNodeInCut.Value.Coord = numberOne;
                        numberOne.Value.Coord = intersectionNodeInCut;

                        numberOne.Value.Type = PointType.Entering;
                        intersectionNodeInClip.Value.Type = PointType.Living;
                    }

                    //добавление двух новых нод в листы
                    clipped.AddAfter(nodeI, intersectionNodeInClip!);
                    cutting.AddAfter(nodeJ, intersectionNodeInCut!);
                }

                //Part

                else if (intersection is { Item1: LinesIntersectionType.Part })
                {
                    flagWereIntersection = true;

                    LinkedListNode<CoordinateSupport>? intersectionNodeInClip = null;
                    LinkedListNode<CoordinateSupport>? intersectionNodeInCut = null;

                    bool flagForCopyPast = false;

                    if (_coordinateComparator.IsEquals(numberTwo!.Value, numberThree.Value) &&
                       _lineService.IsCoordinateInSegmentBorders(numberOne.Value, numberThree.Value, numberFour!.Value))
                    {
                        intersectionNodeInClip = new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberOne.Value));

                        intersectionNodeInClip.Value.Coord = numberOne;
                        numberOne.Value.Coord = intersectionNodeInClip;

                        numberTwo.Value.Coord = numberThree;
                        numberThree.Value.Coord = numberTwo;
                    }
                    else if (_coordinateComparator.IsEquals(numberTwo!.Value, numberThree.Value) &&
                             _lineService.IsCoordinateInSegmentBorders(numberFour!.Value, numberOne.Value, numberTwo.Value))
                    {

                    }
                }
*/
                //Inner

                if (intersection is { Item2: not null, Item1: LinesIntersectionType.Inner })
                {
                    flagWereIntersection = true;

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
                    }
                    else
                    {
                        intersectionNodeInClip.Value.Type = PointType.Entering;
                    }
                }

                else if (intersection is { Item2: not null, Item1: LinesIntersectionType.Corner })
                {
                }
            }
        }

        if (!flagWereIntersection)
        {
            if (IsPointInPolygon(clipped.First!.Value, cuttingCoordinates))
            {
                return new List<LinearRing>() { new(clipped.Select(support => (Coordinate)support).ToArray()) };
            }

            return new List<LinearRing>() { new(cutting.Select(support => (Coordinate)support).ToArray()) };
        }

        Print(clipped, cutting);

        //обход списков, формирование пересечений многоугольников

        List<IEnumerable<Coordinate>> result = new();

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

                        if (nodeFromEToL!.Value.Type == PointType.Living)
                        {
                            startInCutting = nodeFromEToL.Value.Coord;
                            break;
                        }

                        figure.Add(nodeFromEToL.Value);
                    }

                    if (nodeFromEToL.Next!.Value.Type == PointType.Living)
                    {
                        startInCutting = nodeFromEToL.Next.Value.Coord;
                    }
                }

                for (LinkedListNode<CoordinateSupport>? nodeCutting = startInCutting;
                     nodeCutting!.Value.Coord != nodeInClipped;
                     nodeCutting = nodeCutting.Next)
                {
                    figure.Add(nodeCutting.Value);

                    if (nodeCutting.Next == null)
                    {
                        nodeCutting = cutting.First;
                        figure.Add(nodeCutting!.Value);

                        if (nodeCutting.Value.Coord == nodeInClipped)
                        {
                            break;
                        }
                    }
                }

                figure.Add(nodeInClipped.Value);
                result.Add(figure);
            }
        }

        return result.Select(enumerable => new LinearRing(enumerable.ToArray()));
    }

    void Print(LinkedList<CoordinateSupport> clipped, LinkedList<CoordinateSupport> cutting)
    {
        try
        {
            //Pass the filepath and filename to the StreamWriter Constructor
            StreamWriter sw =
                new StreamWriter(
                    "C:\\Users\\micha\\Desktop\\Миша\\work\\C#\\Geo\\geo-slicer\\GeoSlicer\\GridSlicer\\Bad.txt");
            //Write a line of text
            sw.WriteLine("clipped\n");
            for (LinkedListNode<CoordinateSupport>? i = clipped.First; i != null; i = i.Next)
            {
                sw.Write(i.Value + " " + i.Value.Type);
                if (i.Value.Coord is not null)
                {
                    sw.Write(" ссылка на " + i.Value.Coord.Value + " ");
                }
                else
                {
                    sw.WriteLine();
                }

                if (i.Value.Coord is { Next: not null })
                {
                    sw.WriteLine("Value.Coord.Next = " + i.Value.Coord.Next.Value);
                }
            }

            sw.WriteLine("\n\n\ncutting\n");
            for (LinkedListNode<CoordinateSupport>? i = cutting.First; i != null; i = i.Next)
            {
                sw.Write(i.Value + " " + i.Value.Type);
                if (i.Value.Coord is not null)
                {
                    sw.Write(" ссылка на " + i.Value.Coord.Value + " ");
                }
                else
                {
                    sw.WriteLine();
                }

                if (i.Value.Coord is { Next: not null })
                {
                    sw.WriteLine("Value.Coord.Next = " + i.Value.Coord.Next.Value);
                }
            }

            //Close the file
            sw.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }
}