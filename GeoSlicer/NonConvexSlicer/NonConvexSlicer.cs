using NetTopologySuite;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;


namespace GeoSlicer.NonConvexSlicer;

public class NonConvexSlicer
{
    private readonly GeometryFactory _gf;
    private readonly bool _clockwise;

    public NonConvexSlicer(bool clockwise)
    {
        _gf = NtsGeometryServices.Instance.CreateGeometryFactory(4326);
        _clockwise = clockwise;
    }

    public List<CoordinateM> GetSpecialPoints(LinearRing ring)
    {
        var list = new List<CoordinateM>();
        for (var i = 0; i < ring.Coordinates.Length - 1; ++i)
        {
            if (SegmentService.VectorProduct(
                    new Coordinate(
                        ring.Coordinates[i].X -
                        ring.Coordinates[(i - 1 + ring.Coordinates.Length - 1) % (ring.Coordinates.Length - 1)].X,
                        ring.Coordinates[i].Y -
                        ring.Coordinates[(i - 1 + ring.Coordinates.Length - 1) % (ring.Coordinates.Length - 1)].Y),
                    new Coordinate(ring.Coordinates[(i + 1) % (ring.Coordinates.Length - 1)].X - ring.Coordinates[i].X,
                        ring.Coordinates[(i + 1) % (ring.Coordinates.Length - 1)].Y - ring.Coordinates[i].Y)
                ) >= 0 == _clockwise)
            {
                list.Add(new CoordinateM(ring.Coordinates[i].X, ring.Coordinates[i].Y, i));
            }
        }

        return list;
    }


    private List<LinearRing> SimpleSlice(LinearRing ring, int pozSpecialPoint)
    {
        var listResult = new List<LinearRing>();

        var i = (pozSpecialPoint + 1) % (ring.Count - 1);
        var count = 0;
        while (count < ring.Count - 1 - 2)
        {
            var array = new Coordinate[4];
            array[0] = ring.Coordinates[pozSpecialPoint];
            array[1] = ring.Coordinates[(i + ring.Count - 1) % (ring.Count - 1)];
            array[2] = ring.Coordinates[(i + 1 + ring.Count - 1) % (ring.Count - 1)];
            array[3] = ring.Coordinates[pozSpecialPoint];

            listResult.Add(new LinearRing(array));
            count++;
            i = (i + 1) % (ring.Count - 1);
        }

        return listResult;
    }

    public List<LinearRing> SliceFigureWithOneSpecialPoint(LinearRing ring)
    {
        var newRing = SegmentService.IgnoreInnerPointsOfSegment(ring);
        var listRingsWithoutSpecialPoints = new List<LinearRing>(2);

        var listSpecialPoints = GetSpecialPoints(newRing);

        if (!listSpecialPoints.Any() || listSpecialPoints.Count > 1)
        {
            listRingsWithoutSpecialPoints.Add(newRing);
            return listRingsWithoutSpecialPoints;
        }

        var pozSpecialPoint = (int)listSpecialPoints[0].M;
        var pozMiddlePoint = ((newRing.Count - 1) / 2 + pozSpecialPoint) % (newRing.Count - 1);

        var coordA = new CoordinateM(
            newRing.Coordinates[(pozSpecialPoint - 1 + newRing.Count - 1) % (newRing.Count - 1)].X,
            newRing.Coordinates[(pozSpecialPoint - 1 + newRing.Count - 1) % (newRing.Count - 1)].Y,
                (pozSpecialPoint - 1 + newRing.Count - 1) % (newRing.Count - 1));

        var coordB = new CoordinateM(
            newRing.Coordinates[(pozSpecialPoint + newRing.Count - 1) % (newRing.Count - 1)].X,
            newRing.Coordinates[(pozSpecialPoint + newRing.Count - 1) % (newRing.Count - 1)].Y,
                (pozSpecialPoint + newRing.Count - 1) % (newRing.Count - 1));

        var coordC = new CoordinateM(
            newRing.Coordinates[(pozSpecialPoint + 1 + newRing.Count - 1) % (newRing.Count - 1)].X,
            newRing.Coordinates[(pozSpecialPoint + 1 + newRing.Count - 1) % (newRing.Count - 1)].Y,
                (pozSpecialPoint + 1 + newRing.Count - 1) % (newRing.Count - 1));

        var flag = true;

        var delta = -1;
        var k = 0;

        while (flag)
        {
            var coordM = new CoordinateM(
                newRing.Coordinates[(pozMiddlePoint + newRing.Count - 1) % (newRing.Count - 1)].X,
                newRing.Coordinates[(pozMiddlePoint + newRing.Count - 1) % (newRing.Count - 1)].Y,
                (pozMiddlePoint + newRing.Count - 1) % (newRing.Count - 1));

            if (coordM.Equals2D(coordC) || coordM.Equals2D(coordA))
            {
                return SimpleSlice(newRing, pozSpecialPoint);
            }

            if (!(SegmentService.IsIntersectionOfSegments(coordB, coordA, coordB, coordM) ||
                  SegmentService.IsIntersectionOfSegments(coordB, coordC, coordB, coordM)) &&
                SegmentService.VectorProduct(
                    new Coordinate(
                        coordB.X - coordA.X,
                        coordB.Y - coordA.Y),
                    new Coordinate(
                        coordM.X - coordB.X,
                        coordM.Y - coordB.Y)
                ) > 0 != _clockwise &&
                SegmentService.VectorProduct(
                    new Coordinate(
                        coordB.X - coordM.X,
                        coordB.Y - coordM.Y),
                    new Coordinate(
                        coordC.X - coordB.X,
                        coordC.Y - coordB.Y)
                ) > 0 != _clockwise
            )
            {
                //MiddlePoint не является особой точкой в новом кольце
                //и не лежит на одной прямой со старой особой точкой и предыдущей для старой особой
                var listFirst = new List<Coordinate>();

                for (var i = (int)coordM.M; i != (int)coordB.M; i = (i + 1) % (newRing.Count - 1))
                {
                    listFirst.Add(newRing[i]);
                }
                listFirst.Add(coordB);
                listFirst.Add(coordM);
                var ringFirst = SegmentService.IgnoreInnerPointsOfSegment(new LinearRing(listFirst.ToArray()));

                var listSecond = new List<Coordinate>();

                for (var i = (int)coordB.M; i != (int)coordM.M; i = (i + 1) % (newRing.Count - 1))
                {
                    listSecond.Add(newRing[i]);
                }
                listSecond.Add(coordM);
                listSecond.Add(coordB);
                var ringSecond = SegmentService.IgnoreInnerPointsOfSegment(new LinearRing(listSecond.ToArray()));

                listRingsWithoutSpecialPoints.Add(ringFirst);
                listRingsWithoutSpecialPoints.Add(ringSecond);

                flag = false;
            }
            else
            {
                pozMiddlePoint = (pozMiddlePoint + delta + newRing.Count - 1) % (newRing.Count - 1);

                if (k % 2 == 0)
                {
                    delta = -delta + 1;
                }
                else
                {
                    delta = -delta - 1;
                }
                k++;

                if (k == newRing.Count - 1)
                {
                    return SimpleSlice(newRing, pozSpecialPoint);
                }
            }
        }

        return listRingsWithoutSpecialPoints;
    }

    public List<LinearRing> Slice(LinearRing ring)
    {
        //Список особых точек
        var listSpecialPoints = GetSpecialPoints(ring);
        var ringCoords = new CoordinatePCN[ring.Count - 1];
        for (var i = 0; i < ring.Count - 1; ++i)
        {
            ringCoords[i] = new CoordinatePCN(ring.Coordinates[i].X, ring.Coordinates[i].Y,
                (i - 2 + ring.Count) % (ring.Count - 1),
                i, (i + 1) % (ring.Count - 1));
        }

        //Список LinearRing для ответа
        var listLinearRing = new List<LinearRing>();

        switch (listSpecialPoints.Count)
        {
            case 0:
                return new List<LinearRing> { ring };
            case 1:
                return SliceFigureWithOneSpecialPoint(ring);
        }

        //coordCurrent, coordNext - координаты текущей и следующей точки, которые мы хотим соединить
        var coordCurrent = ringCoords[(int)listSpecialPoints[0].M];
        var coordPrev = coordCurrent;
        //Индексы начала и конца элементов списка особых точек, которые входят в одно кольцо в итерации
        var beginSpecialPointIndex = 0;
        var endSpecialPointIndex = 0;
        //Индексы точек, с которыми мы соединили начальную точку: beforeFirstIndex -> firstPoint -> afterFirstIndex
        var afterFirstIndex = 0;

        bool wasIntersectionInIteration;


        for (int i = 0, currentSpecialPointIndex = 0;
                currentSpecialPointIndex < listSpecialPoints.Count;
                i = (i + 1) % ringCoords.Length, currentSpecialPointIndex += wasIntersectionInIteration ? 0 : 1)
        {
            if (currentSpecialPointIndex == endSpecialPointIndex && endSpecialPointIndex != listSpecialPoints.Count)
            {
                beginSpecialPointIndex = endSpecialPointIndex;
                endSpecialPointIndex = listSpecialPoints.Count;
                coordCurrent = ringCoords[(int)listSpecialPoints[beginSpecialPointIndex].M];
                coordPrev = coordCurrent;
                //Замена коориднат кольца старой итерации на новое
                for (var j = coordCurrent.C; j != coordCurrent.P;)
                {
                    var coordIter = ringCoords[j];
                    coordIter.PL = coordIter.P;
                    coordIter.NL = coordIter.N;
                    j = coordIter.N;
                }

                ringCoords[coordCurrent.P].PL = ringCoords[coordCurrent.P].P;
                ringCoords[coordCurrent.P].NL = ringCoords[coordCurrent.P].N;
            }

            var coordNext = ringCoords[(int)listSpecialPoints[
                (currentSpecialPointIndex + 1 - beginSpecialPointIndex) %
                (endSpecialPointIndex - beginSpecialPointIndex) + beginSpecialPointIndex].M];


            wasIntersectionInIteration = false;
            if (SegmentService.HasIntersection(ringCoords, coordCurrent.ToCoordinateM(), coordNext.ToCoordinateM()))
            {
                var nextIndex = coordNext.P;
                while (SegmentService.HasIntersection(ringCoords, coordCurrent.ToCoordinateM(), ringCoords[nextIndex].ToCoordinateM()))
                {
                    nextIndex = ringCoords[nextIndex].P;
                }

                wasIntersectionInIteration = true;
                coordNext = ringCoords[nextIndex];
            }

            if (coordPrev.C == (int)listSpecialPoints[beginSpecialPointIndex].M &&
                coordCurrent.C != (int)listSpecialPoints[beginSpecialPointIndex].M)
            {
                afterFirstIndex = coordCurrent.C;
            }

            if (coordNext.C == (int)listSpecialPoints[beginSpecialPointIndex].M)
            {
                var beforeFirstIndex = coordCurrent.C;
                //Добавляем начальную точку, если она особая в получившемся кольце
                if (SegmentService.VectorProduct(
                        new Coordinate(
                            ringCoords[coordNext.C].X - ringCoords[beforeFirstIndex].X,
                            ringCoords[coordNext.C].Y - ringCoords[beforeFirstIndex].Y),
                        new Coordinate(
                            ringCoords[afterFirstIndex].X - ringCoords[coordNext.C].X,
                            ringCoords[afterFirstIndex].Y - ringCoords[coordNext.C].Y)
                    ) > 0 == _clockwise)
                {
                    listSpecialPoints.Add(coordNext.ToCoordinateM());
                }
            }

            if (currentSpecialPointIndex >= beginSpecialPointIndex + 1 &&
                currentSpecialPointIndex <= endSpecialPointIndex - 1)
            {
                //Если особая точка будет особой в получившемся кольце, то добавляем с конец списка особых точек
                if (SegmentService.VectorProduct(
                        new Coordinate(
                            ringCoords[coordCurrent.C].X - ringCoords[coordPrev.C].X,
                            ringCoords[coordCurrent.C].Y - ringCoords[coordPrev.C].Y),
                        new Coordinate(
                            ringCoords[coordNext.C].X - ringCoords[coordCurrent.C].X,
                            ringCoords[coordNext.C].Y - ringCoords[coordCurrent.C].Y)
                    ) > 0 == _clockwise)
                {
                    listSpecialPoints.Add(coordCurrent.ToCoordinateM());
                }
            }

            if (Math.Abs(coordNext.C - coordCurrent.C) + 1 != 2)
            {
                var currentLinearRingCoords = new List<Coordinate>();
                for (var j = coordCurrent.C; j != (coordCurrent.C == coordNext.C ? coordCurrent.P : coordNext.C);)
                {
                    var coordIter = ringCoords[j];
                    currentLinearRingCoords.Add(new Coordinate(ringCoords[j % ringCoords.Length].X,
                        ringCoords[j % ringCoords.Length].Y));
                    j = coordIter.N;
                    //Возможно, стоит написать j = coordIter.NL; - ничего не изменится, но по-смыслу будет ближе
                }

                currentLinearRingCoords.Add(coordCurrent.C == coordNext.C
                    ? ringCoords[coordCurrent.P].ToCoordinate()
                    : ringCoords[coordNext.C].ToCoordinate());
                currentLinearRingCoords.Add(currentLinearRingCoords[0]);
                var currentLinearRing = _gf.CreateLinearRing(currentLinearRingCoords.ToArray());
                var convexLists = SliceFigureWithOneSpecialPoint(currentLinearRing);
                listLinearRing = listLinearRing.Union(convexLists).ToList();
            }

            coordCurrent.N = coordNext.C;
            coordNext.P = coordCurrent.C;
            coordPrev = coordCurrent;
            coordCurrent = coordNext;
            /*Если на текущей итерации мы дошли до конца списка особых точек, при этом
            * новых точек не добавилось, и количество особых точек текущеё итерации > 2,
            * то создаём из всех особых точек текущей итерации LinearRing и добавляем его в listLinearRing
            */
            if (endSpecialPointIndex == listSpecialPoints.Count &&
                currentSpecialPointIndex == endSpecialPointIndex - 1 &&
                endSpecialPointIndex - beginSpecialPointIndex > 2)
            {
                var lastLinearRingCoords = new Coordinate[endSpecialPointIndex - beginSpecialPointIndex + 1];
                for (var j = beginSpecialPointIndex; j < endSpecialPointIndex; ++j)
                {
                    lastLinearRingCoords[j - beginSpecialPointIndex] = listSpecialPoints[j];
                }
                lastLinearRingCoords[endSpecialPointIndex - beginSpecialPointIndex] = listSpecialPoints[beginSpecialPointIndex];
                var lastLinearRing = _gf.CreateLinearRing(lastLinearRingCoords);
                listLinearRing.Add(lastLinearRing);
            }
        }

        return listLinearRing;
    }
}
