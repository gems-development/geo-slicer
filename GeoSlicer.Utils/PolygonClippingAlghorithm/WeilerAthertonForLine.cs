using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.PolygonClippingAlghorithm;

public class WeilerAthertonForLine
{
    private readonly LinesIntersector _linesIntersector;
    private readonly AreasIntersector _areasIntersector = new();
    private const AreasIntersectionType SuitableAreaAreaIntersectionType = AreasIntersectionType.Inside;
    private readonly LineService _lineService;
    private readonly ICoordinateComparator _coordinateComparator;
    private readonly ContainsChecker _containsChecker;
    private readonly double _epsilon;

    public WeilerAthertonForLine(LinesIntersector linesIntersector, LineService lineService,
        ICoordinateComparator coordinateComparator, ContainsChecker containsChecker, double epsilon)
    {
        _linesIntersector = linesIntersector;
        _lineService = lineService;
        _coordinateComparator = coordinateComparator;
        _containsChecker = containsChecker;
        _epsilon = epsilon;
    }

    // todo Переделать на массивы
    private LinkedList<CoordinateSupport> CoordinateToCoordinateSupport(LineString ring)
    {
        LinkedList<CoordinateSupport> result = new LinkedList<CoordinateSupport>();

        Coordinate[] coordinates = ring.Coordinates;

        foreach (Coordinate coord in coordinates)
        {
            result.AddLast(new CoordinateSupport(coord));
        }

        return result;
        // Вернёт LinkedList с координатами всех точек кольца без последней (равной первой)
    }

    public (LinesIntersectionType, Coordinate?) GetIntersection(CoordinateSupport line1Point1,
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


    private (int,int) AssistTyShapedMakeNotes(bool isShell, double vecProdNext, double vecProdPrev, 
        LinkedListNode<CoordinateSupport> existNode, LinkedListNode<CoordinateSupport> newNode)
    {
        int numberOfEnteringMarks = 0;
        int numberOfLeavingMarks = 0;
        
        if (existNode.Value.Type == PointType.Useless)
        {
            if (vecProdNext < 0 && vecProdPrev < 0)
            {
                existNode.Value.Type = PointType.SelfIntersection;
                newNode.Value.Type = PointType.SelfIntersection;
            }
            else if (vecProdNext < 0 && vecProdPrev > -_epsilon)
            {
                existNode.Value.Type = PointType.Entering;
                newNode.Value.Type = PointType.Entering;

                numberOfEnteringMarks++;
            }
            else if (vecProdNext > 0)
            {
                if (isShell)
                {
                    if (vecProdPrev < _epsilon)
                    {
                        existNode.Value.Type = PointType.Leaving;
                        newNode.Value.Type = PointType.Leaving;

                        numberOfLeavingMarks++;
                    }
                }
                else
                {
                    if (vecProdPrev < 0)
                    {
                        existNode.Value.Type = PointType.Leaving;
                        newNode.Value.Type = PointType.Leaving;

                        numberOfLeavingMarks++;
                    }
                }
            }
        }
        
        existNode.Value.Coord = newNode;
        newNode.Value.Coord = existNode;

        return (numberOfEnteringMarks, numberOfLeavingMarks);
    }
    
    
    // Функция расстановки меток и создания ссылок между списками
    private (bool, int, int) MakeNotes(
        LinkedList<CoordinateSupport> clipped, LinkedList<CoordinateSupport> cutting, bool isShell)
    {
        int numberOfEnteringMarks = 0;
        int numberOfLeavingMarks = 0;

        bool flagWereIntersection = false;

        LinkedListNode<CoordinateSupport> firstPointInCutting = cutting.First!;
        LinkedListNode<CoordinateSupport> lastPointInCutting = cutting.Last!;
        LinkedListNode<CoordinateSupport> currentPointInCutting = cutting.First!;

        for (LinkedListNode<CoordinateSupport>? currentPointInClipped = clipped.First!; 
             currentPointInClipped != null; 
             currentPointInClipped = currentPointInClipped.Next)
        {
            LinkedListNode<CoordinateSupport> nextPointInClipped = clipped.First!;
            LinkedListNode<CoordinateSupport> prevPointInClipped = clipped.Last!;

            if (currentPointInClipped.Next != null)
            {
                nextPointInClipped = currentPointInClipped.Next;
            }

            if (currentPointInClipped.Previous != null)
            {
                prevPointInClipped = currentPointInClipped.Previous;
            }

            (LinesIntersectionType, Coordinate?) intersection = GetIntersection(
                currentPointInClipped.Value, nextPointInClipped!.Value,
                firstPointInCutting.Value, lastPointInCutting.Value);
            
            double vecProdNext = LineService.VectorProduct(
                firstPointInCutting.Value, lastPointInCutting.Value,
                currentPointInClipped.Value, nextPointInClipped.Value);

            double vecProdPrev = LineService.VectorProduct(
                firstPointInCutting.Value, lastPointInCutting.Value,
                currentPointInClipped.Value, prevPointInClipped.Value);

            // Inner

            if (intersection is { Item2: not null, Item1: LinesIntersectionType.Inner })
            {
                LinkedListNode<CoordinateSupport> intersectionNodeInClip =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(intersection.Item2));
                LinkedListNode<CoordinateSupport> intersectionNodeInCut =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(intersection.Item2));

                clipped.AddAfter(currentPointInClipped, intersectionNodeInClip);
                cutting.AddAfter(currentPointInCutting, intersectionNodeInCut);
                currentPointInCutting = intersectionNodeInCut;
                
                if (vecProdNext > 0 == isShell)
                {
                    intersectionNodeInClip.Value.Type = PointType.Entering;
                    intersectionNodeInCut.Value.Type = PointType.Entering;

                    numberOfEnteringMarks++;
                }
                else if (vecProdNext < 0 == isShell)
                {
                    intersectionNodeInClip.Value.Type = PointType.Leaving;
                    intersectionNodeInCut.Value.Type = PointType.Leaving;

                    numberOfLeavingMarks++;
                }

                intersectionNodeInClip.Value.Coord = intersectionNodeInCut;
                intersectionNodeInCut.Value.Coord = intersectionNodeInClip;

                flagWereIntersection = true;
            }

            // TyShaped
            //для внешней оболочки и для дыр метки ставятся одинаково

            else if (intersection is { Item2: not null, Item1: LinesIntersectionType.TyShaped })
            {
                LinkedListNode<CoordinateSupport> intersectionNode =
                    new LinkedListNode<CoordinateSupport>(new CoordinateSupport(intersection.Item2));

                int e = 0; 
                int l = 0;
                
                if (_coordinateComparator.IsEquals(intersection.Item2, currentPointInClipped.Value))
                {
                    cutting.AddAfter(currentPointInCutting, intersectionNode);
                    currentPointInCutting = intersectionNode;
                    
                    (e, l) = AssistTyShapedMakeNotes(isShell, vecProdNext, vecProdPrev, currentPointInClipped, intersectionNode);
                    
                    flagWereIntersection = true;
                }
                else if (_coordinateComparator.IsEquals(intersection.Item2, firstPointInCutting.Value))
                {
                    clipped.AddAfter(currentPointInClipped, intersectionNode);
                    
                    (e, l) = AssistTyShapedMakeNotes(isShell, vecProdNext, vecProdPrev, firstPointInCutting, intersectionNode);
                    
                    flagWereIntersection = true;
                }
                else if (_coordinateComparator.IsEquals(intersection.Item2, lastPointInCutting.Value))
                {
                    clipped.AddAfter(currentPointInClipped, intersectionNode);
                    
                    (e, l) = AssistTyShapedMakeNotes(isShell, vecProdNext, vecProdPrev, lastPointInCutting, intersectionNode);
                    
                    flagWereIntersection = true;
                }
                numberOfEnteringMarks += e;
                numberOfLeavingMarks += l;
            }

            //Part
            //возможен только для внешней оболочки
            else if (intersection is { Item1: LinesIntersectionType.Part })
            {
                if (isShell)
                {
                    if (!(_coordinateComparator.IsEquals(currentPointInClipped.Value, firstPointInCutting.Value) ||
                          _coordinateComparator.IsEquals(currentPointInClipped.Value, lastPointInCutting.Value)) &&
                        currentPointInClipped.Value.Type == PointType.Useless)
                    {
                        LinkedListNode<CoordinateSupport> intersectionNodeInCut =
                            new LinkedListNode<CoordinateSupport>(new CoordinateSupport(currentPointInClipped.Value));

                        if (vecProdPrev < 0)
                        {
                            currentPointInClipped.Value.Type = PointType.Leaving;
                            intersectionNodeInCut.Value.Type = PointType.Leaving;

                            numberOfLeavingMarks++;
                        }
                        else if (vecProdPrev > 0)
                        {
                            currentPointInClipped.Value.Type = PointType.Entering;
                            intersectionNodeInCut.Value.Type = PointType.Entering;

                            numberOfEnteringMarks++;
                        }
                    
                        cutting.AddAfter(currentPointInCutting, intersectionNodeInCut);
                        currentPointInCutting = intersectionNodeInCut;

                        currentPointInClipped.Value.Coord = intersectionNodeInCut;
                        intersectionNodeInCut.Value.Coord = currentPointInClipped;
                        
                        flagWereIntersection = true;
                    }
                }
            }
            
            // Contains
            //для внешней оболочки и для дыр метки ставятся одинаково

            else if (intersection is { Item1: LinesIntersectionType.Contains })
            {
                //вставляем только одну точку, потому что другая вставится при TyShaped
                //вставляем только тогда, когда на ней ставится метка
                if (vecProdPrev < 0 && currentPointInClipped.Value.Type == PointType.Useless)
                {
                    LinkedListNode<CoordinateSupport> intersectionNodeFirst =
                                     new LinkedListNode<CoordinateSupport>(new CoordinateSupport(currentPointInClipped.Value));
                    intersectionNodeFirst.Value.Type = PointType.Leaving;
                    currentPointInClipped.Value.Type = PointType.Leaving;
                    
                    cutting.AddAfter(currentPointInCutting, intersectionNodeFirst);
                    
                    currentPointInClipped.Value.Coord = intersectionNodeFirst;
                    intersectionNodeFirst.Value.Coord = currentPointInClipped;

                    currentPointInCutting = intersectionNodeFirst;

                    numberOfLeavingMarks++;
                    
                    flagWereIntersection = true;
                }
            }
            
            // Corner & Extension

            else if (intersection is { Item2: not null, Item1: LinesIntersectionType.Corner } or
                     { Item2: not null, Item1: LinesIntersectionType.Extension })
            {
                if (isShell)
                {
                    if (_coordinateComparator.IsEquals(firstPointInCutting.Value, currentPointInClipped.Value) &&
                        _lineService.InsideTheAngleWithoutBorders(firstPointInCutting.Value, lastPointInCutting.Value,
                            prevPointInClipped.Value, currentPointInClipped.Value, nextPointInClipped.Value) &&
                        firstPointInCutting.Value.Type == PointType.Useless) 
                    {
                        firstPointInCutting.Value.Type = PointType.Leaving;

                        firstPointInCutting.Value.Coord = currentPointInClipped;
                        currentPointInClipped.Value.Coord = firstPointInCutting;

                        numberOfLeavingMarks++;
                        
                        flagWereIntersection = true;
                    }
                    else if (_coordinateComparator.IsEquals(lastPointInCutting.Value, currentPointInClipped.Value) &&
                             _lineService.InsideTheAngleWithoutBorders(lastPointInCutting.Value, firstPointInCutting.Value,
                                 prevPointInClipped.Value, currentPointInClipped.Value, nextPointInClipped.Value) &&
                             lastPointInCutting.Value.Type == PointType.Useless)
                    {
                        lastPointInCutting.Value.Type = PointType.Entering;

                        lastPointInCutting.Value.Coord = currentPointInClipped;
                        currentPointInClipped.Value.Coord = lastPointInCutting;

                        numberOfEnteringMarks++;
                        
                        flagWereIntersection = true;
                    }
                }
            }
        }

        return (flagWereIntersection, numberOfEnteringMarks, numberOfLeavingMarks);
    }


    public Polygon[] WeilerAtherton(Polygon clippedPolygon, LineString cuttingRingShell)
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

        // numberOfLeavingMarks может понадобиться для отладки: должно быть равно numberOfEnteringMarks

        var (cuttingMinX, cuttingMinY, cuttingMaxX, cuttingMaxY) = cuttingRingShell.GetMinAndMaxPoints();

        List<LinearRing> maybeInnerRings = new List<LinearRing>();

        for (int i = 0; i < clippedListArray.Length; i++)
        {
            var (clippedMinX, clippedMinY, clippedMaxX, clippedMaxY) = allRingsClipped[i].GetMinAndMaxPoints();
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
                clippedMinY >= cuttingMinY && clippedMaxY < cuttingMaxY &&
                clippedMinX <= cuttingMaxX && clippedMaxX > cuttingMaxX ||
                clippedMinY >= cuttingMinY && clippedMaxY <= cuttingMaxY &&
                clippedMinX <= cuttingMinX && clippedMaxX >= cuttingMinX ||
                clippedMaxY <= cuttingMaxY && clippedMinY >= cuttingMinY &&
                clippedMaxX <= cuttingMaxX && clippedMinX >= cuttingMinX)
            {
                var tuple = MakeNotes(clippedListArray[i], cutting, i == 0);

                PrintMarks(clippedListArray[i], cutting, "Bad" + i + ".txt.ignore");

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

            // SelfIntersection может трактоваться и как вход, и как выход. Ищем, какая будет после нее, и трактуем как противоположную
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
                     nodeFromLToEInCutting!.Value.Type != PointType.SelfIntersection || count == 0;
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

                    foreach (var arrCoordinates in maybeInnerRing.Coordinates)
                    {
                        if (!_containsChecker.IsPointInLinearRing(arrCoordinates, ringShell))
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