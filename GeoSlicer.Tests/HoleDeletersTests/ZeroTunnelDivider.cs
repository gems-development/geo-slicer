﻿using System;
using System.Collections.Generic;
using GeoSlicer.Utils.Intersectors;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.HoleDeletersTests;

public class ZeroTunnelDivider
{
    private readonly IList<(int countOfSteps, double stepSize)> _stepCharacteristic;
    private LineIntersector _intersector;

    private readonly double _tolerance;

    public ZeroTunnelDivider(
        IList<(int countOfSteps, double stepSize)> stepCharacteristic,
        LineIntersector intersector, double tolerance)
    {
        _intersector = intersector;
        _tolerance = tolerance;
        _stepCharacteristic = stepCharacteristic;
    }
    
    
    public void DivideZeroTunnels(LinearRing ring, out LinearRing resultRing, out LinkedList<Coordinate> problemCoordinates)
    {
        Coordinate[] oldCoordinates = ring.Coordinates;
        Coordinate[] coordinates = new Coordinate[oldCoordinates.Length - 1];
        Array.Copy(oldCoordinates, 0, coordinates, 0, coordinates.Length);
        problemCoordinates
            = new LinkedList<Coordinate>();
        for (int stepNumber = 0; stepNumber < _stepCharacteristic.Count; stepNumber++)
        {
            bool stepNumberIterationHaveErrors = false;
            
            for (int i = 0; i < coordinates.Length - 1; i++)
            {
                for (int j = i + 1; j < coordinates.Length; j++)
                {
                    if (i != j && coordinates[i].Equals(coordinates[j]))
                    {
                        var buff = coordinates[i];
                        if (!MoveCoordinate(
                                coordinates,
                                i,
                                _stepCharacteristic[stepNumber].countOfSteps,
                                _stepCharacteristic[stepNumber].stepSize))
                        {
                            if (stepNumber == _stepCharacteristic.Count - 1)
                                problemCoordinates.AddFirst(coordinates[i]);
                            else coordinates[i] = buff;
                            stepNumberIterationHaveErrors = true;
                        }
                        break;
                    }
                }
            }

            if (!stepNumberIterationHaveErrors)
                break;
        }

        Coordinate[] newCoordinates = new Coordinate[oldCoordinates.Length];
        Array.Copy(coordinates, 0, newCoordinates, 0, coordinates.Length);
        newCoordinates[newCoordinates.Length - 1] = coordinates[0];
        resultRing = new LinearRing(newCoordinates);
    }

    // numbersOfEqualCoordsSecondCoord - номера координат, которые совпадают с координатой с номером secondCoordFirstTunnel
    // numbersOfEqualCoordsAdjacentCoord - номера координат, которые совпадают с координатой с номером coordAdjacentLine
    // в методе MoveCoordinate
    // метод пытается передвинуть точку под номером firstCoord по ступенькам. True в случае успеха, false иначе
    private bool MoveCoordinate(Coordinate[] coordinates, int firstCoordinate, int countOfSteps, double stepSize)
    {
        LinkedList<int> numbersOfEqualCoordsSecondCoord = new LinkedList<int>();
        LinkedList<int> numbersOfEqualCoordsAdjacentCoord = new LinkedList<int>();
        int secondCoord = firstCoordinate + 1;
        FillNumbersOfEqualCoordinates(coordinates, secondCoord, numbersOfEqualCoordsSecondCoord);
        // номер второй координаты линии, смежной к проверяемой линии-(firstCoordFirstTunnel, secondCoordFirstTunnel)
        int coordAdjacentLine = (2 * firstCoordinate - secondCoord + coordinates.Length - 1) %
                                (coordinates.Length - 1);
        FillNumbersOfEqualCoordinates(coordinates, coordAdjacentLine, numbersOfEqualCoordsAdjacentCoord);
        
        var originalCoord = coordinates[firstCoordinate];
        bool correctMove = false;
        // quarterNumber = номер четверти на координатной оси
        for (int quarterNumber = 1; quarterNumber <= 4; quarterNumber++)
        {
            Coordinate buffer = coordinates[firstCoordinate];
            
            // stepNumber = номер ступеньки на которую передвигаем текущую координату
            for (int stepNumber = 0; stepNumber < countOfSteps; stepNumber++)
            {
                MovePointUpTheStairs(coordinates, quarterNumber, stepNumber, firstCoordinate, stepSize);
                if (CheckIntersects(coordinates, firstCoordinate, secondCoord, coordAdjacentLine,
                        numbersOfEqualCoordsSecondCoord, numbersOfEqualCoordsAdjacentCoord))
                {
                    if (correctMove)
                    {
                        coordinates[firstCoordinate] = buffer;
                        return true;
                    }
                }
                else
                {
                    correctMove = true;
                }
                buffer = coordinates[firstCoordinate];
            }

            if (correctMove)
            {
                return true;
            }
            if (quarterNumber != 4) 
                coordinates[firstCoordinate] = originalCoord;
        }
        return false;
    }
    
    // Проверяет, что линии с общей точкой с номером firstCoordFirstTunnel не пересекают другие линии в проверяемой геометрии
    // Точки с координатами secondCoord и coordAdjacentLine может совпадать с другими координатами в
    // геометрии(накладываться на координаты других нулевых тунелей)
    private bool CheckIntersects(Coordinate[] coordinates,
        int firstCoord, int secondCoord, int coordAdjacentLine,
        LinkedList<int> numbersOfEqualCoordsSecondCoord,
        LinkedList<int> numbersOfEqualCoordsAdjacentCoord)
    {
        for (int i = 0; i < coordinates.Length - 1; i++)
        {
            if (!EqualLines(
                    i, 
                    i + 1, 
                    firstCoord, 
                    secondCoord) &&
                !EqualLines(
                    i, 
                    i + 1, 
                    coordAdjacentLine, 
                    firstCoord))
            {
                bool res1 = CheckIntersectsLine(i, firstCoord, secondCoord, coordinates,
                    numbersOfEqualCoordsSecondCoord);
                
                bool res2 = CheckIntersectsLine(i, firstCoord, coordAdjacentLine, coordinates,
                    numbersOfEqualCoordsAdjacentCoord);
                
                if (res1 || res2)
                    return true;
            }
        }
        return false;
    }

    private bool CheckIntersectsLine(
        int coordLineChecked, int firstCoordFirstTunnel, int secondCoordFirstTunnel,
        Coordinate[] coordinates, LinkedList<int> numberOfEqualCoordinates)
    {
        var intersectionType = _intersector.GetIntersection(
                        coordinates[coordLineChecked],
                        coordinates[coordLineChecked + 1],
                        coordinates[firstCoordFirstTunnel], 
                        coordinates[secondCoordFirstTunnel]);
        var intersection = intersectionType.Item1;
        var intersectionPoint = intersectionType.Item2;
        if (intersection != IntersectionType.NoIntersection && intersection != IntersectionType.Outside)
        {
            if ((IntersectionType.Overlay <= intersection && IntersectionType.Equals >= intersection)
                || IntersectionType.Inner == intersection || IntersectionType.TyShaped == intersection)
            {
                return true;
            }
            // Дальше пересечение либо corner, либо extension
            if (!LinesFollowEachOther(
                    coordLineChecked, 
                    coordLineChecked + 1, 
                    firstCoordFirstTunnel, 
                    secondCoordFirstTunnel))
                    
            {
                // Линии не идут друг за другом
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

    private void FillNumbersOfEqualCoordinates(
        Coordinate[] coordinates, int numberOfCoordinate, LinkedList<int> numbersOfEqualsCoordinates)
    {
        for (int i = 0; i < coordinates.Length; i++)
        {
            if (coordinates[i].Equals(coordinates[numberOfCoordinate]))
                numbersOfEqualsCoordinates.AddFirst(i);
        }
    }
    // Проверяет, что переданные линии являются одной и той же линией
    private bool EqualLines(int firstCoordFirstTunnel, int secondCoordFirstTunnel,
        int firstCoordSecondTunnel, int secondCoordSecondTunnel)
    {
        return (firstCoordFirstTunnel == firstCoordSecondTunnel && secondCoordFirstTunnel == secondCoordSecondTunnel) ||
               (secondCoordFirstTunnel == firstCoordSecondTunnel && firstCoordFirstTunnel == secondCoordSecondTunnel);
    }
    // Проверяет, что линии идут друг за другом(неважно в каком порядке) 
    private bool LinesFollowEachOther(int firstCoordFirstLine, int secondCoordFirstLine,
        int firstCoordSecondLine, int secondCoordSecondLine)
    { 
        return firstCoordSecondLine == firstCoordFirstLine
             || firstCoordSecondLine == secondCoordFirstLine
             || secondCoordSecondLine == firstCoordFirstLine
             || secondCoordSecondLine == secondCoordFirstLine;
    }

    private void MovePointUpTheStairs(
        Coordinate[] coordinates,
        int quarterNumber,
        int numberOfIteration,
        int firstCoordFirstTunnel,
        double stepSize)
    {
        if (numberOfIteration % 2 == 0)
        {
            if (quarterNumber == 1 || quarterNumber == 4)
                coordinates[firstCoordFirstTunnel] = new Coordinate(
                    coordinates[firstCoordFirstTunnel].X + stepSize, coordinates[firstCoordFirstTunnel].Y);
            else if (quarterNumber == 2 || quarterNumber == 3)
                coordinates[firstCoordFirstTunnel] = new Coordinate(
                    coordinates[firstCoordFirstTunnel].X - stepSize, coordinates[firstCoordFirstTunnel].Y);
        }
        else
        {
            if (quarterNumber == 1 || quarterNumber == 2)
                coordinates[firstCoordFirstTunnel] = new Coordinate(
                    coordinates[firstCoordFirstTunnel].X, coordinates[firstCoordFirstTunnel].Y + stepSize);
            else if (quarterNumber == 3 || quarterNumber == 4)
                coordinates[firstCoordFirstTunnel] = new Coordinate(
                    coordinates[firstCoordFirstTunnel].X,coordinates[firstCoordFirstTunnel].Y - stepSize);
        }
    }
}