using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.PolygonClippingAlghorithm;

public class WeilerAthertonAlghorithm
{
    private readonly LinesIntersector _linesIntersector;
    private readonly AreasIntersector _areasIntersector = new();
    private const AreasIntersectionType SuitableAreaAreaIntersectionType = AreasIntersectionType.Inside;
    private readonly LineService _lineService;
    private readonly ICoordinateComparator _coordinateComparator;
    private readonly ContainsChecker _containsChecker;
    private readonly double _epsilon;

    public WeilerAthertonAlghorithm(LinesIntersector linesIntersector, LineService lineService,
        ICoordinateComparator coordinateComparator, ContainsChecker containsChecker, double epsilon)
    {
        _linesIntersector = linesIntersector;
        _lineService = lineService;
        _coordinateComparator = coordinateComparator;
        _containsChecker = containsChecker;
        _epsilon = epsilon;
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

        if (boxCoordinated.All(coordinate => _containsChecker.IsPointInLinearRing(coordinate, clipped))
            && !boxLinearRing.Intersects(clipped))
        {
            return IntersectionType.BoxInGeometry;
        }

        if (clipped.Coordinates.All(coordinate => _containsChecker.IsPointInLinearRing(coordinate, boxLinearRing)))
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

    public (LinesIntersectionType, Coordinate?) GetIntersection(CoordinateSupport line1Point1, CoordinateSupport line1Point2, CoordinateSupport line2Point1, CoordinateSupport line2Point2)
    {
        if (line1Point1.Equals2D(line1Point2)) return (LinesIntersectionType.NoIntersection, null);
        if (_areasIntersector.CheckIntersection(SuitableAreaAreaIntersectionType,
                line1Point1, line1Point2, line2Point1, line2Point2))
        {
            return _linesIntersector.GetIntersection(line1Point1, line1Point2, line2Point1, line2Point2);
        }
        return (LinesIntersectionType.NoIntersection, null);
    }
    
    //функция расстановки меток и создания ссылок между списками
    private (bool, int, int) MakeNotes(LinkedList<CoordinateSupport> clipped, LinkedList<CoordinateSupport> cutting)
    {
        bool flagWereIntersectionOnCurrentIteration = false;
        int numberOfEnteringMarks = 0;
        int numberOfLivingMarks = 0;

        bool flagWereIntersection = false;
        
        for (LinkedListNode<CoordinateSupport>? nodeI = clipped.First!;
             nodeI != null;
             nodeI = flagWereIntersectionOnCurrentIteration ? nodeI : nodeI.Next)
        {
            for (LinkedListNode<CoordinateSupport>? nodeJ = cutting.First!;
                 nodeJ != null;
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
                    intersection = GetIntersection(nodeI.Value, nodeI.Next!.Value, nodeJ.Value, nodeJ.Next!.Value);
                }
                else if (nodeI.Next != null)
                {
                    intersection = GetIntersection(nodeI.Value, nodeI.Next!.Value, nodeJ.Value, cutting.First!.Value);

                    numberFour = cutting.First;
                }
                else if (nodeJ.Next != null)
                {
                    intersection = GetIntersection(nodeI.Value, clipped.First!.Value, nodeJ.Value, nodeJ.Next!.Value);

                    numberTwo = clipped.First;
                }
                else
                {
                    intersection = GetIntersection(nodeI.Value, clipped.First!.Value, nodeJ.Value, cutting.First!.Value);

                    numberTwo = clipped.First;
                    numberFour = cutting.First;
                }

                //Inner

                if (intersection is { Item2: not null, Item1: LinesIntersectionType.Inner })
                {
                    LinkedListNode<CoordinateSupport> intersectionNodeInClip =
                        new LinkedListNode<CoordinateSupport>(new CoordinateSupport(intersection.Item2));
                    LinkedListNode<CoordinateSupport> intersectionNodeInCut =
                        new LinkedListNode<CoordinateSupport>(new CoordinateSupport(intersection.Item2));

                    clipped.AddAfter(nodeI, intersectionNodeInClip);
                    cutting.AddAfter(nodeJ, intersectionNodeInCut);
                    flagWereIntersectionOnCurrentIteration = true;
                }

                //TyShaped

                else if (intersection is { Item2: not null, Item1: LinesIntersectionType.TyShaped })
                {
                    LinkedListNode<CoordinateSupport> intersectionNode =
                        new LinkedListNode<CoordinateSupport>(new CoordinateSupport(intersection.Item2));

                    if (_coordinateComparator.IsEquals(intersectionNode.Value, numberOne.Value))
                    {
                        cutting.AddAfter(numberThree, intersectionNode);
                        flagWereIntersectionOnCurrentIteration = true;
                    }
                    else if (_coordinateComparator.IsEquals(intersectionNode.Value, numberTwo!.Value))
                    {
                        cutting.AddAfter(numberThree, intersectionNode);
                        flagWereIntersectionOnCurrentIteration = true;
                    }
                    else if (_coordinateComparator.IsEquals(intersectionNode.Value, numberThree.Value))
                    {
                        clipped.AddAfter(numberOne, intersectionNode);
                        flagWereIntersectionOnCurrentIteration = true;
                    }
                    else
                    {
                        clipped.AddAfter(numberOne, intersectionNode);
                        flagWereIntersectionOnCurrentIteration = true;
                    }
                }

                //Corner

                else if (intersection is { Item2: not null, Item1: LinesIntersectionType.Corner } or
                         { Item2: not null, Item1: LinesIntersectionType.Extension })
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

                    if (_coordinateComparator.IsEquals(numberOne.Value, numberThree.Value))
                    {
                        if (_lineService.InsideTheAngleWithoutBorders(numberOne.Value, numberTwo.Value,
                                numberFour.Value, numberThree.Value, prevThree.Value) &&
                            !_lineService.InsideTheAngleWithoutBorders(numberOne.Value, prevOne.Value,
                                numberFour.Value, numberThree.Value, prevThree.Value) &&
                            numberOne.Value.Type == PointType.Useless)
                        {
                            flagWereIntersection = true;
                            numberOfEnteringMarks++;
                            numberOne.Value.Type = PointType.Entering;
                            numberThree.Value.Type = PointType.Entering;
                            numberOne.Value.Coord = numberThree;
                            numberThree.Value.Coord = numberOne;
                        }

                        else if (!_lineService.InsideTheAngleWithoutBorders(numberOne.Value, numberTwo.Value,
                                     numberFour.Value, numberThree.Value, prevThree.Value) &&
                                 _lineService.InsideTheAngleWithoutBorders(numberOne.Value, prevOne.Value,
                                     numberFour.Value, numberThree.Value, prevThree.Value) &&
                                 numberOne.Value.Type == PointType.Useless)
                        {
                            flagWereIntersection = true;
                            numberOfLivingMarks++;
                            numberOne.Value.Type = PointType.Leaving;
                            numberThree.Value.Type = PointType.Leaving;
                            numberOne.Value.Coord = numberThree;
                            numberThree.Value.Coord = numberOne;
                        }
                        
                        else if ((!_lineService.InsideTheAngle(numberOne.Value, numberTwo.Value,
                                     prevOne.Value, numberThree.Value, prevThree.Value)
                                  && _lineService.InsideTheAngleWithoutBorders(numberOne.Value, numberFour.Value,
                                     prevOne.Value, numberThree.Value, prevThree.Value)
                                  || _lineService.InsideTheAngleWithoutBorders(numberOne.Value, numberTwo.Value,
                                     prevOne.Value, numberThree.Value, prevThree.Value)
                                  && !_lineService.InsideTheAngle(numberOne.Value, numberFour.Value,
                                     prevOne.Value, numberThree.Value, prevThree.Value))
                                 && (LineService.VectorProduct(
                                         prevOne.Value, numberOne.Value,
                                         numberOne.Value, numberTwo.Value) > _epsilon
                                     || LineService.VectorProduct(
                                         prevThree.Value, numberThree.Value,
                                         numberThree.Value, numberFour.Value) > _epsilon)
                                 && numberOne.Value.Type == PointType.Useless)
                        {
                            numberOne.Value.Type = PointType.SelfIntersection;
                            numberThree.Value.Type = PointType.SelfIntersection;
                            numberOne.Value.Coord = numberThree;
                            numberThree.Value.Coord = numberOne;
                            numberOfEnteringMarks++;
                        }
                    }

                    if (_coordinateComparator.IsEquals(numberTwo.Value, numberFour.Value))
                    {
                        if (_lineService.InsideTheAngleWithoutBorders(numberTwo.Value, numberOne.Value,
                                nextFour.Value, numberFour.Value, numberThree.Value) &&
                            !_lineService.InsideTheAngleWithoutBorders(numberTwo.Value, nextTwo.Value,
                                nextFour.Value, numberFour.Value, numberThree.Value) &&
                            numberTwo.Value.Type == PointType.Useless)
                        {
                            flagWereIntersection = true;
                            numberOfLivingMarks++;
                            numberTwo.Value.Type = PointType.Leaving;
                            numberFour.Value.Type = PointType.Leaving;
                            numberTwo.Value.Coord = numberFour;
                            numberFour.Value.Coord = numberTwo;
                        }

                        else if (!_lineService.InsideTheAngleWithoutBorders(numberTwo.Value, numberOne.Value,
                                     nextFour.Value, numberFour.Value, numberThree.Value) &&
                                 _lineService.InsideTheAngleWithoutBorders(numberTwo.Value, nextTwo.Value,
                                     nextFour.Value, numberFour.Value, numberThree.Value) &&
                                 numberTwo.Value.Type == PointType.Useless)
                        {
                            flagWereIntersection = true;
                            numberOfEnteringMarks++;
                            numberTwo.Value.Type = PointType.Entering;
                            numberFour.Value.Type = PointType.Entering;
                            numberTwo.Value.Coord = numberFour;
                            numberFour.Value.Coord = numberTwo;
                        }

                        
                        else if ((!_lineService.InsideTheAngle(numberTwo.Value, nextTwo.Value,
                                      numberOne.Value, numberTwo.Value, numberThree.Value)
                                  && _lineService.InsideTheAngleWithoutBorders(numberTwo.Value, nextFour.Value,
                                      numberOne.Value, numberTwo.Value, numberThree.Value)
                                  || _lineService.InsideTheAngleWithoutBorders(numberTwo.Value, nextTwo.Value,
                                      numberOne.Value, numberTwo.Value, numberThree.Value)
                                  && !_lineService.InsideTheAngle(numberTwo.Value, nextFour.Value,
                                      numberOne.Value, numberTwo.Value, numberThree.Value))
                                 && (LineService.VectorProduct(
                                         numberOne.Value, numberTwo.Value,
                                         numberTwo.Value, nextTwo.Value) > _epsilon
                                     || LineService.VectorProduct(
                                         numberThree.Value, numberFour.Value,
                                         numberFour.Value, nextFour.Value) > _epsilon)
                                 && numberTwo.Value.Type == PointType.Useless)
                        {
                            numberTwo.Value.Type = PointType.SelfIntersection;
                            numberFour.Value.Type = PointType.SelfIntersection;
                            numberTwo.Value.Coord = numberFour;
                            numberFour.Value.Coord = numberTwo;
                            numberOfEnteringMarks++;
                        }
                    }
                }

                //Overlay

                else if (intersection is { Item1: LinesIntersectionType.Overlay })
                {
                    LinkedListNode<CoordinateSupport>? intersectionNodeInClip = null;
                    LinkedListNode<CoordinateSupport>? intersectionNodeInCut = null;

                    //первый-четвёртый случаи
                    if (_lineService.IsCoordinateInSegmentBorders(numberThree.Value, numberOne.Value,
                            numberTwo!.Value) &&
                        _lineService.IsCoordinateInSegmentBorders(numberTwo.Value, numberThree.Value,
                            numberFour!.Value))
                    {
                        intersectionNodeInClip =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberThree.Value));
                        intersectionNodeInCut =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberTwo.Value));
                    }
                    else if (_lineService.IsCoordinateInSegmentBorders(numberFour!.Value, numberOne.Value,
                                 numberTwo.Value) &&
                             _lineService.IsCoordinateInSegmentBorders(numberTwo.Value, numberThree.Value,
                                 numberFour.Value))
                    {
                        intersectionNodeInClip =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberFour.Value));
                        intersectionNodeInCut =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberTwo.Value));
                    }
                    else if (_lineService.IsCoordinateInSegmentBorders(numberThree.Value, numberOne.Value,
                                 numberTwo.Value) &&
                             _lineService.IsCoordinateInSegmentBorders(numberOne.Value, numberThree.Value,
                                 numberFour.Value))
                    {
                        intersectionNodeInClip =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberThree.Value));
                        intersectionNodeInCut =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberOne.Value));
                    }
                    else if (_lineService.IsCoordinateInSegmentBorders(numberFour.Value, numberOne.Value,
                                 numberTwo.Value) &&
                             _lineService.IsCoordinateInSegmentBorders(numberOne.Value, numberThree.Value,
                                 numberFour.Value))
                    {
                        intersectionNodeInClip =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberFour.Value));
                        intersectionNodeInCut =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberOne.Value));
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

                    if (_lineService.IsCoordinateInIntervalBorders(numberOne.Value, numberThree.Value,
                            numberFour!.Value))
                    {
                        intersectionNodeInCut =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberOne.Value));
                    }
                    else if (_lineService.IsCoordinateInIntervalBorders(numberFour.Value, numberOne.Value,
                                 numberTwo!.Value))
                    {
                        intersectionNodeInClip =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberFour.Value));
                    }
                    else if (_lineService.IsCoordinateInIntervalBorders(numberTwo.Value, numberThree.Value,
                                 numberFour.Value))
                    {
                        intersectionNodeInCut =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberTwo.Value));
                    }
                    else if (_lineService.IsCoordinateInIntervalBorders(numberThree.Value, numberOne.Value,
                                 numberTwo.Value))
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
                    else if (_lineService.IsCoordinateInSegmentBorders(numberOne.Value, numberFour!.Value,
                                 numberTwo.Value))
                    {
                        intersectionNodeFirst =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberTwo.Value));
                        intersectionNodeSecond =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberOne.Value));

                        cutting.AddAfter(numberThree, intersectionNodeFirst);
                        cutting.AddAfter(intersectionNodeFirst, intersectionNodeSecond);

                        flagWereIntersectionOnCurrentIteration = true;
                    }
                    else if (_lineService.IsCoordinateInSegmentBorders(numberThree.Value, numberOne.Value,
                                 numberFour.Value))
                    {
                        intersectionNodeFirst =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberThree.Value));
                        intersectionNodeSecond =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberFour.Value));

                        clipped.AddAfter(numberOne, intersectionNodeFirst);
                        clipped.AddAfter(intersectionNodeFirst, intersectionNodeSecond);

                        flagWereIntersectionOnCurrentIteration = true;
                    }
                    else if (_lineService.IsCoordinateInSegmentBorders(numberThree.Value, numberTwo.Value,
                                 numberFour.Value))
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

        return (flagWereIntersection, numberOfEnteringMarks ,numberOfLivingMarks);
    }
    
    
    public Polygon[] WeilerAtherton(Polygon clippedPolygon, Polygon cuttingPolygon)
    {
        LinearRing clippedRingShell = clippedPolygon.Shell;
        LinearRing[] clippedRingsHoles = clippedPolygon.Holes;

        LinearRing[] allRingsClipped = new LinearRing[clippedRingsHoles.Length + 1];
        
        allRingsClipped[0] = clippedRingShell;
        for(int i = 0; i < clippedRingsHoles.Length; i++){
            allRingsClipped[i + 1] = clippedRingsHoles[i];
        }

        LinearRing cuttingRingShell = cuttingPolygon.Shell;
        
        //проверка: внешняя оболочка должна обходиться по часовой стрелке, внутренние границы - против часовой

        if (!TraverseDirection.IsClockwiseBypass(clippedRingShell)) 
        {
            TraverseDirection.ChangeDirection(clippedRingShell);
        }

        if (!TraverseDirection.IsClockwiseBypass(cuttingRingShell))
        {
            TraverseDirection.ChangeDirection(cuttingRingShell);
        }
        
        foreach (var hole in clippedRingsHoles)
        {
            if (TraverseDirection.IsClockwiseBypass(hole))
            {
                TraverseDirection.ChangeDirection(hole);
            }
        }
        
        LinkedList<CoordinateSupport>[] clippedListArray = new LinkedList<CoordinateSupport>[allRingsClipped.Length];
        
        for (int i = 0; i < allRingsClipped.Length; i++)
        {
            clippedListArray[i] = CoordinateToCoordinateSupport(allRingsClipped[i]);
        }
        
        LinkedList<CoordinateSupport> cutting = CoordinateToCoordinateSupport(cuttingRingShell);
        
        //нужно определить, с какими дырами пересекается cutting,
        //потом пересечь каждую дыру по отдельнности 
        //
        int numberOfEnteringMarks = 0;
        int numberOfLivingMarks = 0;//может понадобиться для отладки: должно быть равно numberOfEnteringMarks
        
        Envelope envelopeCutting = new Envelope(cuttingRingShell.Coordinates);
        double cuttingMaxX = envelopeCutting.MaxX;
        double cuttingMaxY = envelopeCutting.MaxY;
        double cuttingMinX = envelopeCutting.MinX;
        double cuttingMinY = envelopeCutting.MinY;

        List<LinearRing> maybeInnerRings = new List<LinearRing>();

        for(int i = 0; i < clippedListArray.Length; i++)
        {
            Envelope envelopeClipped = new Envelope(allRingsClipped[i].Coordinates);
            if (envelopeClipped.MinY <= cuttingMaxY && envelopeClipped.MaxY >= cuttingMaxY && 
                envelopeClipped.MaxX >= cuttingMinX && envelopeClipped.MinX <= cuttingMinX ||
                
                envelopeClipped.MinY <= cuttingMaxY && envelopeClipped.MaxY >= cuttingMaxY && 
                envelopeClipped.MinX <= cuttingMaxX && envelopeClipped.MaxX >= cuttingMaxX ||
                
                envelopeClipped.MaxY >= cuttingMinY && envelopeClipped.MinY <= cuttingMinY && 
                envelopeClipped.MinX <= cuttingMaxX && envelopeClipped.MaxX >= cuttingMaxX ||
                
                envelopeClipped.MaxY >= cuttingMinY && envelopeClipped.MinY <= cuttingMinY && 
                envelopeClipped.MaxX >= cuttingMinX && envelopeClipped.MinX <= cuttingMinX ||
                
                envelopeClipped.MinY <= cuttingMaxY && envelopeClipped.MaxY >= cuttingMaxY &&
                envelopeClipped.MinX >= cuttingMinX && envelopeClipped.MaxX <= cuttingMaxX ||
                
                envelopeClipped.MinY <= cuttingMinY && envelopeClipped.MaxY >= cuttingMinY &&
                envelopeClipped.MinX >= cuttingMinX && envelopeClipped.MaxX <= cuttingMaxX ||
                
                envelopeClipped.MinY >= cuttingMinY && envelopeClipped.MaxY < cuttingMaxY &&
                envelopeClipped.MinX <= cuttingMaxX && envelopeClipped.MaxX > cuttingMaxX ||
                
                envelopeClipped.MinY >= cuttingMinY && envelopeClipped.MaxY <= cuttingMaxY &&
                envelopeClipped.MinX <= cuttingMinX && envelopeClipped.MaxX >= cuttingMinX ||
                
                envelopeClipped.MaxY <= cuttingMaxY && envelopeClipped.MinY >= cuttingMinY && 
                envelopeClipped.MaxX <= cuttingMaxX && envelopeClipped.MinX >= cuttingMinX)
            {
                var tuple = MakeNotes(clippedListArray[i], cutting);
                bool flagWereIntersection = tuple.Item1;
                numberOfEnteringMarks += tuple.Item2;
                numberOfLivingMarks += tuple.Item3;
                
                if (!flagWereIntersection)
                {
                    maybeInnerRings.Add(allRingsClipped[i]);
                }
                
                PrintMarks(clippedListArray[i], cutting, "Bad" + i + ".txt.ignore");
            }
        }

        //обход списков, формирование пересечений многоугольников

        List<IEnumerable<Coordinate>> result = new();
        
        //Найдём лист, в котором есть метка Entering, чтобы из него стартовать.
        //Если во внешней оболочке clipped нет Entering, то перейдём в дыру
        
        int startedClipped = 0;
        bool findEntering = false;
        
        for(int i = 0; i < clippedListArray.Length && findEntering == false; i++) 
        {
            for (LinkedListNode<CoordinateSupport>? nodeInClipped = clippedListArray[i].First;
                 nodeInClipped != null;
                 nodeInClipped = nodeInClipped.Next)
            {
                if (nodeInClipped.Value.Type == PointType.Entering)
                {
                    startedClipped = i;
                    findEntering = true;
                    break;
                }
            }
        }
        
        
        for (LinkedListNode<CoordinateSupport>? nodeInClipped = clippedListArray[startedClipped].First;
             nodeInClipped != null;
             nodeInClipped = nodeInClipped.Next)
        {
            if (nodeInClipped.Value.Type != PointType.Entering && nodeInClipped.Value.Type != PointType.SelfIntersection) continue;
            
            List<Coordinate> figure = new();

            LinkedListNode<CoordinateSupport>? startInCutting = nodeInClipped;
            LinkedListNode<CoordinateSupport>? startInClipped = nodeInClipped;

            do
            {
                numberOfEnteringMarks--;
                int count = 0;
                for (LinkedListNode<CoordinateSupport>? nodeFromEToLInClipped = startInClipped;
                     nodeFromEToLInClipped!.Value.Type != PointType.Leaving && (nodeFromEToLInClipped!.Value.Type != PointType.SelfIntersection || count == 0);
                     nodeFromEToLInClipped = nodeFromEToLInClipped.Next)
                {
                    figure.Add(nodeFromEToLInClipped.Value);
                    count = 1;
                    if (nodeFromEToLInClipped.Next == null)
                    {
                        //nodeFromEToLInClipped = clippedListArray[currentClipped].First;
                        
                        //чтобы взять First, не зная, в каком листе массива мы находимся,
                        //можно в цикле дойти до начала через Previous: while(node.Previous!=null)
                        //или можно придумать что-то с метками (у каждой точки листа хранить ссылку на первый элемент листа)
                        //возможны другие варианты
                        
                        //выбрал первый вариант, он проще в реализации
                        
                        while (nodeFromEToLInClipped.Previous != null)
                        {
                            nodeFromEToLInClipped = nodeFromEToLInClipped.Previous;
                        }

                        if (nodeFromEToLInClipped!.Value.Type == PointType.Leaving || nodeFromEToLInClipped!.Value.Type == PointType.SelfIntersection)
                        {
                            startInCutting = nodeFromEToLInClipped.Value.Coord;
                            startInCutting!.Value.Type = PointType.Useless;
                            break;
                        }

                        figure.Add(nodeFromEToLInClipped.Value);
                    }

                    if (nodeFromEToLInClipped.Next!.Value.Type == PointType.Leaving)
                    {
                        startInCutting = nodeFromEToLInClipped.Next.Value.Coord;
                        startInCutting!.Value.Type = PointType.Useless;
                    }
                    else if (nodeFromEToLInClipped.Next!.Value.Type == PointType.SelfIntersection)
                    {
                        startInCutting = nodeFromEToLInClipped.Next.Value.Coord;
                    }
                }
                count = 0;
                for (LinkedListNode<CoordinateSupport>? nodeFromLToEInCutting = startInCutting;
                     nodeFromLToEInCutting!.Value.Type != PointType.Entering && (nodeFromLToEInCutting!.Value.Type != PointType.SelfIntersection || count == 0);
                     nodeFromLToEInCutting = nodeFromLToEInCutting.Next)
                {
                    figure.Add(nodeFromLToEInCutting.Value);
                    count = 1;
                    if (nodeFromLToEInCutting.Next == null)
                    {
                        //nodeFromLToEInCutting = cutting.First;

                        while (nodeFromLToEInCutting.Previous != null)
                        {
                            nodeFromLToEInCutting = nodeFromLToEInCutting.Previous;
                        }
                        
                        if (nodeFromLToEInCutting!.Value.Type == PointType.Entering || nodeFromLToEInCutting!.Value.Type == PointType.SelfIntersection)
                        {
                            startInClipped = nodeFromLToEInCutting.Value.Coord;
                            startInClipped!.Value.Type = PointType.Useless;
                            break;
                        }

                        figure.Add(nodeFromLToEInCutting.Value);
                    }

                    if (nodeFromLToEInCutting.Next!.Value.Type == PointType.Entering)
                    {
                        startInClipped = nodeFromLToEInCutting.Next.Value.Coord;
                        startInClipped!.Value.Type = PointType.Useless;
                    }
                    else if (nodeFromLToEInCutting.Next!.Value.Type == PointType.SelfIntersection)
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
        
        PrintMaybeInnerRings(maybeInnerRings);
        PrintResult(result);
        
        if (result.Count == 0 && maybeInnerRings.Count == 0)
        {
            bool flagCuttingInClipped = true;
            foreach (CoordinateSupport coordinate in cutting)
            {
                if (!_containsChecker.IsPointInLinearRing(coordinate, clippedRingShell))
                {
                    flagCuttingInClipped = false;
                    break;
                }
            }

            bool flagClippedInCutting = true;
            foreach (CoordinateSupport coordinate in clippedListArray[0])
            {
                if (!_containsChecker.IsPointInLinearRing(coordinate, cuttingRingShell))
                {
                    flagClippedInCutting = false;
                    break;
                }
            }

            if (!flagCuttingInClipped && !flagClippedInCutting)
            {
                return new Polygon[0];
            }

            if (!flagCuttingInClipped && flagClippedInCutting)
            {
                return new Polygon[]
                {
                    new Polygon(new LinearRing(clippedRingShell.Coordinates))
                };
            }

            if (flagCuttingInClipped && !flagClippedInCutting)
            {
                return new Polygon[]
                {
                    new Polygon(new LinearRing(cuttingRingShell.Coordinates))
                };
            }

            if (flagCuttingInClipped && flagClippedInCutting)
            {
                return new Polygon[]
                {
                    new Polygon(new LinearRing(cuttingRingShell.Coordinates))
                };
            }
        }
        
        
        if (result.Count != 0)
        { 
            Polygon[] resultPolygons = new Polygon[result.Count];
            for (int i = 0; i < result.Count; i++)
            {
                //todo:linked list заменить на массивы координат, т.к. использование ToArray не оптимально
                Coordinate[] arrayCoordinates = result[i].ToArray();
                LinearRing ringShell = new LinearRing(arrayCoordinates);
                List<LinearRing> holes = new List<LinearRing>();

                foreach (var maybeInnerRing in maybeInnerRings)
                {
                    bool isPolygonInPolygon = true;

                    for (int j = 0; j < maybeInnerRing.Coordinates.Length; j++)
                    {
                        if (!_containsChecker.IsPointInLinearRing(maybeInnerRing.Coordinates[j], ringShell))
                        {
                            isPolygonInPolygon = false;
                        }
                    }

                    if (isPolygonInPolygon)
                    {
                        holes.Add(maybeInnerRing);
                    }
                }

                resultPolygons[i] = new Polygon(ringShell, holes.ToArray());
            }

            return resultPolygons;
        }

        
        bool isCuttingInClipped = true;
        
        for (int i = 0; i < cuttingRingShell.Coordinates.Length; i++)
        {
            if (!_containsChecker.IsPointInLinearRing(cuttingRingShell.Coordinates[i], clippedRingShell))
            {
                isCuttingInClipped = false;
            }
        }

        if (isCuttingInClipped)
        {
            List<LinearRing> holes = new List<LinearRing>();
            foreach (var maybeInnerRing in maybeInnerRings)
            {
                bool isPolygonInCutting = true;

                for (int i = 0; i < maybeInnerRing.Coordinates.Length; i++)
                {
                    if (!_containsChecker.IsPointInLinearRing(maybeInnerRing.Coordinates[i], cuttingRingShell))
                    {
                        isPolygonInCutting = false;
                    }
                }

                if (isPolygonInCutting)
                {
                    holes.Add(maybeInnerRing);
                }
            }

            return new []{ new Polygon(cuttingRingShell, holes.ToArray()) };
        }
        
        return new Polygon[]{ };
    }

    
    //метод, который нужен для тестов, которые использовали старый метод с другой сигнатурой
    public IEnumerable<LinearRing> WeilerAtherton(
        LinearRing clippedCoordinates, LinearRing cuttingCoordinates)
    {
        Polygon clippedPolygon = new Polygon(clippedCoordinates);
        Polygon cuttingPolygon = new Polygon(cuttingCoordinates);
        Polygon[] resultAfterNewWeilerAtherton = WeilerAtherton(clippedPolygon, cuttingPolygon);

        List<LinearRing> result = new List<LinearRing>(resultAfterNewWeilerAtherton.Length);
        foreach (var polygon in resultAfterNewWeilerAtherton)
        {
            result.Add(polygon.Shell);
            var holes = polygon.Holes;
            foreach (var hole in holes)
            {
                result.Add(hole);
            }
        }

        return result;
    }

    void PrintMaybeInnerRings(List<LinearRing> maybeInnerRings, String path = "Maybe.txt.ignore")
    {
        try
        {
            StreamWriter sw =
                new StreamWriter("..\\..\\..\\" + path);
            
            foreach (var ring in maybeInnerRings)
            {
                sw.WriteLine();
                for (int i = 0; i < ring.Coordinates.Length; i++)
                {
                    sw.Write(ring.Coordinates[i]+ " ");
                }

                sw.WriteLine();
                sw.WriteLine("**********************************");
            }
            
            sw.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }

    void PrintResult(List<IEnumerable<Coordinate>> result, String path = "Result.txt.ignore")
    {
        try
        {
            StreamWriter sw =
                new StreamWriter("..\\..\\..\\" + path);
            
            foreach (var ring in result)
            {
                sw.WriteLine();

                foreach (Coordinate coord in ring)
                {
                    sw.Write(coord + " ");
                }

                sw.WriteLine();
                sw.WriteLine("**********************************");
            }
            
            sw.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }
    
    void PrintMarks(LinkedList<CoordinateSupport> clipped, LinkedList<CoordinateSupport> cutting, String path = "Bad.txt.ignore")
    {
        try
        {
            StreamWriter sw =
                new StreamWriter("..\\..\\..\\" + path);
            
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
                else if (i.Value.Type != PointType.Useless)
                {
                    sw.WriteLine("Value.Coord.Next = " + cutting.First!.Value);
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
                else if (i.Value.Type != PointType.Useless)
                {
                    sw.WriteLine("Value.Coord.Next = " + clipped.First!.Value);
                }
            }

            sw.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }
}