using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace GeoSlicer;

public class NonConvexSlicer
{
    private readonly GeometryFactory _gf;
    private readonly bool _clockwise;

    public NonConvexSlicer(bool clockwise)
    {
        _gf = NtsGeometryServices.Instance.CreateGeometryFactory(4326);
        _clockwise = clockwise;
    }

    private double VectorProduct(Coordinate firstVec,
        Coordinate secondVec,
        double epsilon = 1e-9)
    {
        double product = firstVec.X * secondVec.Y - secondVec.X * firstVec.Y;

        if (Math.Abs(product) < epsilon)
        {
            return 0;
        }

        return product;
    }

    public List<CoordinateZM> GetSpecialPoints(LinearRing ring)
    {
        var list = new List<CoordinateZM>();
        for (var i = 0; i < ring.Coordinates.Length - 1; ++i)
        {
            if (VectorProduct(
                    new Coordinate(
                        ring.Coordinates[i].X -
                        ring.Coordinates[(i - 1 + ring.Coordinates.Length - 1) % (ring.Coordinates.Length - 1)].X,
                        ring.Coordinates[i].Y -
                        ring.Coordinates[(i - 1 + ring.Coordinates.Length - 1) % (ring.Coordinates.Length - 1)].Y),
                    new Coordinate(ring.Coordinates[(i + 1) % (ring.Coordinates.Length - 1)].X - ring.Coordinates[i].X,
                        ring.Coordinates[(i + 1) % (ring.Coordinates.Length - 1)].Y - ring.Coordinates[i].Y)
                ) > 0 == _clockwise)
            {
                list.Add(new CoordinateZM(ring.Coordinates[i].X, ring.Coordinates[i].Y, (i + 1) % (ring.Count - 1), i));
            }
        }

        return list;
    }

    private bool IsIntersectionOfSegments(Coordinate firstSegmentPointA,
        Coordinate firstSegmentPointB,
        Coordinate secondSegmentPointC,
        Coordinate secondSegmentPointD,
        double epsilon = 1e-9)
    {
        var vecAB = new Coordinate(firstSegmentPointB.X - firstSegmentPointA.X,
            firstSegmentPointB.Y - firstSegmentPointA.Y);

        var vecAC = new Coordinate(secondSegmentPointC.X - firstSegmentPointA.X,
            secondSegmentPointC.Y - firstSegmentPointA.Y);

        var vecAD = new Coordinate(secondSegmentPointD.X - firstSegmentPointA.X,
            secondSegmentPointD.Y - firstSegmentPointA.Y);

        var vecCD = new Coordinate(secondSegmentPointD.X - secondSegmentPointC.X,
            secondSegmentPointD.Y - secondSegmentPointC.Y);

        var vecCA = new Coordinate(firstSegmentPointA.X - secondSegmentPointC.X,
            firstSegmentPointA.Y - secondSegmentPointC.Y);

        var vecCB = new Coordinate(firstSegmentPointB.X - secondSegmentPointC.X,
            firstSegmentPointB.Y - secondSegmentPointC.Y);

        return (!(VectorProduct(vecAB, vecAC, epsilon) >= 0) || !(VectorProduct(vecAB, vecAD, epsilon) >= 0)) &&
               (!(VectorProduct(vecAB, vecAC, epsilon) <= 0) || !(VectorProduct(vecAB, vecAD, epsilon) <= 0)) &&
               (!(VectorProduct(vecCD, vecCA, epsilon) >= 0) || !(VectorProduct(vecCD, vecCB, epsilon) >= 0)) &&
               (!(VectorProduct(vecCD, vecCA, epsilon) <= 0) || !(VectorProduct(vecCD, vecCB, epsilon) <= 0));
    }

    private bool HasIntersection(CoordinateZM[] ring, CoordinateZM coordCurrent, CoordinateZM coordNext)
    {
        if (coordCurrent.Equals2D(coordNext)) return true;
        for (var i = 0; i < ring.Length; i++)
        {
            while (double.IsNaN(ring[i].M)) i++;
            var firstCoord = ring[i];
            while (double.IsNaN(ring[(i + 1) % ring.Length].M)) i++;
            if (IsIntersectionOfSegments(coordCurrent, coordNext, firstCoord, ring[(i + 1) % ring.Length]))
            {
                return true;
            }
        }

        return false;
    }

    public List<LinearRing> SliceFigureWithOneSpecialPoint(LinearRing ring)
    {
        var listTwoRingsWithoutSpecialPoints = new List<LinearRing>(2);
 
        var listSpecialPoints = GetSpecialPoints(ring);
 
        if(!listSpecialPoints.Any() || listSpecialPoints.Count > 1)
        {
            listTwoRingsWithoutSpecialPoints.Add(ring);
            return listTwoRingsWithoutSpecialPoints;
        }
 
        var pozSpecialPoint = (int) listSpecialPoints[0].M;
        var pozMiddlePoint = (ring.Count - 1) / 2;
 
        var coordA = new CoordinateM(
                ring.Coordinates[(pozSpecialPoint - 1 + ring.Count - 1) % (ring.Count - 1)].X,
                ring.Coordinates[(pozSpecialPoint - 1 + ring.Count - 1) % (ring.Count - 1)].Y,
                (pozSpecialPoint - 1 + ring.Count - 1) % (ring.Count - 1));
 
        var coordB = new CoordinateM(
                ring.Coordinates[(pozSpecialPoint + ring.Count - 1) % (ring.Count - 1)].X,
                ring.Coordinates[(pozSpecialPoint + ring.Count - 1) % (ring.Count - 1)].Y,
                (pozSpecialPoint + ring.Count - 1) % (ring.Count - 1));
 
        var coordC = new CoordinateM(
                ring.Coordinates[(pozSpecialPoint + 1 + ring.Count - 1) % (ring.Count - 1)].X,
                ring.Coordinates[(pozSpecialPoint + 1 + ring.Count - 1) % (ring.Count - 1)].Y,
                (pozSpecialPoint + 1 + ring.Count - 1) % (ring.Count - 1));
 
        var flag = true;
 
        var delta = -1;
        var k = 0;
 
        while (flag)
        {
            var coordM = new CoordinateM(
                ring.Coordinates[(pozMiddlePoint + ring.Count - 1) % (ring.Count - 1)].X,
                ring.Coordinates[(pozMiddlePoint + ring.Count - 1) % (ring.Count - 1)].Y,
                (pozMiddlePoint + ring.Count - 1) % (ring.Count - 1));
 
            if (VectorProduct(
                    new Coordinate(
                        coordB.X - coordA.X,
                        coordB.Y - coordA.Y),
                    new Coordinate(
                        coordM.X - coordB.X,
                        coordM.Y - coordB.Y)
                ) > 0 != _clockwise && 
                VectorProduct(
                    new Coordinate(
                        coordB.X - coordM.X,
                        coordB.Y - coordM.Y),
                    new Coordinate(
                        coordC.X - coordB.X,
                        coordC.Y - coordB.Y)
                ) > 0 != _clockwise &&
                !(IsIntersectionOfSegments(coordA, coordB, coordA, coordM) &&
                  IsIntersectionOfSegments(coordB, coordC, coordB, coordM))
            )
            {
                //MiddlePoint не является особой точкой в новом кольце
                //и не лежит на одной прямой со старой особой точкой и предыдущей для старой особой
                var listFirst = new List<Coordinate>();
 
                for(var i = (int)coordM.M; i != (int)coordB.M; i = (i + 1) % (ring.Count - 1))
                {
                    listFirst.Add(ring[i]);
                }
                listFirst.Add(coordB);
                listFirst.Add(coordM);
                var ringFirst = new LinearRing(listFirst.ToArray());
 
                var listSecond = new List<Coordinate>();
 
                for (var i = (int)coordB.M; i != (int)coordM.M; i = (i + 1) % (ring.Count - 1))
                {
                    listSecond.Add(ring[i]);
                }
                listSecond.Add(coordM);
                listSecond.Add(coordB);
                var ringSecond = new LinearRing(listSecond.ToArray());
 
                listTwoRingsWithoutSpecialPoints.Add(ringFirst);
                listTwoRingsWithoutSpecialPoints.Add(ringSecond);
 
                flag = false;
            }
            else
            {
                pozMiddlePoint = (pozMiddlePoint + delta + ring.Count - 1) % (ring.Count - 1);
 
                if(k % 2 == 0)
                {
                    delta = -delta + 1;
                }
                else
                {
                    delta = -delta - 1;
                }
                k++;
            } 
        }
 
        return listTwoRingsWithoutSpecialPoints;
    }

    public List<LinearRing> Slice(LinearRing ring)
    {
        //Список особых точек
        var listSpecialPoints = GetSpecialPoints(ring);
        /* Массив координат исходного LinearRing без повторений, в котором у каждой точки есть пометка М:
         * Если М == i, то точка будет просмотрена в цикле обхода точек, i - индекс точки в исходном массиве
         * Если М = NaN, то точка игнорируется
         */
        var ringCoords = new CoordinateZM[ring.Count - 1];
        for (var i = 0; i < ring.Count - 1; ++i)
        {
            ringCoords[i] = new CoordinateZM(ring.Coordinates[i].X, ring.Coordinates[i].Y, (i + 1) % (ring.Count - 1), i);
        }

        //Список LinearRing для ответа
        var listLinearRing = new List<LinearRing>();

        switch (listSpecialPoints.Count)
        {
            //Если список особых точек пуст, то возвращаем исходный LinearRing
            case 0:
                return new List<LinearRing> { ring };
            case 1:
                return SliceFigureWithOneSpecialPoint(ring);
        }

        //coordCurrent, coordNext - координаты текущей и следующей точки, которые мы хотим соединить
        var coordCurrent = listSpecialPoints[0];
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
                coordCurrent = listSpecialPoints[beginSpecialPointIndex];
                coordPrev = coordCurrent;

                /*coordPrev =
                    listSpecialPoints[
                        (currentSpecialPointIndex - 1 + endSpecialPointIndex - 1 - 2 * beginSpecialPointIndex) %
                        (endSpecialPointIndex - 1 - beginSpecialPointIndex) + beginSpecialPointIndex];*/
            }

            var coordNext = listSpecialPoints[
                (currentSpecialPointIndex + 1 - beginSpecialPointIndex) %
                (endSpecialPointIndex - beginSpecialPointIndex) + beginSpecialPointIndex];


            wasIntersectionInIteration = false;
            if (HasIntersection(ringCoords, coordCurrent, coordNext))
            {
                /*
                 * Проблема с индексацией: nextIndex идёт по элементам исходного LinearRing, а не по элементам, которые входят в текущую итерацию
                 * можно хранить такую структуру:
                 * P - previous: int, индекс элемента, из которого мы можем попасть в текущий
                 * C - current: int, индекс элемента в исходном массиве
                 * N - next: int, индекс элемента, в который можем перейти из текущего
                 * 
                 */
                var nextIndex = (int)coordNext.M - 1;
                while (HasIntersection(ringCoords, coordCurrent, ringCoords[nextIndex]))
                {
                    nextIndex = (nextIndex - 1) % ringCoords.Length;
                }

                wasIntersectionInIteration = true;
                coordNext = ringCoords[nextIndex];
            }

            if ((int)coordPrev.M == (int)listSpecialPoints[beginSpecialPointIndex].M &&
                (int)coordCurrent.M != (int)listSpecialPoints[beginSpecialPointIndex].M)
            {
                afterFirstIndex = (int)coordCurrent.M;
            }

            if ((int)coordNext.M == (int)listSpecialPoints[beginSpecialPointIndex].M)
            {
                var beforeFirstIndex = (int)coordCurrent.M;
                //Добавляем начальную точку, если она особая в получившемся кольце
                if (VectorProduct(
                        new Coordinate(
                            ringCoords[(int)coordNext.M].X - ringCoords[beforeFirstIndex].X,
                            ringCoords[(int)coordNext.M].Y - ringCoords[beforeFirstIndex].Y),
                        new Coordinate(
                            ringCoords[afterFirstIndex].X - ringCoords[(int)coordNext.M].X,
                            ringCoords[afterFirstIndex].Y - ringCoords[(int)coordNext.M].Y)
                    ) > 0 == _clockwise)
                {
                    listSpecialPoints.Add(coordNext);
                }
            }

            if (currentSpecialPointIndex >= beginSpecialPointIndex + 1 &&
                currentSpecialPointIndex <= endSpecialPointIndex - 1)
            {
                //Если особая точка будет особой в получившемся кольце, то добавляем с конец списка особых точек
                if (VectorProduct(
                        new Coordinate(
                            ringCoords[(int)coordCurrent.M].X - ringCoords[(int)coordPrev.M].X,
                            ringCoords[(int)coordCurrent.M].Y - ringCoords[(int)coordPrev.M].Y),
                        new Coordinate(
                            ringCoords[(int)coordNext.M].X - ringCoords[(int)coordCurrent.M].X,
                            ringCoords[(int)coordNext.M].Y - ringCoords[(int)coordCurrent.M].Y)
                    ) > 0 == _clockwise)
                {
                    listSpecialPoints.Add(coordCurrent);
                }
            }

            //Если отсекается больше 2 точек, то продолжаем
            if (Math.Abs((int)coordNext.M - (int)coordCurrent.M) + 1 != 2)
            {
                var currentLinearRingCoords = new List<Coordinate>();
                for (var j = (int)coordCurrent.M;
                     j <= (int)coordNext.M + ((int)coordNext.M < (int)coordCurrent.M ? ringCoords.Length : 0);
                     j++)
                {
                    //Если не входим в текущее кольцо, то пропускаем
                    if (double.IsNaN(ringCoords[j % ringCoords.Length].M)) continue;
                    //Помечаем неособые точки как точки, не входящие в формируемое кольцо
                    if (j % ringCoords.Length != (int)coordCurrent.M && j % ringCoords.Length != (int)coordNext.M)
                        ringCoords[j % ringCoords.Length].M = double.NaN;
                    currentLinearRingCoords.Add(new Coordinate(ringCoords[j % ringCoords.Length].X, ringCoords[j % ringCoords.Length].Y));
                }

                currentLinearRingCoords.Add(currentLinearRingCoords[0]);
                var currentLinearRing = new LinearRing(currentLinearRingCoords.ToArray());
                var convexLists = SliceFigureWithOneSpecialPoint(currentLinearRing);
                listLinearRing = listLinearRing.Union(convexLists).ToList();
                //listLinearRing.Add(currentLinearRing);
            }

            coordPrev = coordCurrent;
            coordCurrent = coordNext;
        }
        return listLinearRing;
    }
}