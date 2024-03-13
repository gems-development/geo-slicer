using System.Collections.Generic;
using GeoSlicer.NonConvexSlicer.Helpers;
using GeoSlicer.Utils;
using NetTopologySuite.Geometries;

namespace GeoSlicer.NonConvexSlicer;

public class NonConvexSlicer
{
    private readonly GeometryFactory _gf;
    private readonly SegmentService _segmentService;
    private readonly LineService _lineService;
    private readonly NonConvexSlicerHelper _helper;
    private readonly TraverseDirection _traverseDirection;

    public NonConvexSlicer(
        GeometryFactory gf,
        SegmentService segmentService,
        NonConvexSlicerHelper helper,
        TraverseDirection traverseDirection, LineService lineService)
    {
        _gf = gf;
        _segmentService = segmentService;
        _helper = helper;
        _traverseDirection = traverseDirection;
        _lineService = lineService;
    }

    
    // todo убедиться в рациональности
    private List<LinearRing> SimpleSlice(LinearRing ring, int pozSpecialPoint)
    {
        var listResult = new List<LinearRing>(ring.Count -2);
        var coordinates = ring.Coordinates;
        var i = (pozSpecialPoint + 1) % (ring.Count - 1);
        var count = 0;
        while (count < ring.Count - 1 - 2)
        {
            var array = new Coordinate[4];
            array[0] = coordinates[pozSpecialPoint];
            array[1] = coordinates[(i + ring.Count - 1) % (ring.Count - 1)];
            array[2] = coordinates[(i + 1 + ring.Count - 1) % (ring.Count - 1)];
            array[3] = coordinates[pozSpecialPoint];

            listResult.Add(new LinearRing(array));
            count++;
            i = (i + 1) % (ring.Count - 1);
        }

        return listResult;
    }


    public List<LinearRing> SliceFigureWithMinNumberOfSpecialPoints(LinearRing ring, Coordinate firstPoint, Coordinate? secondPoint = null)
    {
        var newRing = _segmentService.IgnoreInnerPointsOfSegment(ring);
        var newRingCoordinates = newRing.Coordinates;


        var firstPointIndex = newRing.GetIndex(firstPoint) ?? -1;
        var firstPointIsSpecial = firstPointIndex != -1 && _helper.CurrentPointIsSpecial(
            newRingCoordinates[(firstPointIndex - 1 + newRingCoordinates.Length - 1) % (newRingCoordinates.Length - 1)],
            firstPoint, newRingCoordinates[(firstPointIndex + 1) % (newRingCoordinates.Length - 1)]);


        int secondPointIndex = -1;
        bool secondPointIsSpecial = false;
        
        
        if (secondPoint != null)
        {
            secondPointIndex = newRing.GetIndex(secondPoint) ?? -1;
            secondPointIsSpecial = _helper.CurrentPointIsSpecial(newRingCoordinates[(secondPointIndex - 1 + newRingCoordinates.Length - 1) % (newRingCoordinates.Length - 1)],
                secondPoint, newRingCoordinates[(secondPointIndex + 1) % (newRingCoordinates.Length - 1)]);
        }
        var listRingsWithoutSpecialPoints = new List<LinearRing>(newRingCoordinates.Length - 4);

        int specialPointIndex;
        Coordinate specialPoint;
        
        if (firstPointIsSpecial && firstPointIndex != -1)
        {
            specialPointIndex = firstPointIndex;
            specialPoint = firstPoint;
        }
        else if (secondPointIsSpecial && secondPointIndex != -1 && secondPoint != null)
        {
            specialPointIndex = secondPointIndex;
            specialPoint = secondPoint;
        }
        else
        {
            listRingsWithoutSpecialPoints.Add(newRing);
            return listRingsWithoutSpecialPoints;
        }

        var coordC = newRingCoordinates[(specialPointIndex + 1) % (newRingCoordinates.Length - 1)];

        var pozNextPoint = (specialPointIndex + 2) % (newRingCoordinates.Length - 1);

        var flag = true;
        var k = 0;

        while (flag)
        {
            var coordM = newRingCoordinates[pozNextPoint];

            if (!_helper.CurrentPointIsSpecial(coordC, specialPoint, coordM))
            {
                //NextPoint не является особой точкой в новом кольце
                //и не лежит на одной прямой со старой особой точкой и предыдущей для старой особой
                pozNextPoint = (pozNextPoint - 1 + newRingCoordinates.Length - 1) % (newRingCoordinates.Length - 1);
                coordM = newRingCoordinates[pozNextPoint];
                
                // todo Вычислить заранее количество точек (и ниже) (и убедиться, что вычисленно верно)
                var listFirst = new List<Coordinate>();

                for (var i = specialPointIndex; i != pozNextPoint; i = (i + 1) % (newRingCoordinates.Length - 1))
                {
                    listFirst.Add(newRingCoordinates[i]);
                }

                listFirst.Add(coordM);
                listFirst.Add(specialPoint);

                var listSecond = new List<Coordinate>();

                for (var i = pozNextPoint; i != specialPointIndex; i = (i + 1) % (newRingCoordinates.Length - 1))
                {
                    listSecond.Add(newRingCoordinates[i]);
                }

                listSecond.Add(specialPoint);
                listSecond.Add(coordM);


                var ringFirst = _segmentService.IgnoreInnerPointsOfSegment(new LinearRing(listFirst.ToArray()));
                var ringSecond = _segmentService.IgnoreInnerPointsOfSegment(new LinearRing(listSecond.ToArray()));

                listRingsWithoutSpecialPoints.Add(ringFirst);

                flag = false;

                var listRec = SliceFigureWithMinNumberOfSpecialPoints(ringSecond, firstPoint, secondPoint);

                listRingsWithoutSpecialPoints.AddRange(listRec);
            }
            else
            {
                pozNextPoint = (pozNextPoint + 1) % (newRingCoordinates.Length - 1);

                k++;

                if (k == newRingCoordinates.Length - 1)
                {
                    return SimpleSlice(newRing, specialPointIndex);
                }
            }
        }

        return listRingsWithoutSpecialPoints;
    }

    public List<LinearRing> Slice(LinearRing ring)
    {
        if (!_traverseDirection.IsClockwiseBypass(ring)) TraverseDirection.ChangeDirection(ring);
        //Список особых точек
        var listSpecialPoints = _helper.GetSpecialPoints(ring);
        var ringCoords = new CoordinatePcn[ring.Count - 1];
        for (var i = 0; i < ring.Count - 1; ++i)
        {
            ringCoords[i] = new CoordinatePcn(ring.Coordinates[i].X, ring.Coordinates[i].Y,
                (i - 2 + ring.Count) % (ring.Count - 1),
                i, (i + 1) % (ring.Count - 1));
        }

        //Список LinearRing для ответа
        var listLinearRing = new List<LinearRing>(listSpecialPoints.Count);

        switch (listSpecialPoints.Count)
        {
            case 0:
                return new List<LinearRing> { ring };
            case 1:
                return SliceFigureWithMinNumberOfSpecialPoints(ring, listSpecialPoints[0]);
        }

        //coordCurrent, coordNext - координаты текущей и следующей точки, которые мы хотим соединить
        var coordCurrent = ringCoords[listSpecialPoints[0].C];
        var coordPrev = coordCurrent;
        //Индексы начала и конца элементов списка особых точек, которые входят в одно кольцо в итерации
        var beginSpecialPointIndex = 0;
        var endSpecialPointIndex = 0;
        //Индексы точек, с которыми мы соединили начальную точку: beforeFirstIndex -> firstPoint -> afterFirstIndex
        var afterFirstIndex = 0;
        var beforeFirstIndex = 0;
        bool wasIntersectionInIteration;


        for (int i = 0, currentSpecialPointIndex = 0;
             currentSpecialPointIndex < listSpecialPoints.Count;
             currentSpecialPointIndex += wasIntersectionInIteration ? 0 : 1)
        {
            if (currentSpecialPointIndex == endSpecialPointIndex && endSpecialPointIndex != listSpecialPoints.Count)
            {
                i = 0;
                beginSpecialPointIndex = endSpecialPointIndex;
                endSpecialPointIndex = listSpecialPoints.Count;
                coordCurrent = ringCoords[listSpecialPoints[beginSpecialPointIndex].C];
                coordPrev = coordCurrent;
                //Замена коориднат кольца старой итерации на новое
                for (var j = coordCurrent.C; j != coordCurrent.P;)
                {
                    var coordIter = ringCoords[j];
                    coordIter.Pl = coordIter.P;
                    coordIter.Nl = coordIter.N;
                    j = coordIter.N;
                }

                ringCoords[coordCurrent.P].Pl = ringCoords[coordCurrent.P].P;
                ringCoords[coordCurrent.P].Nl = ringCoords[coordCurrent.P].N;
            }

            var coordNext = ringCoords[listSpecialPoints[
                (currentSpecialPointIndex + 1 - beginSpecialPointIndex) %
                (endSpecialPointIndex - beginSpecialPointIndex) + beginSpecialPointIndex].C];

            wasIntersectionInIteration = false;
            if (!_helper.CanSeeEachOther(ringCoords, coordCurrent, coordNext) ||
                _helper.HasIntersection(ringCoords, coordCurrent, coordNext))
            {
                var nextIndex = coordNext.P;
                while (!_helper.CanSeeEachOther(ringCoords, ringCoords[coordCurrent.C],
                           ringCoords[nextIndex]) ||
                       _helper.HasIntersection(ringCoords, coordCurrent, ringCoords[nextIndex]))
                {
                    nextIndex = ringCoords[nextIndex].P;
                }

                wasIntersectionInIteration = true;
                coordNext = ringCoords[nextIndex];
            }
            if (coordPrev.C == listSpecialPoints[beginSpecialPointIndex].C &&
                coordCurrent.C != listSpecialPoints[beginSpecialPointIndex].C)
            {
                afterFirstIndex = coordCurrent.C;
            }

            if (coordNext.C == listSpecialPoints[beginSpecialPointIndex].C)
            {
                beforeFirstIndex = coordCurrent.C;
            }

            if (currentSpecialPointIndex >= beginSpecialPointIndex + 1 &&
                currentSpecialPointIndex <= endSpecialPointIndex - 1)
            {
                //Если особая точка будет особой в получившемся кольце, то добавляем с конец списка особых точек.
                //При этом кольцо не двуугольник
                if (afterFirstIndex != beforeFirstIndex &&
                    _lineService.VectorProduct(
                        ringCoords[coordCurrent.C].X - ringCoords[coordPrev.C].X, 
                        ringCoords[coordCurrent.C].Y - ringCoords[coordPrev.C].Y, 
                        ringCoords[coordNext.C].X - ringCoords[coordCurrent.C].X, 
                        ringCoords[coordNext.C].Y - ringCoords[coordCurrent.C].Y
                    ) >= 0)
                {
                    listSpecialPoints.Add(coordCurrent);
                }
            }

            if (coordNext.C == listSpecialPoints[beginSpecialPointIndex].C)
            {
                //Добавляем начальную точку, если она особая в получившемся кольце и не является единственной в нём
                //При этом кольцо не двуугольник
                if (coordNext.C != afterFirstIndex &&
                    coordNext.C != beforeFirstIndex &&
                    afterFirstIndex != beforeFirstIndex &&
                    _lineService.VectorProduct(
                        ringCoords[coordNext.C].X - ringCoords[beforeFirstIndex].X,
                        ringCoords[coordNext.C].Y - ringCoords[beforeFirstIndex].Y,
                        ringCoords[afterFirstIndex].X - ringCoords[coordNext.C].X,
                        ringCoords[afterFirstIndex].Y - ringCoords[coordNext.C].Y
                    ) >= 0)
                {
                    listSpecialPoints.Add(coordNext);
                }
            }

            if (coordNext.C != coordCurrent.Nl)
            {
                var currentLinearRingCoords = new List<Coordinate>();
                for (var j = coordCurrent.C; j != (coordCurrent.C == coordNext.C ? coordCurrent.P : coordNext.C);)
                {
                    var coordIter = ringCoords[j];
                    currentLinearRingCoords.Add(new Coordinate(ringCoords[j % ringCoords.Length].X,
                        ringCoords[j % ringCoords.Length].Y));
                    j = coordIter.N;
                }

                currentLinearRingCoords.Add(coordCurrent.C == coordNext.C
                    ? ringCoords[coordCurrent.P]
                    : ringCoords[coordNext.C]);
                currentLinearRingCoords.Add(currentLinearRingCoords[0]);
                var currentLinearRing = _gf.CreateLinearRing(currentLinearRingCoords.ToArray());
                var convexLists = SliceFigureWithMinNumberOfSpecialPoints(currentLinearRing, coordCurrent, coordNext);
                listLinearRing.AddRange(convexLists);
            }

            i++;
            coordCurrent.N = coordNext.C;
            coordNext.P = coordCurrent.C;
            coordPrev = coordCurrent;
            coordCurrent = coordNext;
            /*Если на текущей итерации мы дошли до конца списка особых точек, при этом
             * новых точек не добавилось, и количество особых точек текущеё итерации > 2,
             * то создаём из всех особых точек текущей итерации LinearRing и добавляем его в listLinearRing
             */
            if (i > 2 && endSpecialPointIndex == listSpecialPoints.Count &&
                currentSpecialPointIndex == endSpecialPointIndex - 1)
            {
                var lastLinearRingCoords = new Coordinate[i + 1];
                var t = 0;
                var j = listSpecialPoints[beginSpecialPointIndex].C;
                do
                {
                    var coordIter = ringCoords[j];
                    lastLinearRingCoords[t] = new Coordinate(ringCoords[j % ringCoords.Length].X,
                        ringCoords[j % ringCoords.Length].Y);
                    j = coordIter.N;
                    ++t;
                } while (j != listSpecialPoints[beginSpecialPointIndex].C);

                lastLinearRingCoords[i] = lastLinearRingCoords[0];
                var lastLinearRing = _gf.CreateLinearRing(lastLinearRingCoords);
                listLinearRing.Add(lastLinearRing);
            }
        }

        return listLinearRing;
    }
}