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
        // Вернёт LinkedList с координатами всех точек кольца без последней (равной первой)
    }



    private (LinesIntersectionType, Coordinate?) GetIntersection(CoordinateSupport line1Point1,
        CoordinateSupport line1Point2, CoordinateSupport line2Point1, CoordinateSupport line2Point2)
    {
        if (line1Point1.Equals2D(line1Point2)) return (LinesIntersectionType.NoIntersection, null);
        if (_areasIntersector.CheckIntersection(SuitableAreaAreaIntersectionType,
                line1Point1, line1Point2, line2Point1, line2Point2))
        {
            return _linesIntersector.GetIntersection(line1Point1, line1Point2, line2Point1, line2Point2);
        }

        return (LinesIntersectionType.NoIntersection, null);
    }

    // Функция расстановки меток и создания ссылок между списками
    private (bool, int, int) MakeNotes(LinkedList<CoordinateSupport> clipped, LinkedList<CoordinateSupport> cutting)
    {
        bool flagWereIntersectionOnCurrentIteration = false;
        int numberOfEnteringMarks = 0;
        int numberOfLeavingMarks = 0;

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

                if (nodeJ.Next == null)
                {
                    numberFour = cutting.First;
                }
                if (nodeI.Next == null)
                {
                    numberTwo = clipped.First;
                }

                (LinesIntersectionType, Coordinate?) intersection = GetIntersection(numberOne.Value, numberTwo!.Value, 
                    numberThree.Value, numberFour!.Value);

                // Inner

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

                // TyShaped

                else if (intersection is { Item2: not null, Item1: LinesIntersectionType.TyShaped })
                {
                    flagWereIntersectionOnCurrentIteration = true;
                    
                    LinkedListNode<CoordinateSupport> intersectionNode =
                        new LinkedListNode<CoordinateSupport>(new CoordinateSupport(intersection.Item2));

                    if (_coordinateComparator.IsEquals(intersectionNode.Value, numberOne.Value))
                    {
                        cutting.AddAfter(numberThree, intersectionNode);
                    }
                    else if (_coordinateComparator.IsEquals(intersectionNode.Value, numberTwo.Value))
                    {
                        cutting.AddAfter(numberThree, intersectionNode);
                    }
                    else if (_coordinateComparator.IsEquals(intersectionNode.Value, numberThree.Value))
                    {
                        clipped.AddAfter(numberOne, intersectionNode);
                    }
                    else
                    {
                        clipped.AddAfter(numberOne, intersectionNode);
                    }
                }

                // Corner

                else if (intersection is { Item2: not null, Item1: LinesIntersectionType.Corner } or
                         { Item2: not null, Item1: LinesIntersectionType.Extension })
                {
                    // FlagWereIntersection = true;

                    // Определили точки для углов в Corner
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

                    if (numberTwo.Next != null)
                    {
                        nextTwo = numberTwo.Next;
                    }

                    if (numberFour.Next != null)
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
                            numberOfLeavingMarks++;
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
                            numberOfLeavingMarks++;
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
                            numberOfLeavingMarks++;
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
                            numberOfLeavingMarks++;
                        }
                    }
                }

                // Overlay

                else if (intersection is { Item1: LinesIntersectionType.Overlay })
                {
                    LinkedListNode<CoordinateSupport>? intersectionNodeInClip = null;
                    LinkedListNode<CoordinateSupport>? intersectionNodeInCut = null;

                    // Первый-четвёртый случаи
                    if (_lineService.IsCoordinateInSegmentBorders(numberThree.Value, numberOne.Value,
                            numberTwo.Value) &&
                        _lineService.IsCoordinateInSegmentBorders(numberTwo.Value, numberThree.Value,
                            numberFour.Value))
                    {
                        intersectionNodeInClip =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberThree.Value));
                        intersectionNodeInCut =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberTwo.Value));
                    }
                    else if (_lineService.IsCoordinateInSegmentBorders(numberFour.Value, numberOne.Value,
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

                    // Добавление двух новых нод в листы
                    clipped.AddAfter(nodeI, intersectionNodeInClip!);
                    cutting.AddAfter(nodeJ, intersectionNodeInCut!);

                    flagWereIntersectionOnCurrentIteration = true;
                }

                // Part

                else if (intersection is { Item1: LinesIntersectionType.Part })
                {
                    LinkedListNode<CoordinateSupport>? intersectionNodeInClip = null;
                    LinkedListNode<CoordinateSupport>? intersectionNodeInCut = null;

                    if (_lineService.IsCoordinateInIntervalBorders(numberOne.Value, numberThree.Value,
                            numberFour.Value))
                    {
                        intersectionNodeInCut =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberOne.Value));
                    }
                    else if (_lineService.IsCoordinateInIntervalBorders(numberFour.Value, numberOne.Value,
                                 numberTwo.Value))
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

                // Contains

                else if (intersection is { Item1: LinesIntersectionType.Contains })
                {
                    LinkedListNode<CoordinateSupport>? intersectionNodeFirst;
                    LinkedListNode<CoordinateSupport>? intersectionNodeSecond;

                    if (_lineService.IsCoordinateInSegmentBorders(numberOne.Value, numberThree.Value, numberTwo.Value))
                    {
                        intersectionNodeFirst =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberOne.Value));
                        intersectionNodeSecond =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(numberTwo.Value));

                        cutting.AddAfter(numberThree, intersectionNodeFirst);
                        cutting.AddAfter(intersectionNodeFirst, intersectionNodeSecond);

                        flagWereIntersectionOnCurrentIteration = true;
                    }
                    else if (_lineService.IsCoordinateInSegmentBorders(numberOne.Value, numberFour.Value,
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
        
        if (numberOfEnteringMarks != numberOfLeavingMarks)
        {
            throw new DifferentNumbersOfPointTypes();
        }

        return (flagWereIntersection, numberOfEnteringMarks, numberOfLeavingMarks);
    }


    public Polygon[] WeilerAtherton(Polygon clippedPolygon, LinearRing cuttingRingShell)
    {
        LinearRing clippedRingShell = clippedPolygon.Shell;
        LinearRing[] clippedRingsHoles = clippedPolygon.Holes;

        LinearRing[] allRingsClipped = new LinearRing[clippedRingsHoles.Length + 1];

        allRingsClipped[0] = clippedRingShell;
        for (int i = 0; i < clippedRingsHoles.Length; i++)
        {
            allRingsClipped[i + 1] = clippedRingsHoles[i];
        }

        // Проверка: внешняя оболочка должна обходиться по часовой стрелке, внутренние границы - против часовой

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

        // Нужно определить, с какими дырами пересекается cutting,
        // потом пересечь каждую дыру по отдельнности 

        int numberOfEnteringMarks = 0;
        int numberOfLeavingMarks = 0;

        // Может понадобиться для отладки: должно быть равно numberOfEnteringMarks

        cuttingRingShell.GetMinAndMaxOrdinates(
            out double cuttingMinX, out double cuttingMinY, out double cuttingMaxX, out double cuttingMaxY);

        List<LinearRing> maybeInnerRings = new List<LinearRing>();

        for (int i = 0; i < clippedListArray.Length; i++)
        {
            allRingsClipped[i].GetMinAndMaxOrdinates(
                out double clippedMinX, out double clippedMinY, out double clippedMaxX, out double clippedMaxY);
            if (clippedMinY <= cuttingMaxY && clippedMaxY >= cuttingMaxY &&
                clippedMaxX >= cuttingMinX && clippedMinX <= cuttingMinX ||
                clippedMinY <= cuttingMaxY && clippedMaxY >= cuttingMaxY &&
                clippedMinX <= cuttingMaxX && clippedMaxX >= cuttingMaxX ||
                clippedMaxY >= cuttingMinY && clippedMinY <= cuttingMinY &&
                clippedMinX <= cuttingMaxX && clippedMaxX >= cuttingMaxX ||
                clippedMaxY >= cuttingMinY && clippedMinY <= cuttingMinY &&
                clippedMaxX >= cuttingMinX && clippedMinX <= cuttingMinX ||
                clippedMinY <= cuttingMaxY && clippedMaxY >= cuttingMaxY &&
                clippedMinX >= cuttingMinX && clippedMaxX <= cuttingMaxX ||
                clippedMinY <= cuttingMinY && clippedMaxY >= cuttingMinY &&
                clippedMinX >= cuttingMinX && clippedMaxX <= cuttingMaxX ||
                clippedMinY >= cuttingMinY && clippedMaxY <= cuttingMaxY &&
                clippedMinX <= cuttingMaxX && clippedMaxX >= cuttingMaxX ||
                clippedMinY >= cuttingMinY && clippedMaxY <= cuttingMaxY &&
                clippedMinX <= cuttingMinX && clippedMaxX >= cuttingMinX ||
                clippedMaxY <= cuttingMaxY && clippedMinY >= cuttingMinY &&
                clippedMaxX <= cuttingMaxX && clippedMinX >= cuttingMinX)
            {
                var tuple = MakeNotes(clippedListArray[i], cutting);
                // PrintMarks(clippedListArray[i], cutting, "Bad" + i + ".txt.ignore");
                bool flagWereIntersection = tuple.Item1;
                numberOfEnteringMarks += tuple.Item2;
                numberOfLeavingMarks += tuple.Item3;

                if (!flagWereIntersection)
                {
                    maybeInnerRings.Add(allRingsClipped[i]);
                }
            }
        }

        // Обход списков, формирование пересечений многоугольников

        List<IEnumerable<Coordinate>> result = new();

        for (LinkedListNode<CoordinateSupport>? nodeInCutting = cutting.First;
             nodeInCutting != null;
             nodeInCutting = nodeInCutting!.Next)
        {
            if (nodeInCutting.Value.Type != PointType.Entering &&
                nodeInCutting.Value.Type != PointType.SelfIntersection) continue;

            // Может трактоваться и как вход, и как выход. Ищем, какая будет после нее, и трактуем как противоположную
            if (nodeInCutting.Value.Type == PointType.SelfIntersection)
            {
                bool isEntering = true;
                LinkedListNode<CoordinateSupport>? temp;
                for (temp = nodeInCutting.Value.Coord!.Next;
                     temp != null;
                     temp = temp.Next)
                {
                    if (temp.Value.Type == PointType.Entering)
                    {
                        isEntering = false;
                        nodeInCutting = nodeInCutting.Previous;
                        break;
                    }

                    if (temp.Value.Type == PointType.Leaving)
                    {
                        break;
                    }
                }

                if (temp == null)
                {
                    LinkedListNode<CoordinateSupport> firstInRing = nodeInCutting!.Value.Coord!;
                    while (firstInRing.Previous != null)
                    {
                        firstInRing = firstInRing.Previous;
                    }
                    LinkedListNode<CoordinateSupport> endOfCycle = nodeInCutting.Value.Coord!;
                    for (temp = firstInRing;
                         temp != endOfCycle;
                         temp = temp.Next)
                    {
                        if (temp!.Value.Type == PointType.Entering)
                        {
                            isEntering = false;
                            nodeInCutting = nodeInCutting.Previous;
                            break;
                        }

                        if (temp.Value.Type == PointType.Leaving)
                        {
                            break;
                        }
                    }

                    if (temp == endOfCycle)
                    {
                        //isEntering = false;
                        // todo Разобраться с одним selfIntersection без входов и выходов

                        throw new Exception(
                            "Встретили ситуацию, существование которой считали невозможным. Надо фиксить");
                    }
                }

                if (!isEntering) continue;
            }

            List<Coordinate> figure = new(clippedRingShell.Count);

            LinkedListNode<CoordinateSupport>? startInCutting = nodeInCutting;
            LinkedListNode<CoordinateSupport>? startInClipped = nodeInCutting!.Value.Coord;

            do
            {
                numberOfEnteringMarks--;
                int count = 0;
                for (LinkedListNode<CoordinateSupport> nodeFromEToLInClipped = startInClipped!;
                     nodeFromEToLInClipped!.Value.Type != PointType.Leaving &&
                     nodeFromEToLInClipped.Value.Type != PointType.SelfIntersection || count == 0;
                     nodeFromEToLInClipped = nodeFromEToLInClipped.Next)
                {
                    figure.Add(nodeFromEToLInClipped.Value);
                    count = 1;
                    if (nodeFromEToLInClipped.Next == null)
                    {
                        // Чтобы взять First, не зная, в каком листе массива мы находимся,
                        // можно в цикле дойти до начала через Previous: while(node.Previous!=null)
                        // или можно придумать что-то с метками (у каждой точки листа хранить ссылку на первый элемент листа)

                        nodeFromEToLInClipped = startInClipped!;
                        while (nodeFromEToLInClipped.Previous != null)
                        {
                            nodeFromEToLInClipped = nodeFromEToLInClipped.Previous;
                        }

                        if (nodeFromEToLInClipped.Value.Type == PointType.Leaving ||
                            nodeFromEToLInClipped.Value.Type == PointType.SelfIntersection)
                        {
                            startInCutting = nodeFromEToLInClipped.Value.Coord;
                            if (nodeFromEToLInClipped.Value.Type == PointType.SelfIntersection)
                            {
                                nodeFromEToLInClipped.Value.Type = PointType.Entering;
                                startInCutting!.Value.Type = PointType.Entering;
                            }
                            else
                            {
                                nodeFromEToLInClipped.Value.Type = PointType.Useless;
                                startInCutting!.Value.Type = PointType.Useless;
                            }

                            break;
                        }

                        figure.Add(nodeFromEToLInClipped.Value);
                    }

                    if (nodeFromEToLInClipped.Next!.Value.Type == PointType.Leaving)
                    {
                        startInCutting = nodeFromEToLInClipped.Next.Value.Coord;
                        nodeFromEToLInClipped.Next!.Value.Type = PointType.Useless;
                        startInCutting!.Value.Type = PointType.Useless;
                        break;
                    }

                    if (nodeFromEToLInClipped.Next!.Value.Type == PointType.SelfIntersection)
                    {
                        startInCutting = nodeFromEToLInClipped.Next.Value.Coord;
                        nodeFromEToLInClipped.Next!.Value.Type = PointType.Entering;
                        startInCutting!.Value.Type = PointType.Entering;
                        break;
                    }
                }

                count = 0;
                for (LinkedListNode<CoordinateSupport> nodeFromLToEInCutting = startInCutting!;
                     nodeFromLToEInCutting!.Value.Type != PointType.Entering &&
                     nodeFromLToEInCutting.Value.Type != PointType.SelfIntersection || count == 0;
                     nodeFromLToEInCutting = nodeFromLToEInCutting.Next)
                {
                    figure.Add(nodeFromLToEInCutting.Value);
                    count = 1;
                    if (nodeFromLToEInCutting.Next == null)
                    {
                        nodeFromLToEInCutting = startInCutting!;
                        while (nodeFromLToEInCutting.Previous != null)
                        {
                            nodeFromLToEInCutting = nodeFromLToEInCutting.Previous;
                        }

                        if (nodeFromLToEInCutting.Value.Type == PointType.Entering ||
                            nodeFromLToEInCutting.Value.Type == PointType.SelfIntersection)
                        {
                            startInClipped = nodeFromLToEInCutting.Value.Coord;
                            if (nodeFromLToEInCutting.Value.Type == PointType.SelfIntersection)
                            {
                                nodeFromLToEInCutting.Value.Type = PointType.Leaving;
                                startInClipped!.Value.Type = PointType.Leaving;
                            }
                            else
                            {
                                nodeFromLToEInCutting.Value.Type = PointType.Useless;
                                startInClipped!.Value.Type = PointType.Useless;
                            }

                            break;
                        }

                        figure.Add(nodeFromLToEInCutting.Value);
                    }

                    if (nodeFromLToEInCutting.Next!.Value.Type == PointType.Entering)
                    {
                        startInClipped = nodeFromLToEInCutting.Next.Value.Coord;
                        startInClipped!.Value.Type = PointType.Useless;
                        nodeFromLToEInCutting.Next!.Value.Type = PointType.Useless;
                        break;
                    }

                    if (nodeFromLToEInCutting.Next!.Value.Type == PointType.SelfIntersection)
                    {
                        startInClipped = nodeFromLToEInCutting.Next.Value.Coord;
                        startInClipped!.Value.Type = PointType.Leaving;
                        nodeFromLToEInCutting.Next!.Value.Type = PointType.Leaving;
                        break;
                    }
                }
            } while (startInClipped != nodeInCutting.Value.Coord);

            figure.Add(nodeInCutting.Value);
            result.Add(figure);

            if (numberOfEnteringMarks == 0)
            {
                break;
            }
        }

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
                return Array.Empty<Polygon>();
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
                Coordinate[] arrayCoordinates = result[i].ToArray();
                LinearRing ringShell = new LinearRing(arrayCoordinates);
                List<LinearRing> holes = new List<LinearRing>();

                foreach (var maybeInnerRing in maybeInnerRings)
                {
                    bool isPolygonInPolygon = true;

                    foreach (var arrCoordinate in maybeInnerRing.Coordinates)
                    {
                        if (!_containsChecker.IsPointInLinearRing(arrCoordinate, ringShell))
                        {
                            isPolygonInPolygon = false;
                            break;
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
                break;
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

            return new[] { new Polygon(cuttingRingShell, holes.ToArray()) };
        }

        return new Polygon[] { };
    }

    void PrintMarks(LinkedList<CoordinateSupport> clipped, LinkedList<CoordinateSupport> cutting,
        String path = "Bad.txt.ignore")
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