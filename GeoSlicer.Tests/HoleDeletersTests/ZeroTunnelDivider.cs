using System;
using System.Collections.Generic;
using GeoSlicer.Utils.Intersectors;
using Microsoft.CSharp.RuntimeBinder;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.HoleDeletersTests;

public class ZeroTunnelDivider
{
    public int CountOfSteps { get; set; }
    public double StepSize { get; set; }
    
    private LineIntersector _intersector;

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
                    if (!MoveTunnel(coordinates, i, j, i + 1, j - 1) ||
                        !MoveTunnel(coordinates, i + 1, j - 1, i, j))
                    {
                        return new LinearRing(coordinates);
                    }
                    

                }
            }
        }

        return new LinearRing(coordinates);
    }

    private LinkedList<int>? _numbersOfEqualCoordinates;
    private bool MoveTunnel(Coordinate[] coordinates, int firstCoordFirstTunnel, int firstCoordSecondTunnel, int secondCoordFirstTunnel, int secondCoordSecondTunnel)
    {
        _numbersOfEqualCoordinates = new LinkedList<int>();
        FillNumbersOfEqualCoordinates(coordinates, secondCoordFirstTunnel);
        var originalCoord = coordinates[firstCoordFirstTunnel];
        for (int j = 0; j < 4; j++)
        {
            Coordinate? buffer = null;
            bool flagOfFirstIteration = false;
            for (int i = 0; i < CountOfSteps; i++)
            {
                if (i % 2 == 0)
                {
                    if (j == 0 || j == 1)
                        AssignNewCoordinate(
                            coordinates, 
                            firstCoordFirstTunnel, 
                            new Coordinate(
                                coordinates[firstCoordFirstTunnel].X + StepSize, 
                                coordinates[firstCoordFirstTunnel].Y));
                    else if (j == 2 || j == 3)
                        AssignNewCoordinate(
                        coordinates, 
                        firstCoordFirstTunnel, 
                        new Coordinate(
                            coordinates[firstCoordFirstTunnel].X - StepSize,
                            coordinates[firstCoordFirstTunnel].Y));
                    
                }
                else
                {
                    if (j == 0 || j == 3)
                        AssignNewCoordinate(
                            coordinates, 
                            firstCoordFirstTunnel, 
                          new Coordinate(coordinates[firstCoordFirstTunnel].X,
                              coordinates[firstCoordFirstTunnel].Y + StepSize));
                    
                    else if (j == 1 || j == 2)
                        AssignNewCoordinate(
                            coordinates, 
                            firstCoordFirstTunnel, 
                            new Coordinate(coordinates[firstCoordFirstTunnel].X,
                                coordinates[firstCoordFirstTunnel].Y - StepSize));
                }
                if (CheckIntersects(coordinates, firstCoordFirstTunnel, secondCoordFirstTunnel))
                {
                    if (i == 0) break;
                    AssignNewCoordinate(
                        coordinates, 
                        firstCoordFirstTunnel, 
                        buffer!);
                    _numbersOfEqualCoordinates = null;
                    return true;
                }
                
                flagOfFirstIteration = true;
                buffer = coordinates[firstCoordFirstTunnel];
            }

            if (flagOfFirstIteration)
            {
                _numbersOfEqualCoordinates = null;
                return true;
            }
            if (j != 3) 
                AssignNewCoordinate(
                    coordinates, 
                    firstCoordFirstTunnel, 
                    originalCoord);
        }
        _numbersOfEqualCoordinates = null;
        return false;
    }

    private int g = 0;
    private bool CheckIntersects(Coordinate[] coordinates, int firstCoordFirstTunnel, int secondCoordFirstTunnel)
    {
        for (int i = 0; i < coordinates.Length - 1; i++)
        {
            g++;
            if (g == 23) GeoJsonFileService.GeoJsonFileService.WriteGeometryToFile(new LinearRing(coordinates),
                "C:\\Users\\Данил\\Downloads\\Telegram Desktop\\test" + g +".geojson", true);
            g++;
            if (g == 5)
                Console.WriteLine("er");
            if (i != firstCoordFirstTunnel)
            {
                var intersectionType = _intersector.GetIntersection(coordinates[i], coordinates[i + 1], coordinates[firstCoordFirstTunnel],
                    coordinates[secondCoordFirstTunnel]);
                var intersection = intersectionType.Item1;
                var intersectionPoint = intersectionType.Item2;
                bool flag = false;
                if (intersection != IntersectionType.NoIntersection && intersection != IntersectionType.Outside)
                {
                    if (IntersectionType.Overlay <= intersection && IntersectionType.Equals >= intersection)
                        return true;
                    if (IntersectionType.Inner == intersection || IntersectionType.TyShaped == intersection)
                        return true;
                    foreach (var number in _numbersOfEqualCoordinates)
                    {
                        if (coordinates[number].Equals2D(intersectionPoint, 1e-9))
                            flag = true;
                    }

                    if (!flag)
                    {
                        
                        Console.WriteLine("error2");
                        Console.WriteLine(g);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void FillNumbersOfEqualCoordinates(Coordinate[] coordinates, int numberOfCoordinate)
    {
        for (int i = 0; i < coordinates.Length; i++)
        {
            if (coordinates[i].Equals(coordinates[numberOfCoordinate]))
                _numbersOfEqualCoordinates!.AddFirst(i);
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
}