using System;
using System.Collections.Generic;
using GeoSlicer.Utils.Intersectors;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.HoleDeletersTests;

public class ZeroTunnelDivider
{
    public int CountOfSteps { get; set; }
    public double StepSize { get; set; }
    
    private LineIntersector _intersector;

    private double _tolerance = 1e-9;

    public ZeroTunnelDivider(int countOfSteps, double stepSize, LineIntersector intersector)
    {
        CountOfSteps = countOfSteps;
        StepSize = stepSize;
        _intersector = intersector;
    }

    public LinearRing DivideZeroTunnels(LinearRing ring)
    {
        var coordinates = ring.Coordinates;
        for (int i = 0; i < coordinates.Length - 1; i++)
        {
            for (int j = 1; j < coordinates.Length; j++)
            {
                if (i != j && coordinates[i].Equals(coordinates[j]) && coordinates[i + 1].Equals(coordinates[j - 1]))
                {
                    if (!MoveTunnel(coordinates, i ,  i + 1) ||
                        !MoveTunnel(coordinates, i + 1,i))
                    {
                        return new LinearRing(coordinates);
                    }
                    

                }
            }
        }

        return new LinearRing(coordinates);
    }

    //numbersOfEqualCoordsSecondCoord - номера координат, которые совпадают с координатой с номером secondCoordFirstTunnel
    //numbersOfEqualCoordsAdjacentCoord - номера координат, которые совпадают с координатой с номером coordAdjacentLine
    //в методе MoveTunnel
    //метод пытается передвинуть точку под номером firstCoordFirstTunnel по ступенькам. True в случае успеха, false иначе
    private int m = 0;
    private bool MoveTunnel(Coordinate[] coordinates, int firstCoordFirstTunnel, int secondCoordFirstTunnel)
    {
        m++;
        
        LinkedList<int> numbersOfEqualCoordsSecondCoord = new LinkedList<int>();
        LinkedList<int> numbersOfEqualCoordsAdjacentCoord = new LinkedList<int>();
        FillNumbersOfEqualCoordinates(coordinates, secondCoordFirstTunnel, numbersOfEqualCoordsSecondCoord);
        //номер второй координаты линии, смежной к проверяемой линии-(firstCoordFirstTunnel, secondCoordFirstTunnel)
        int coordAdjacentLine = (2 * firstCoordFirstTunnel - secondCoordFirstTunnel + coordinates.Length - 1) %
                                (coordinates.Length - 1);
        FillNumbersOfEqualCoordinates(coordinates, coordAdjacentLine, numbersOfEqualCoordsAdjacentCoord);
        
        var originalCoord = coordinates[firstCoordFirstTunnel];
        bool correctMove = false;
        
        //quarterNumber = номер четверти на координатной оси
        for (int quarterNumber = 1; quarterNumber <= 4; quarterNumber++)
        {
            Coordinate buffer = coordinates[firstCoordFirstTunnel];
            
            //stepNumber = номер ступеньки на которую передвигаем текущую координату
            for (int stepNumber = 0; stepNumber < CountOfSteps; stepNumber++)
            {
                MovePointUpTheStairs(coordinates, quarterNumber, stepNumber, firstCoordFirstTunnel);
                GeoJsonFileService.GeoJsonFileService.WriteGeometryToFile(new LinearRing(coordinates), "C:\\Users\\Данил\\Downloads\\Telegram Desktop\\newSampleBefore"+ m + quarterNumber + stepNumber + ".geojson");
                if (CheckIntersects(coordinates, firstCoordFirstTunnel, secondCoordFirstTunnel, coordAdjacentLine,
                        numbersOfEqualCoordsSecondCoord, numbersOfEqualCoordsAdjacentCoord))
                {
                    if (correctMove)
                    {
                        AssignNewCoordinate(
                            coordinates,
                            firstCoordFirstTunnel,
                            buffer);
                        return true;
                    }
                }
                else correctMove = true;
                buffer = coordinates[firstCoordFirstTunnel];
            }

            if (correctMove)
            {
                return true;
            }
            if (quarterNumber != 4) 
                AssignNewCoordinate(
                    coordinates, 
                    firstCoordFirstTunnel, 
                    originalCoord);
        }
        return false;
    }
    
    //Проверяет, что линии с общей точкой с номером firstCoordFirstTunnel не пересекают другие линии в проверяемой геометрии
    //Точка с координатой secondCoordFirstTunnel может совпадать с другими координатами в
    //геометрии(накладываться на координаты других нулевых тунелей)
    private int g = 0;
    private bool CheckIntersects(Coordinate[] coordinates,
        int firstCoordFirstTunnel, int secondCoordFirstTunnel, int coordAdjacentLine,
        LinkedList<int> numbersOfEqualCoordsSecondCoord,
        LinkedList<int> numbersOfEqualCoordsAdjacentCoord)
    {
        for (int i = 0; i < coordinates.Length - 1; i++)
        {
            if (!EqualLines(
                    i, 
                    i + 1, 
                    firstCoordFirstTunnel, 
                    secondCoordFirstTunnel,
                    coordinates) &&
                !EqualLines(
                    i, 
                    i + 1, 
                    coordAdjacentLine, 
                    firstCoordFirstTunnel,
                    coordinates))
            {
                bool res1 = CheckIntersectsLine(i, firstCoordFirstTunnel, secondCoordFirstTunnel, coordinates,
                    numbersOfEqualCoordsSecondCoord);
                
                bool res2 = CheckIntersectsLine(i, firstCoordFirstTunnel, coordAdjacentLine, coordinates,
                    numbersOfEqualCoordsAdjacentCoord);
                
                if (res1 || res2)
                    return true;
            }
        }
        return false;
    }

    private bool CheckIntersectsLine(int i, int firstCoordFirstTunnel, int secondCoordFirstTunnel, Coordinate[] coordinates, LinkedList<int> numberOfEqualCoordinates)
    {
        var intersectionType = _intersector.GetIntersection(
                        coordinates[i],
                        coordinates[i + 1],
                        coordinates[firstCoordFirstTunnel], 
                        coordinates[secondCoordFirstTunnel]);
        var intersection = intersectionType.Item1;
        var intersectionPoint = intersectionType.Item2;
        //в случае пересечения extension getIntersection возращает точку пересечения null
        if (intersection == IntersectionType.Extension)
        {
            if (coordinates[i].Equals2D(coordinates[firstCoordFirstTunnel], _tolerance) ||
                coordinates[i].Equals2D(coordinates[secondCoordFirstTunnel], _tolerance))
            {
                intersectionPoint = coordinates[i];
            }
            else
            {
                intersectionPoint = coordinates[i + 1];
            }
        }
        if (intersection != IntersectionType.NoIntersection && intersection != IntersectionType.Outside)
        {
            if ((IntersectionType.Overlay <= intersection && IntersectionType.Equals >= intersection)
                || IntersectionType.Inner == intersection || IntersectionType.TyShaped == intersection)
            {
                return true;
            }
            //дальше пересечение либо corner, либо extension
            if (!LinesFollowEachOther(
                    i, 
                    i + 1, 
                    firstCoordFirstTunnel, 
                    secondCoordFirstTunnel, 
                    coordinates))
                    
            {
                //линии не идут друг за другом
                bool flag = false;
                foreach (var number in numberOfEqualCoordinates)
                {
                    if (coordinates[number].Equals2D(intersectionPoint, _tolerance))
                        flag = true;
                }
                if (!flag)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void FillNumbersOfEqualCoordinates(Coordinate[] coordinates, int numberOfCoordinate, LinkedList<int> numbersOfEqualsCoordinates)
    {
        for (int i = 0; i < coordinates.Length; i++)
        {
            if (coordinates[i].Equals(coordinates[numberOfCoordinate]))
                numbersOfEqualsCoordinates.AddFirst(i);
        }
    }
    private void AssignNewCoordinate(Coordinate[] coordinates, int coordinateFirstLine, Coordinate newCoordinate)
    {
        if (coordinateFirstLine == 0 || coordinateFirstLine == coordinates.Length - 1)
        {
            coordinates[0] = newCoordinate;
            coordinates[coordinates.Length - 1] = newCoordinate;
        }
        else coordinates[coordinateFirstLine] = newCoordinate;
    }
    //проверяет, что переданные линии являются одной и той же линией
    private bool EqualLines(int firstCoordFirstTunnel, int secondCoordFirstTunnel,
        int firstCoordSecondTunnel, int secondCoordSecondTunnel, Coordinate[] coordinates)
    {
        if (firstCoordFirstTunnel == coordinates.Length - 1)
            firstCoordFirstTunnel = 0;
        if (secondCoordFirstTunnel == coordinates.Length - 1)
            secondCoordFirstTunnel = 0;
        if (firstCoordSecondTunnel == coordinates.Length - 1)
            firstCoordSecondTunnel = 0;
        if (secondCoordSecondTunnel == coordinates.Length - 1)
            secondCoordSecondTunnel = 0;
        return (firstCoordFirstTunnel == firstCoordSecondTunnel && secondCoordFirstTunnel == secondCoordSecondTunnel) ||
               (secondCoordFirstTunnel == firstCoordSecondTunnel && firstCoordFirstTunnel == secondCoordSecondTunnel);
    }
    //проверяет, что линии идут друг за другом(неважно в каком порядке) 
    private bool LinesFollowEachOther(int firstCoordFirstLine, int secondCoordFirstLine,
        int firstCoordSecondLine, int secondCoordSecondLine, Coordinate[] coordinates)
    {
        if (firstCoordFirstLine == coordinates.Length - 1)
            firstCoordFirstLine = 0;
        if (secondCoordFirstLine == coordinates.Length - 1)
            secondCoordFirstLine = 0;
        if (firstCoordSecondLine == coordinates.Length - 1)
            firstCoordSecondLine = 0;
        if (secondCoordSecondLine == coordinates.Length - 1)
            secondCoordSecondLine = 0;
        
        return firstCoordSecondLine == firstCoordFirstLine
               || firstCoordSecondLine == secondCoordFirstLine
               || secondCoordSecondLine == firstCoordFirstLine
               || secondCoordSecondLine == secondCoordFirstLine;
    }

    private void MovePointUpTheStairs(
        Coordinate[] coordinates,
        int quarterNumber,
        int numberOfIteration,
        int firstCoordFirstTunnel)
    {
        if (numberOfIteration % 2 == 0)
        {
            if (quarterNumber == 1 || quarterNumber == 4)
                AssignNewCoordinate(
                    coordinates, 
                    firstCoordFirstTunnel, 
                    new Coordinate(
                        coordinates[firstCoordFirstTunnel].X + StepSize, 
                        coordinates[firstCoordFirstTunnel].Y));
            else if (quarterNumber == 2 || quarterNumber == 3)
                AssignNewCoordinate(
                    coordinates, 
                    firstCoordFirstTunnel, 
                    new Coordinate(
                        coordinates[firstCoordFirstTunnel].X - StepSize,
                        coordinates[firstCoordFirstTunnel].Y));
                    
        }
        else
        {
            if (quarterNumber == 1 || quarterNumber == 2)
                AssignNewCoordinate(
                    coordinates, 
                    firstCoordFirstTunnel, 
                    new Coordinate(coordinates[firstCoordFirstTunnel].X,
                        coordinates[firstCoordFirstTunnel].Y + StepSize));
                    
            else if (quarterNumber == 3 || quarterNumber == 4)
                AssignNewCoordinate(
                    coordinates, 
                    firstCoordFirstTunnel, 
                    new Coordinate(coordinates[firstCoordFirstTunnel].X,
                        coordinates[firstCoordFirstTunnel].Y - StepSize));
        }
    }
}