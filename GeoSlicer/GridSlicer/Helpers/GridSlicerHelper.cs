using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.GridSlicer.Helpers;

public class GridSlicerHelper
{
    private readonly LinesIntersector _linesIntersector;
    private readonly double _epsilon;
    private readonly LineService _lineService;
    private readonly SegmentService _segmentService;
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

        _segmentService = new SegmentService(_lineService);
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
        int numberOfEnteringMarks = 0;
        
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
        bool flagWereIntersectionOnCurrentIteration = false;
        
        // Создание двух списков с помеченными точками
        for (LinkedListNode<CoordinateSupport>? nodeI = clipped.First!; nodeI != null; 
             nodeI = flagWereIntersectionOnCurrentIteration ? nodeI : nodeI.Next)
        {
            for (LinkedListNode<CoordinateSupport>? nodeJ = cutting.First!; nodeJ != null; 
                 nodeJ = flagWereIntersectionOnCurrentIteration ? nodeJ : nodeJ.Next)
            {
                flagWereIntersectionOnCurrentIteration = false;
                
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
                
                //Inner

                if (intersection is { Item2: not null, Item1: LinesIntersectionType.Inner })
                {
                    //flagWereIntersection = true;

                    LinkedListNode<CoordinateSupport> intersectionNodeInClip =
                        new LinkedListNode<CoordinateSupport>(new CoordinateSupport(intersection.Item2));
                    LinkedListNode<CoordinateSupport> intersectionNodeInCut =
                        new LinkedListNode<CoordinateSupport>(new CoordinateSupport(intersection.Item2));
/*
                    intersectionNodeInClip.Value.Coord = intersectionNodeInCut;
                    intersectionNodeInCut.Value.Coord = intersectionNodeInClip;
                    
                    double product = _lineService.VectorProduct(
                        numberOne.Value, numberTwo.Value, 
                        numberThree.Value, numberFour.Value);
                    if (product > 0 && 
                    
                        intersectionNodeInClip.Value.Type == PointType.Useless && 
                        intersectionNodeInCut.Value.Type == PointType.Useless)
                    {
                        flagWereIntersection = true;
                        intersectionNodeInClip.Value.Type = PointType.Entering;
                        intersectionNodeInCut.Value.Type = PointType.Entering;
                    }
                    else if (product < 0 && 
                             numberTwo.Value.Type == PointType.Useless && 
                             numberFour.Value.Type == PointType.Useless)
                    {
                        flagWereIntersection = true;
                        intersectionNodeInClip.Value.Type = PointType.Living;
                        intersectionNodeInCut.Value.Type = PointType.Living;
                    }
                    */
                    clipped.AddAfter(nodeI, intersectionNodeInClip);
                    cutting.AddAfter(nodeJ, intersectionNodeInCut);
                    flagWereIntersectionOnCurrentIteration = true;
                }

                //TyShaped
                
                else if (intersection is { Item2: not null, Item1: LinesIntersectionType.TyShaped })
                {
                    //flagWereIntersection = true;

                    LinkedListNode<CoordinateSupport> intersectionNode =
                        new LinkedListNode<CoordinateSupport>(new CoordinateSupport(intersection.Item2));
              /*      
                    double product = _lineService.VectorProduct(
                        numberOne.Value, numberTwo.Value, 
                        numberThree.Value, numberFour.Value);
                    if (product > 0 && 
                        intersectionNode.Value.Type == PointType.Useless)
                    {
                        flagWereIntersection = true;
                        intersectionNode.Value.Type = PointType.Entering;
                        //пометить ещё и другую точку 
                    }
                    else if (product < 0 && 
                             numberTwo.Value.Type == PointType.Useless)
                    {
                        flagWereIntersection = true;
                        intersectionNode.Value.Type = PointType.Living;
                        //пометить ещё и другую точку
                    }
*/
                    if(_coordinateComparator.IsEquals(intersectionNode.Value, numberOne.Value))
                    {
                        //numberOne.Value.Type = intersectionNode.Value.Type;
                        cutting.AddAfter(numberThree, intersectionNode);
                        flagWereIntersectionOnCurrentIteration = true;
                    }
                    else if (_coordinateComparator.IsEquals(intersectionNode.Value, numberTwo!.Value))
                    {
                     //   numberTwo.Value.Type = intersectionNode.Value.Type;
                        cutting.AddAfter(numberThree, intersectionNode);
                        flagWereIntersectionOnCurrentIteration = true;
                    }
                    else if (_coordinateComparator.IsEquals(intersectionNode.Value, numberThree.Value))
                    {
                        //numberThree.Value.Type = intersectionNode.Value.Type;
                        clipped.AddAfter(numberOne, intersectionNode);
                        flagWereIntersectionOnCurrentIteration = true;
                    }
                    else
                    {
                       // numberFour.Value.Type = intersectionNode.Value.Type;
                        clipped.AddAfter(numberOne, intersectionNode);
                        flagWereIntersectionOnCurrentIteration = true;
                    }
                }
                
                //Corner
                
                else if (intersection is { Item2: not null, Item1: LinesIntersectionType.Corner } or {Item2: not null, Item1: LinesIntersectionType.Extension})
                {
                    //flagWereIntersection = true;
                    
                    //определили точки для углов в Corner
                    LinkedListNode<CoordinateSupport> prevOne = clipped.Last!;
                    LinkedListNode<CoordinateSupport> prevThree = cutting.Last!;
                    LinkedListNode<CoordinateSupport> nextTwo = clipped.First!;
                    LinkedListNode<CoordinateSupport> nextFour = cutting.First!;

                    if (numberOne.Previous != null)
                    {
                        prevOne = numberOne.Previous;
                    }

                    if (numberThree.Previous != null)
                    {
                        prevThree = numberThree.Previous;
                    }

                    if (numberTwo!.Next != null)
                    {
                        nextTwo = numberTwo.Next;
                    }

                    if (numberFour!.Next != null)
                    {
                        nextFour = numberFour.Next;
                    }
                    
                    if (_coordinateComparator.IsEquals(numberOne.Value, numberFour.Value))
                    {
                        numberOne.Value.Coord = numberFour;
                        numberFour.Value.Coord = numberOne;
                    }

                    else if (_coordinateComparator.IsEquals(numberTwo.Value, numberFour.Value))
                    {
                        if (!(VectorService.InsideTheAngle(numberFour.Value,numberThree.Value,
                                nextTwo!.Value,numberTwo.Value,numberOne.Value) &&
                              VectorService.InsideTheAngle(numberFour.Value,nextFour!.Value,
                                nextTwo.Value,numberTwo.Value,numberOne.Value)))
                        {
                            double product = _lineService.VectorProduct(
                                numberOne.Value, numberTwo.Value, 
                                numberThree.Value, numberFour.Value);
                            /*if (product > 0 && 
                                numberTwo.Value.Type == PointType.Useless && 
                                numberFour.Value.Type == PointType.Useless)
                            {
                                flagWereIntersection = true;
                                numberOfEnteringMarks++;
                                numberTwo.Value.Type = PointType.Entering;
                                numberFour.Value.Type = PointType.Entering;
                            }
                            else */if (product < 0 && 
                                     numberTwo.Value.Type == PointType.Useless && 
                                     numberFour.Value.Type == PointType.Useless)
                            {
                                flagWereIntersection = true;
                                numberTwo.Value.Type = PointType.Living;
                                numberFour.Value.Type = PointType.Living;
                            }
                        }
                        
                        numberTwo.Value.Coord = numberFour;
                        numberFour.Value.Coord = numberTwo;
                    }
                    
                    else if (_coordinateComparator.IsEquals(numberTwo.Value, numberThree.Value))
                    {
                        numberTwo.Value.Coord = numberThree;
                        numberThree.Value.Coord = numberTwo;
                    }

                    else if(_coordinateComparator.IsEquals(numberOne.Value, numberThree.Value))
                    {
                        if (!(VectorService.InsideTheAngle(numberThree.Value,numberFour.Value,
                                  numberTwo.Value,numberOne.Value,prevOne.Value) &&
                              VectorService.InsideTheAngle(numberThree.Value,prevThree!.Value,
                                  numberTwo.Value,numberOne.Value,prevOne.Value)))
                        {
                            double product = _lineService.VectorProduct(
                                numberOne.Value, numberTwo.Value, 
                                numberThree.Value, numberFour.Value);
                            if (product > 0 && 
                                numberOne.Value.Type == PointType.Useless && 
                                numberThree.Value.Type == PointType.Useless)
                            {
                                flagWereIntersection = true;
                                numberOfEnteringMarks++;
                                numberOne.Value.Type = PointType.Entering;
                                numberThree.Value.Type = PointType.Entering;
                            }
                            /*
                            else if (product < 0 && 
                                     numberOne.Value.Type == PointType.Useless && 
                                     numberThree.Value.Type == PointType.Useless)
                            {
                                flagWereIntersection = true;
                                numberOne.Value.Type = PointType.Living;
                                numberThree.Value.Type = PointType.Living;
                            }*/
                        }
                        numberOne.Value.Coord = numberThree;
                        numberThree.Value.Coord = numberOne;
                    }
                }
                
                //Overlay
                
                else if (intersection is { Item1: LinesIntersectionType.Overlay })
                {
                    LinkedListNode<CoordinateSupport>? intersectionNodeInClip = null;
                    LinkedListNode<CoordinateSupport>? intersectionNodeInCut = null;

                    //первый-четвёртый случаи
                    if(_lineService.IsCoordinateInSegmentBorders(numberThree.Value, numberOne.Value, numberTwo!.Value) &&
                        _lineService.IsCoordinateInSegmentBorders(numberTwo.Value, numberThree.Value, numberFour!.Value))
                    {
                        intersectionNodeInClip = new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberThree.Value));
                        intersectionNodeInCut = new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberTwo.Value));
                    /*
                        intersectionNodeInClip.Value.Coord = numberThree;
                        numberThree.Value.Coord = intersectionNodeInClip;

                        intersectionNodeInCut.Value.Coord = numberTwo;
                        numberTwo.Value.Coord = intersectionNodeInCut;
                    */
                    }
                    else if (_lineService.IsCoordinateInSegmentBorders(numberFour!.Value, numberOne.Value, numberTwo.Value) &&
                                _lineService.IsCoordinateInSegmentBorders(numberTwo.Value, numberThree.Value, numberFour.Value))
                    {
                        intersectionNodeInClip = new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberFour.Value));
                        intersectionNodeInCut = new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberTwo.Value));
                    /*
                        intersectionNodeInClip.Value.Coord = numberFour;
                        numberFour.Value.Coord = intersectionNodeInClip;

                        intersectionNodeInCut.Value.Coord = numberTwo;
                        numberTwo.Value.Coord = intersectionNodeInCut;
                    */
                    }
                    else if (_lineService.IsCoordinateInSegmentBorders(numberThree.Value, numberOne.Value, numberTwo.Value) &&
                                _lineService.IsCoordinateInSegmentBorders(numberOne.Value, numberThree.Value, numberFour.Value))
                    {
                        intersectionNodeInClip = new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberThree.Value));
                        intersectionNodeInCut = new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberOne.Value));
                    /*
                        intersectionNodeInClip.Value.Coord = numberThree;
                        numberThree.Value.Coord = intersectionNodeInClip;

                        intersectionNodeInCut.Value.Coord = numberOne;
                        numberOne.Value.Coord = intersectionNodeInCut;
                    */
                    }
                    else if (_lineService.IsCoordinateInSegmentBorders(numberFour.Value, numberOne.Value, numberTwo.Value) &&
                             _lineService.IsCoordinateInSegmentBorders(numberOne.Value, numberThree.Value, numberFour.Value))
                    {
                        intersectionNodeInClip = new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberFour.Value));
                        intersectionNodeInCut = new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberOne.Value));
                    /*
                        intersectionNodeInClip.Value.Coord = numberFour;
                        numberFour.Value.Coord = intersectionNodeInClip;

                        intersectionNodeInCut.Value.Coord = numberOne;
                        numberOne.Value.Coord = intersectionNodeInCut;
                    */
                    }

                    //добавление двух новых нод в листы
                    clipped.AddAfter(nodeI, intersectionNodeInClip!);
                    cutting.AddAfter(nodeJ, intersectionNodeInCut!);
                    
                    flagWereIntersectionOnCurrentIteration = true;
                }

                //Part

                else if (intersection is { Item1: LinesIntersectionType.Part })
                {
                    LinkedListNode<CoordinateSupport>? intersectionNodeInClip = null;
                    LinkedListNode<CoordinateSupport>? intersectionNodeInCut = null;

                    if (_lineService.IsCoordinateInIntervalBorders(numberOne.Value, numberThree.Value, numberFour!.Value))
                    {
                        intersectionNodeInCut = 
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberOne.Value));
                    }
                    else if (_lineService.IsCoordinateInIntervalBorders(numberFour.Value, numberOne.Value, numberTwo!.Value))
                    {
                        intersectionNodeInClip = 
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberFour.Value));
                    }
                    else if (_lineService.IsCoordinateInIntervalBorders(numberTwo.Value, numberThree.Value, numberFour.Value))
                    {
                        intersectionNodeInCut = 
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberTwo.Value));
                    }
                    else if (_lineService.IsCoordinateInIntervalBorders(numberThree.Value, numberOne.Value, numberTwo.Value))
                    {
                        intersectionNodeInClip =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberThree.Value));
                    }

                    if (intersectionNodeInClip != null)
                    {
                        clipped.AddAfter(nodeI, intersectionNodeInClip);
                        flagWereIntersectionOnCurrentIteration = true;
                    }

                    if (intersectionNodeInCut != null)
                    {
                        cutting.AddAfter(nodeJ, intersectionNodeInCut);
                        flagWereIntersectionOnCurrentIteration = true;
                    }
                }
                
                //Contains
                
                else if (intersection is { Item1: LinesIntersectionType.Contains })
                {
                    LinkedListNode<CoordinateSupport>? intersectionNodeFirst;
                    LinkedListNode<CoordinateSupport>? intersectionNodeSecond;
                    
                    if (_lineService.IsCoordinateInSegmentBorders(numberOne.Value, numberThree.Value, numberTwo!.Value))
                    {
                        intersectionNodeFirst =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberOne.Value));
                        intersectionNodeSecond =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberTwo.Value));
                        
                        cutting.AddAfter(numberThree, intersectionNodeFirst);
                        cutting.AddAfter(intersectionNodeFirst, intersectionNodeSecond);
                        
                        flagWereIntersectionOnCurrentIteration = true;
                    }
                    else if (_lineService.IsCoordinateInSegmentBorders(numberOne.Value, numberFour!.Value, numberTwo.Value))
                    {
                        intersectionNodeFirst =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberTwo.Value));
                        intersectionNodeSecond =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberOne.Value));
                        
                        cutting.AddAfter(numberThree, intersectionNodeFirst);
                        cutting.AddAfter(intersectionNodeFirst, intersectionNodeSecond);
                        
                        flagWereIntersectionOnCurrentIteration = true;
                    }
                    else if(_lineService.IsCoordinateInSegmentBorders(numberThree.Value, numberOne.Value, numberFour.Value))
                    {
                        intersectionNodeFirst =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberThree.Value));
                        intersectionNodeSecond =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberFour.Value));

                        clipped.AddAfter(numberOne, intersectionNodeFirst);
                        clipped.AddAfter(intersectionNodeFirst, intersectionNodeSecond);
                        
                        flagWereIntersectionOnCurrentIteration = true;
                    }
                    else if (_lineService.IsCoordinateInSegmentBorders(numberThree.Value, numberTwo.Value, numberFour.Value))
                    {
                        intersectionNodeFirst =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberFour.Value));
                        intersectionNodeSecond =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberThree.Value));

                        clipped.AddAfter(numberOne, intersectionNodeFirst);
                        clipped.AddAfter(intersectionNodeFirst, intersectionNodeSecond);
                        
                        flagWereIntersectionOnCurrentIteration = true;
                    }
                }
            }
        }

        if (!flagWereIntersection)
        {
            bool flagCuttingInClipped = true;
            foreach (Coordinate coordinate in cutting)
            {
                if (!IsPointInPolygon(coordinate, clippedCoordinates))
                {
                    flagCuttingInClipped = false;
                    break;
                }
            }

            bool flagClippedInCutting = true;
            foreach (Coordinate coordinate in clipped)
            {
                if (!IsPointInPolygon(coordinate, cuttingCoordinates))
                {
                    flagClippedInCutting = false;
                    break;
                }
            }

            if (!flagCuttingInClipped && !flagClippedInCutting)
            {
                return new List<LinearRing>(0);
            }
            
            if (!flagCuttingInClipped && flagClippedInCutting)
            {
                return new List<LinearRing>() { clippedCoordinates };
            }
            
            if (flagCuttingInClipped && !flagClippedInCutting)
            {
                return new List<LinearRing>() { cuttingCoordinates };
            }
            
            if (flagCuttingInClipped && flagClippedInCutting)
            {
                return new List<LinearRing>() { cuttingCoordinates };
            }
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
                
                LinkedListNode<CoordinateSupport>? startInCutting = nodeInClipped;

                LinkedListNode<CoordinateSupport>? startInClipped = nodeInClipped;
                
                do
                {
                    numberOfEnteringMarks--;
                    
                    for (LinkedListNode<CoordinateSupport>? nodeFromEToLInClipped = startInClipped;
                         nodeFromEToLInClipped!.Value.Type != PointType.Living;
                         nodeFromEToLInClipped = nodeFromEToLInClipped.Next)
                    {
                        figure.Add(nodeFromEToLInClipped.Value);

                        if (nodeFromEToLInClipped.Next == null)
                        {
                            nodeFromEToLInClipped = clipped.First;

                            if (nodeFromEToLInClipped!.Value.Type == PointType.Living)
                            {
                                startInCutting = nodeFromEToLInClipped.Value.Coord;
                                break;
                            }

                            figure.Add(nodeFromEToLInClipped.Value);
                        }

                        if (nodeFromEToLInClipped.Next!.Value.Type == PointType.Living)
                        {
                            startInCutting = nodeFromEToLInClipped.Next.Value.Coord;
                        }
                    }

                    for (LinkedListNode<CoordinateSupport>? nodeFromLToEInCutting = startInCutting;
                         nodeFromLToEInCutting!.Value.Type != PointType.Entering;
                         nodeFromLToEInCutting = nodeFromLToEInCutting.Next)
                    {
                        figure.Add(nodeFromLToEInCutting.Value);

                        if (nodeFromLToEInCutting.Next == null)
                        {
                            nodeFromLToEInCutting = cutting.First;

                            if (nodeFromLToEInCutting!.Value.Type == PointType.Entering)
                            {
                                startInClipped = nodeFromLToEInCutting.Value.Coord;
                                break;
                            }

                            figure.Add(nodeFromLToEInCutting.Value);
                        }

                        if (nodeFromLToEInCutting.Next!.Value.Type == PointType.Entering)
                        {
                            startInClipped = nodeFromLToEInCutting.Next.Value.Coord;
                        }
                    }
                } while (startInClipped != nodeInClipped);
                
                figure.Add(nodeInClipped.Value);
                result.Add(figure);

                if (numberOfEnteringMarks == 0)
                {
                    break;
                }
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////
        /*
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
        */
        return result.Select(enumerable => new LinearRing(enumerable.ToArray()));
    }

    void Print(LinkedList<CoordinateSupport> clipped, LinkedList<CoordinateSupport> cutting)
    {
        try
        {
            //Pass the filepath and filename to the StreamWriter Constructor
            StreamWriter sw =
                new StreamWriter(
                    "C:\\Users\\micha\\Desktop\\Миша\\work\\C#\\Geo\\geo-slicer\\GeoSlicer\\GridSlicer\\Helpers\\Bad.txt");
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