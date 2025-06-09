using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.PolygonClippingAlgorithm;

public class WeilerAthertonAlgorithm
{
    private readonly LinesIntersector _linesIntersector;
    private readonly AreasIntersector _areasIntersector = new();
    private readonly LineService _lineService;
    private readonly ICoordinateComparator _coordinateComparator;
    private readonly ContainsChecker _containsChecker;


    public WeilerAthertonAlgorithm(LinesIntersector linesIntersector, LineService lineService,
        ICoordinateComparator coordinateComparator, ContainsChecker containsChecker)
    {
        _linesIntersector = linesIntersector;
        _lineService = lineService;
        _coordinateComparator = coordinateComparator;
        _containsChecker = containsChecker;
    }

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
        if (_areasIntersector.IsIntersects(line1Point1, line1Point2, line2Point1, line2Point2))
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

        for (LinkedListNode<CoordinateSupport>? currentInClipped = clipped.First!;
             currentInClipped != null;
             currentInClipped = flagWereIntersectionOnCurrentIteration ? currentInClipped : currentInClipped.Next)
        {
            for (LinkedListNode<CoordinateSupport>? currentInCutting = cutting.First!;
                 currentInCutting != null;
                 currentInCutting = flagWereIntersectionOnCurrentIteration ? currentInCutting : currentInCutting.Next)
            {
                flagWereIntersectionOnCurrentIteration = false;

                LinkedListNode<CoordinateSupport>? nextInClipped = currentInClipped.Next ?? clipped.First;
                LinkedListNode<CoordinateSupport>? nextInCutting = currentInCutting.Next ?? cutting.First;


                (LinesIntersectionType, Coordinate?) intersection = GetIntersection(currentInClipped.Value,
                    nextInClipped!.Value,
                    currentInCutting.Value, nextInCutting!.Value);

                if (intersection is { Item2: not null, Item1: LinesIntersectionType.Inner })
                {
                    ProcessInner(intersection!, currentInClipped, currentInCutting);
                }
                else if (intersection is { Item2: not null, Item1: LinesIntersectionType.TyShaped })
                {
                    ProcessTyShaped(intersection!, currentInClipped, currentInCutting, nextInClipped);
                }
                else if (intersection is { Item2: not null, Item1: LinesIntersectionType.Corner } or
                         { Item2: not null, Item1: LinesIntersectionType.Extension })
                {
                    ProcessCornerAndExtension(currentInClipped, currentInCutting, nextInClipped, nextInCutting);
                }
                else if (intersection is { Item1: LinesIntersectionType.Overlay })
                {
                    ProcessOverlay(currentInClipped, currentInCutting, nextInClipped, nextInCutting);
                }
                else if (intersection is { Item1: LinesIntersectionType.Part })
                {
                    ProcessPart(currentInClipped, currentInCutting, nextInClipped, nextInCutting);
                }
                else if (intersection is { Item1: LinesIntersectionType.Contains })
                {
                    ProcessContains(currentInClipped, currentInCutting, nextInClipped, nextInCutting);
                }
            }
        }

        return (flagWereIntersection, numberOfEnteringMarks, numberOfLeavingMarks);

        void ProcessInner((LinesIntersectionType, Coordinate) intersection,
            LinkedListNode<CoordinateSupport> currentInClipped,
            LinkedListNode<CoordinateSupport> currentInCutting)
        {
            LinkedListNode<CoordinateSupport> intersectionNodeInClip =
                new LinkedListNode<CoordinateSupport>(new CoordinateSupport(intersection.Item2));
            LinkedListNode<CoordinateSupport> intersectionNodeInCut =
                new LinkedListNode<CoordinateSupport>(new CoordinateSupport(intersection.Item2));

            clipped.AddAfter(currentInClipped, intersectionNodeInClip);
            cutting.AddAfter(currentInCutting, intersectionNodeInCut);
            flagWereIntersectionOnCurrentIteration = true;
        }

        void ProcessTyShaped((LinesIntersectionType, Coordinate) intersection,
            LinkedListNode<CoordinateSupport> currentInClipped,
            LinkedListNode<CoordinateSupport> currentInCutting, LinkedListNode<CoordinateSupport> nextInClipped)
        {
            flagWereIntersectionOnCurrentIteration = true;

            LinkedListNode<CoordinateSupport> intersectionNode =
                new LinkedListNode<CoordinateSupport>(new CoordinateSupport(intersection.Item2));

            if (_coordinateComparator.IsEquals(intersectionNode.Value, currentInClipped.Value))
            {
                cutting.AddAfter(currentInCutting, intersectionNode);
            }
            else if (_coordinateComparator.IsEquals(intersectionNode.Value, nextInClipped.Value))
            {
                cutting.AddAfter(currentInCutting, intersectionNode);
            }
            else if (_coordinateComparator.IsEquals(intersectionNode.Value, currentInCutting.Value))
            {
                clipped.AddAfter(currentInClipped, intersectionNode);
            }
            else
            {
                clipped.AddAfter(currentInClipped, intersectionNode);
            }
        }
        
        void ProcessCornerAndExtension(LinkedListNode<CoordinateSupport> currentInClipped,
            LinkedListNode<CoordinateSupport> currentInCutting, LinkedListNode<CoordinateSupport> nextInClipped,
            LinkedListNode<CoordinateSupport> nextInCutting)
        {
            LinkedListNode<CoordinateSupport> prevInClipped = currentInClipped.Previous ?? clipped.Last!;
            LinkedListNode<CoordinateSupport> prevInCutting = currentInCutting.Previous ?? cutting.Last!;
            LinkedListNode<CoordinateSupport> nextNextInClipped = nextInClipped.Next ?? clipped.First!;
            LinkedListNode<CoordinateSupport> nextNextInCutting = nextInCutting.Next ?? cutting.First!;
            
            if (_coordinateComparator.IsEquals(currentInClipped.Value, currentInCutting.Value))
            {
                if (_lineService.InsideTheAngleWithoutBorders(currentInClipped.Value, nextInClipped.Value,
                        nextInCutting.Value, currentInCutting.Value, prevInCutting.Value) &&
                    !_lineService.InsideTheAngleWithoutBorders(currentInClipped.Value, prevInClipped.Value,
                        nextInCutting.Value, currentInCutting.Value, prevInCutting.Value) &&
                    currentInClipped.Value.Type == PointType.Useless)
                {
                    flagWereIntersection = true;
                    numberOfEnteringMarks++;
                    currentInClipped.Value.Type = PointType.Entering;
                    currentInCutting.Value.Type = PointType.Entering;
                    currentInClipped.Value.Ref = currentInCutting;
                    currentInCutting.Value.Ref = currentInClipped;
                }

                else if (!_lineService.InsideTheAngleWithoutBorders(currentInClipped.Value, nextInClipped.Value,
                             nextInCutting.Value, currentInCutting.Value, prevInCutting.Value) &&
                         _lineService.InsideTheAngleWithoutBorders(currentInClipped.Value, prevInClipped.Value,
                             nextInCutting.Value, currentInCutting.Value, prevInCutting.Value) &&
                         currentInClipped.Value.Type == PointType.Useless)
                {
                    flagWereIntersection = true;
                    numberOfLeavingMarks++;
                    currentInClipped.Value.Type = PointType.Leaving;
                    currentInCutting.Value.Type = PointType.Leaving;
                    currentInClipped.Value.Ref = currentInCutting;
                    currentInCutting.Value.Ref = currentInClipped;
                }

                else if ((!_lineService.InsideTheAngle(currentInClipped.Value, nextInClipped.Value,
                              prevInClipped.Value, currentInCutting.Value, prevInCutting.Value)
                          
                          && _lineService.InsideTheAngleWithoutBorders(currentInClipped.Value, nextInCutting.Value,
                              prevInClipped.Value, currentInCutting.Value, prevInCutting.Value)
                          
                          
                          || _lineService.InsideTheAngleWithoutBorders(currentInClipped.Value, nextInClipped.Value,
                              prevInClipped.Value, currentInCutting.Value, prevInCutting.Value)
                          
                          && !_lineService.InsideTheAngle(currentInClipped.Value, nextInCutting.Value,
                              prevInClipped.Value, currentInCutting.Value, prevInCutting.Value))
                         
                         
                         && _lineService.InsideTheAngleWithoutBorders(currentInClipped.Value, nextInClipped.Value,
                             nextInCutting.Value, currentInCutting.Value, prevInCutting.Value)
                         
                         && _lineService.InsideTheAngleWithoutBorders(currentInClipped.Value, prevInClipped.Value,
                             nextInCutting.Value, currentInCutting.Value, prevInCutting.Value)
                         && currentInClipped.Value.Type == PointType.Useless)
                {
                    currentInClipped.Value.Type = PointType.SelfIntersection;
                    currentInCutting.Value.Type = PointType.SelfIntersection;
                    currentInClipped.Value.Ref = currentInCutting;
                    currentInCutting.Value.Ref = currentInClipped;
                    numberOfEnteringMarks++;
                    numberOfLeavingMarks++;
                }
            }

            if (_coordinateComparator.IsEquals(nextInClipped.Value, nextInCutting.Value))
            {
                if (_lineService.InsideTheAngleWithoutBorders(nextInClipped.Value, currentInClipped.Value,
                        nextNextInCutting.Value, nextInCutting.Value, currentInCutting.Value) &&
                    !_lineService.InsideTheAngleWithoutBorders(nextInClipped.Value, nextNextInClipped.Value,
                        nextNextInCutting.Value, nextInCutting.Value, currentInCutting.Value) &&
                    nextInClipped.Value.Type == PointType.Useless)
                {
                    flagWereIntersection = true;
                    numberOfLeavingMarks++;
                    nextInClipped.Value.Type = PointType.Leaving;
                    nextInCutting.Value.Type = PointType.Leaving;
                    nextInClipped.Value.Ref = nextInCutting;
                    nextInCutting.Value.Ref = nextInClipped;
                }

                else if (!_lineService.InsideTheAngleWithoutBorders(nextInClipped.Value, currentInClipped.Value,
                             nextNextInCutting.Value, nextInCutting.Value, currentInCutting.Value) &&
                         _lineService.InsideTheAngleWithoutBorders(nextInClipped.Value, nextNextInClipped.Value,
                             nextNextInCutting.Value, nextInCutting.Value, currentInCutting.Value) &&
                         nextInClipped.Value.Type == PointType.Useless)
                {
                    flagWereIntersection = true;
                    numberOfEnteringMarks++;
                    nextInClipped.Value.Type = PointType.Entering;
                    nextInCutting.Value.Type = PointType.Entering;
                    nextInClipped.Value.Ref = nextInCutting;
                    nextInCutting.Value.Ref = nextInClipped;
                }


                else if ((!_lineService.InsideTheAngle(nextInClipped.Value, nextNextInClipped.Value,
                              currentInClipped.Value, nextInClipped.Value, currentInCutting.Value)
                          && _lineService.InsideTheAngleWithoutBorders(nextInClipped.Value, nextNextInCutting.Value,
                              currentInClipped.Value, nextInClipped.Value, currentInCutting.Value)
                          || _lineService.InsideTheAngleWithoutBorders(nextInClipped.Value, nextNextInClipped.Value,
                              currentInClipped.Value, nextInClipped.Value, currentInCutting.Value)
                          && !_lineService.InsideTheAngle(nextInClipped.Value, nextNextInCutting.Value,
                              currentInClipped.Value, nextInClipped.Value, currentInCutting.Value))
                         && _lineService.InsideTheAngleWithoutBorders(nextInClipped.Value, nextNextInClipped.Value,
                             nextNextInCutting.Value, nextInCutting.Value, currentInCutting.Value)
                         && _lineService.InsideTheAngleWithoutBorders(nextInClipped.Value, currentInClipped.Value,
                             nextInCutting.Value, currentInCutting.Value, prevInCutting.Value)
                         && nextInClipped.Value.Type == PointType.Useless)
                {
                    nextInClipped.Value.Type = PointType.SelfIntersection;
                    nextInCutting.Value.Type = PointType.SelfIntersection;
                    nextInClipped.Value.Ref = nextInCutting;
                    nextInCutting.Value.Ref = nextInClipped;
                    numberOfEnteringMarks++;
                    numberOfLeavingMarks++;
                }
            }
        }

        void ProcessOverlay(LinkedListNode<CoordinateSupport> currentInClipped,
            LinkedListNode<CoordinateSupport> currentInCutting, LinkedListNode<CoordinateSupport> nextInClipped,
            LinkedListNode<CoordinateSupport> nextInCutting)
        {
            LinkedListNode<CoordinateSupport>? intersectionNodeInClip = null;
            LinkedListNode<CoordinateSupport>? intersectionNodeInCut = null;

            // Первый-четвёртый случаи
            if (_lineService.IsCoordinateInSegmentBorders(currentInCutting.Value, currentInClipped.Value,
                    nextInClipped.Value) &&
                _lineService.IsCoordinateInSegmentBorders(nextInClipped.Value, currentInCutting.Value,
                    nextInCutting.Value))
            {
                intersectionNodeInClip =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(currentInCutting.Value));
                intersectionNodeInCut =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(nextInClipped.Value));
            }
            else if (_lineService.IsCoordinateInSegmentBorders(nextInCutting.Value, currentInClipped.Value,
                         nextInClipped.Value) &&
                     _lineService.IsCoordinateInSegmentBorders(nextInClipped.Value, currentInCutting.Value,
                         nextInCutting.Value))
            {
                intersectionNodeInClip =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(nextInCutting.Value));
                intersectionNodeInCut =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(nextInClipped.Value));
            }
            else if (_lineService.IsCoordinateInSegmentBorders(currentInCutting.Value, currentInClipped.Value,
                         nextInClipped.Value) &&
                     _lineService.IsCoordinateInSegmentBorders(currentInClipped.Value, currentInCutting.Value,
                         nextInCutting.Value))
            {
                intersectionNodeInClip =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(currentInCutting.Value));
                intersectionNodeInCut =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(currentInClipped.Value));
            }
            else if (_lineService.IsCoordinateInSegmentBorders(nextInCutting.Value, currentInClipped.Value,
                         nextInClipped.Value) &&
                     _lineService.IsCoordinateInSegmentBorders(currentInClipped.Value, currentInCutting.Value,
                         nextInCutting.Value))
            {
                intersectionNodeInClip =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(nextInCutting.Value));
                intersectionNodeInCut =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(currentInClipped.Value));
            }

            // Добавление двух новых нод в листы
            clipped.AddAfter(currentInClipped, intersectionNodeInClip!);
            cutting.AddAfter(currentInCutting, intersectionNodeInCut!);

            flagWereIntersectionOnCurrentIteration = true;
        }

        void ProcessPart(LinkedListNode<CoordinateSupport> currentInClipped,
            LinkedListNode<CoordinateSupport> currentInCutting, LinkedListNode<CoordinateSupport> nextInClipped,
            LinkedListNode<CoordinateSupport> nextInCutting)
        {
            LinkedListNode<CoordinateSupport>? intersectionNodeInClip = null;
            LinkedListNode<CoordinateSupport>? intersectionNodeInCut = null;

            if (_lineService.IsCoordinateInIntervalBorders(currentInClipped.Value, currentInCutting.Value,
                    nextInCutting.Value))
            {
                intersectionNodeInCut =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(currentInClipped.Value));
            }
            else if (_lineService.IsCoordinateInIntervalBorders(nextInCutting.Value, currentInClipped.Value,
                         nextInClipped.Value))
            {
                intersectionNodeInClip =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(nextInCutting.Value));
            }
            else if (_lineService.IsCoordinateInIntervalBorders(nextInClipped.Value, currentInCutting.Value,
                         nextInCutting.Value))
            {
                intersectionNodeInCut =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(nextInClipped.Value));
            }
            else if (_lineService.IsCoordinateInIntervalBorders(currentInCutting.Value, currentInClipped.Value,
                         nextInClipped.Value))
            {
                intersectionNodeInClip =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(currentInCutting.Value));
            }

            if (intersectionNodeInClip != null)
            {
                clipped.AddAfter(currentInClipped, intersectionNodeInClip);
                flagWereIntersectionOnCurrentIteration = true;
            }

            if (intersectionNodeInCut != null)
            {
                cutting.AddAfter(currentInCutting, intersectionNodeInCut);
                flagWereIntersectionOnCurrentIteration = true;
            }
        }

        void ProcessContains(LinkedListNode<CoordinateSupport> currentInClipped,
            LinkedListNode<CoordinateSupport> currentInCutting, LinkedListNode<CoordinateSupport> nextInClipped,
            LinkedListNode<CoordinateSupport> nextInCutting)
        {
            LinkedListNode<CoordinateSupport>? intersectionNodeFirst;
            LinkedListNode<CoordinateSupport>? intersectionNodeSecond;

            if (_lineService.IsCoordinateInSegmentBorders(currentInClipped.Value, currentInCutting.Value,
                    nextInClipped.Value))
            {
                intersectionNodeFirst =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(currentInClipped.Value));
                intersectionNodeSecond =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(nextInClipped.Value));

                cutting.AddAfter(currentInCutting, intersectionNodeFirst);
                cutting.AddAfter(intersectionNodeFirst, intersectionNodeSecond);

                flagWereIntersectionOnCurrentIteration = true;
            }
            else if (_lineService.IsCoordinateInSegmentBorders(currentInClipped.Value, nextInCutting.Value,
                         nextInClipped.Value))
            {
                intersectionNodeFirst =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(nextInClipped.Value));
                intersectionNodeSecond =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(currentInClipped.Value));

                cutting.AddAfter(currentInCutting, intersectionNodeFirst);
                cutting.AddAfter(intersectionNodeFirst, intersectionNodeSecond);

                flagWereIntersectionOnCurrentIteration = true;
            }
            else if (_lineService.IsCoordinateInSegmentBorders(currentInCutting.Value, currentInClipped.Value,
                         nextInCutting.Value))
            {
                intersectionNodeFirst =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(currentInCutting.Value));
                intersectionNodeSecond =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(nextInCutting.Value));

                clipped.AddAfter(currentInClipped, intersectionNodeFirst);
                clipped.AddAfter(intersectionNodeFirst, intersectionNodeSecond);

                flagWereIntersectionOnCurrentIteration = true;
            }
            else if (_lineService.IsCoordinateInSegmentBorders(currentInCutting.Value, nextInClipped.Value,
                         nextInCutting.Value))
            {
                intersectionNodeFirst =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(nextInCutting.Value));
                intersectionNodeSecond =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(currentInCutting.Value));

                clipped.AddAfter(currentInClipped, intersectionNodeFirst);
                clipped.AddAfter(intersectionNodeFirst, intersectionNodeSecond);

                flagWereIntersectionOnCurrentIteration = true;
            }
        }
    }


    public Polygon[] WeilerAtherton(Polygon clippedPolygon, LinearRing cuttingRingShell)
    {
        LinearRing clippedRingShell = clippedPolygon.Shell;
        LinearRing[] clippedRingsHoles = clippedPolygon.Holes;

        // Проверка: внешняя оболочка должна обходиться по часовой стрелке, внутренние границы - против часовой

        EnsureClockwise();

        ConvertToCoordinateSupports(out LinkedList<CoordinateSupport>[] clippedListArray,
            out LinkedList<CoordinateSupport> cutting);

        cuttingRingShell.GetMinAndMaxOrdinates(
            out double cuttingMinX, out double cuttingMinY, out double cuttingMaxX, out double cuttingMaxY);

        List<LinearRing> maybeInnerRings = new List<LinearRing>();

        MakeNotesInAllRings(out int numberOfEnteringMarks);

        // Обход списков, формирование пересечений многоугольников

        List<IEnumerable<Coordinate>> result = new();

        for (LinkedListNode<CoordinateSupport>? nodeInCutting = cutting.First;
             nodeInCutting != null;
             nodeInCutting = nodeInCutting!.Next)
        {
            if (nodeInCutting.Value.Type != PointType.Entering &&
                nodeInCutting.Value.Type != PointType.SelfIntersection) continue;

            // SelfIntersection может трактоваться и как вход, и как выход. Ищем, какая будет после нее, и трактуем как противоположную
            if (nodeInCutting.Value.Type == PointType.SelfIntersection)
            {
                bool isEntering = true;
                LinkedListNode<CoordinateSupport>? temp;
                for (temp = nodeInCutting.Value.Ref!.Next;
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
                    LinkedListNode<CoordinateSupport> firstInRing = nodeInCutting!.Value.Ref!;
                    while (firstInRing.Previous != null)
                    {
                        firstInRing = firstInRing.Previous;
                    }

                    LinkedListNode<CoordinateSupport> endOfCycle = nodeInCutting.Value.Ref!;
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
                        isEntering = true;
                    }
                }

                if (!isEntering) continue;
            }

            List<Coordinate> figure = new(clippedRingShell.Count);

            LinkedListNode<CoordinateSupport>? startInCutting = nodeInCutting;
            LinkedListNode<CoordinateSupport>? startInClipped = nodeInCutting!.Value.Ref;

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
                            startInCutting = nodeFromEToLInClipped.Value.Ref;
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
                        startInCutting = nodeFromEToLInClipped.Next.Value.Ref;
                        nodeFromEToLInClipped.Next!.Value.Type = PointType.Useless;
                        startInCutting!.Value.Type = PointType.Useless;
                        break;
                    }

                    if (nodeFromEToLInClipped.Next!.Value.Type == PointType.SelfIntersection)
                    {
                        startInCutting = nodeFromEToLInClipped.Next.Value.Ref;
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
                            startInClipped = nodeFromLToEInCutting.Value.Ref;
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
                        startInClipped = nodeFromLToEInCutting.Next.Value.Ref;
                        startInClipped!.Value.Type = PointType.Useless;
                        nodeFromLToEInCutting.Next!.Value.Type = PointType.Useless;
                        break;
                    }

                    if (nodeFromLToEInCutting.Next!.Value.Type == PointType.SelfIntersection)
                    {
                        startInClipped = nodeFromLToEInCutting.Next.Value.Ref;
                        startInClipped!.Value.Type = PointType.Leaving;
                        nodeFromLToEInCutting.Next!.Value.Type = PointType.Leaving;
                        break;
                    }
                }
            } while (startInClipped != nodeInCutting.Value.Ref);

            figure.Add(figure.First());
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
                if (!_containsChecker.IsPointInLinearRing(coordinate, clippedRingShell, out bool isTangent))
                {
                    flagCuttingInClipped = false;
                    break;
                }

                if (!isTangent)
                {
                    break;
                }
            }

            if (flagCuttingInClipped)
            {
                return new Polygon[]
                {
                    new Polygon(new LinearRing(cuttingRingShell.Coordinates))
                };
            }

            bool flagClippedInCutting = true;
            foreach (CoordinateSupport coordinate in clippedListArray[0])
            {
                if (!_containsChecker.IsPointInLinearRing(coordinate, cuttingRingShell, out bool isTangent))
                {
                    flagClippedInCutting = false;
                    break;
                }

                if (!isTangent)
                {
                    break;
                }
            }

            return flagClippedInCutting
                ? new Polygon[]
                {
                    new Polygon(new LinearRing(clippedRingShell.Coordinates))
                }
                : Array.Empty<Polygon>();
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
                    foreach (Coordinate arrCoordinate in maybeInnerRing.Coordinates)
                    {
                        if (!_containsChecker.IsPointInLinearRing(arrCoordinate, ringShell, out bool isTangent))
                        {
                            isPolygonInPolygon = false;
                            break;
                        }

                        if (!isTangent)
                        {
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

        // result.Count == 0 && maybeInnerRings.Count != 0

        bool isCuttingInClipped = true;
        foreach (Coordinate t in cuttingRingShell.Coordinates)
        {
            if (!_containsChecker.IsPointInLinearRing(t, clippedRingShell, out bool isTangent))
            {
                isCuttingInClipped = false;
                break;
            }

            if (!isTangent)
            {
                break;
            }
        }

        if (isCuttingInClipped)
        {
            List<LinearRing> holes = new List<LinearRing>();
            foreach (var maybeInnerRing in maybeInnerRings)
            {
                bool isPolygonInCutting = true;
                foreach (Coordinate ringCoordinate in maybeInnerRing.Coordinates)
                {
                    if (!_containsChecker.IsPointInLinearRing(ringCoordinate, cuttingRingShell, out bool isTangent))
                    {
                        isPolygonInCutting = false;
                        break;
                    }

                    if (!isTangent)
                    {
                        break;
                    }
                }

                if (isPolygonInCutting)
                {
                    holes.Add(maybeInnerRing);
                }
            }

            return new[] { new Polygon(cuttingRingShell, holes.ToArray()) };
        }

        bool isClippedInCutting = true;
        foreach (Coordinate c in clippedRingShell.Coordinates)
        {
            if (!_containsChecker.IsPointInLinearRing(c, cuttingRingShell, out bool isTangent))
            {
                isClippedInCutting = false;
                break;
            }

            if (!isTangent)
            {
                break;
            }
        }

        if (isClippedInCutting)
        {
            return new[] { clippedPolygon };
        }

        return Array.Empty<Polygon>();

        bool IsIntersectsWithCuttingByEnvelope(double clippedMinY, double clippedMaxY, double clippedMaxX,
            double clippedMinX)
        {
            return clippedMinY <= cuttingMaxY && clippedMaxY >= cuttingMaxY &&
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
                   clippedMaxX <= cuttingMaxX && clippedMinX >= cuttingMinX;
        }

        void MakeNotesInAllRings(out int numberOfEnteringMarksInner)
        {
            numberOfEnteringMarksInner = 0;
            var (_, enteringCount, leavingCount) = MakeNotes(clippedListArray[0], cutting);

            if (enteringCount != leavingCount)
            {
                throw new DifferentNumbersOfPointTypes();
            }

            numberOfEnteringMarksInner += enteringCount;

            // Нужно определить, с какими дырами пересекается cutting,
            // потом пересечь каждую дыру по отдельности 
            for (int i = 0; i < clippedRingsHoles.Length; i++)
            {
                clippedRingsHoles[i].GetMinAndMaxOrdinates(
                    out double clippedMinX, out double clippedMinY, out double clippedMaxX, out double clippedMaxY);
                if (IsIntersectsWithCuttingByEnvelope(clippedMinY, clippedMaxY, clippedMaxX, clippedMinX))
                {
                    var (flagWereIntersection, e, l) = MakeNotes(clippedListArray[i + 1], cutting);

                    if (e != l)
                    {
                        throw new DifferentNumbersOfPointTypes();
                    }

                    numberOfEnteringMarksInner += e;

                    if (!flagWereIntersection)
                    {
                        maybeInnerRings.Add(clippedRingsHoles[i]);
                    }
                }
            }
        }

        void EnsureClockwise()
        {
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
        }

        void ConvertToCoordinateSupports(out LinkedList<CoordinateSupport>[] coordinateSupports,
            out LinkedList<CoordinateSupport> linkedList)
        {
            coordinateSupports = new LinkedList<CoordinateSupport>[clippedRingsHoles.Length + 1];

            coordinateSupports[0] = CoordinateToCoordinateSupport(clippedRingShell);
            for (int i = 0; i < clippedRingsHoles.Length; i++)
            {
                coordinateSupports[i + 1] = CoordinateToCoordinateSupport(clippedRingsHoles[i]);
            }

            linkedList = CoordinateToCoordinateSupport(cuttingRingShell);
        }
    }

    // ReSharper disable once UnusedMember.Local
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
                if (i.Value.Ref is not null)
                {
                    sw.Write(" ссылка на " + i.Value.Ref.Value + " ");
                }
                else
                {
                    sw.WriteLine();
                }

                if (i.Value.Ref is { Next: not null })
                {
                    sw.WriteLine("Value.Ref.Next = " + i.Value.Ref.Next.Value);
                }
                else if (i.Value.Type != PointType.Useless)
                {
                    sw.WriteLine("Value.Ref.Next = " + cutting.First!.Value);
                }
            }

            sw.WriteLine("\n\n\ncutting\n");
            for (LinkedListNode<CoordinateSupport>? i = cutting.First; i != null; i = i.Next)
            {
                sw.Write(i.Value + " " + i.Value.Type);
                if (i.Value.Ref is not null)
                {
                    sw.Write(" ссылка на " + i.Value.Ref.Value + " ");
                }
                else
                {
                    sw.WriteLine();
                }

                if (i.Value.Ref is { Next: not null })
                {
                    sw.WriteLine("Value.Ref.Next = " + i.Value.Ref.Next.Value);
                }
                else if (i.Value.Type != PointType.Useless)
                {
                    sw.WriteLine("Value.Ref.Next = " + clipped.First!.Value);
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