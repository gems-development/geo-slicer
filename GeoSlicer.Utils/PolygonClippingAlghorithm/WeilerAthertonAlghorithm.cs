﻿using System;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.PolygonClippingAlghorithm;

public class WeilerAthertonAlghorithm
{
    private readonly LinesIntersector _linesIntersector;
    private readonly LineService _lineService;
    private readonly ICoordinateComparator _coordinateComparator;
    private readonly ContainsChecker _containsChecker;

    public WeilerAthertonAlghorithm(LinesIntersector linesIntersector, LineService lineService,
        ICoordinateComparator coordinateComparator, ContainsChecker containsChecker)
    {
        _linesIntersector = linesIntersector;
        _lineService = lineService;
        _coordinateComparator = coordinateComparator;
        _containsChecker = containsChecker;
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

    public IEnumerable<LinearRing> WeilerAthertonStub(LinearRing clipped, LinearRing cutting)
    {
        var intersection = new Polygon(clipped).Intersection(new Polygon(cutting));

        if (intersection is Polygon polygon)
        {
            return new[] { polygon.Shell };
        }

        if (intersection is MultiPolygon multiPolygon)
        {
            return multiPolygon.Select(geometry => ((Polygon)geometry).Shell);
        }

        if (intersection is GeometryCollection geometryCollection)
        {
            LinkedList<LinearRing> res = new LinkedList<LinearRing>();
            foreach (Geometry geometry in geometryCollection)
            {
                if (geometry is Polygon pol)
                {
                    res.AddLast(pol.Shell);
                }
                else if (geometry is Point || geometry is LineString)
                {
                    
                }
                else
                {
                    GeoJsonFileService.WriteGeometryToFile(clipped, "OutData/clp.geojson.ignore");
                    GeoJsonFileService.WriteGeometryToFile(cutting, "OutData/ctt.geojson.ignore");
                    GeoJsonFileService.WriteGeometryToFile(intersection, "OutData/res.geojson.ignore");
                    GeoJsonFileService.WriteGeometryToFile(geometry, "OutData/geom.geojson.ignore");
                    throw new NotImplementedException(
                        "Пойман нерассмотренный вариант типа вложнной геометрии, возвращаемого 'Intersection'");
                }
            }

            return res;
        }
        throw new NotImplementedException("Пойман нерассмотренный вариант типа, возвращаемого 'Intersection'");
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

    

    //На вход передаются координаты колец
    public IEnumerable<LinearRing> WeilerAtherton(
        LinearRing clippedCoordinates, LinearRing cuttingCoordinates)
    {
        int numberOfEnteringMarks = 0;
        int numberOfLivingMarks = 0;

        //нужно, чтобы обход clipped и cutting был по часовой

        if (!TraverseDirection.IsClockwiseBypass(clippedCoordinates))
        {
            TraverseDirection.ChangeDirection(clippedCoordinates);
        }

        if (!TraverseDirection.IsClockwiseBypass(cuttingCoordinates))
        {
            TraverseDirection.ChangeDirection(cuttingCoordinates);
        }

        LinkedList<CoordinateSupport> clipped = CoordinateToCoordinateSupport(clippedCoordinates);
        LinkedList<CoordinateSupport> cutting = CoordinateToCoordinateSupport(cuttingCoordinates);

        bool flagWereIntersection = false;
        bool flagWereIntersectionOnCurrentIteration = false;

        // Создание двух списков с помеченными точками

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
                            numberOne.Value.Type = PointType.Living;
                            numberThree.Value.Type = PointType.Living;
                            numberOne.Value.Coord = numberThree;
                            numberThree.Value.Coord = numberOne;
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
                            numberTwo.Value.Type = PointType.Living;
                            numberFour.Value.Type = PointType.Living;
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

        if (!flagWereIntersection)
        {
            bool flagCuttingInClipped = true;
            foreach (CoordinateSupport coordinate in cutting)
            {
                if (!_containsChecker.IsPointInLinearRing(coordinate, clippedCoordinates))
                {
                    flagCuttingInClipped = false;
                    break;
                }
            }

            bool flagClippedInCutting = true;
            foreach (CoordinateSupport coordinate in clipped)
            {
                if (!_containsChecker.IsPointInLinearRing(coordinate, cuttingCoordinates))
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

        //обход списков, формирование пересечений многоугольников

        List<IEnumerable<Coordinate>> result = new();

        for (LinkedListNode<CoordinateSupport>? nodeInClipped = clipped.First;
             nodeInClipped != null;
             nodeInClipped = nodeInClipped.Next)
        {
            if (nodeInClipped.Value.Type != PointType.Entering) continue;
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
                }
            } while (startInClipped != nodeInClipped);

            figure.Add(nodeInClipped.Value);
            result.Add(figure);

            if (numberOfEnteringMarks == 0)
            {
                break;
            }
        }


        return result.Select(enumerable => new LinearRing(enumerable.ToArray()));
    }
}