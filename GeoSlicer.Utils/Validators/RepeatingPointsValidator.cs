using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.Validators;

public class RepeatingPointsValidator
{
    private readonly ICoordinateComparator _coordinateComparator;

    public RepeatingPointsValidator(ICoordinateComparator coordinateComparator)
    {
        _coordinateComparator = coordinateComparator;
    }

    public string GetErrorsString(LineString lineString, bool isFull = false)
    {
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < lineString.Count - 1; i++)
        {
            if (_coordinateComparator.IsEquals(lineString[i], lineString[i + 1]))
            {
                stringBuilder.Append($"Equals points at {i} and {i + 1}\n");
                if (!isFull)
                {
                    return stringBuilder.ToString();
                }
            }
        }

        return stringBuilder.ToString();
    }

    public bool IsValid(LineString lineString, bool isFull = false)
    {
        if (string.IsNullOrEmpty(GetErrorsString(lineString, isFull)))
        {
            return true;
        }

        return false;
    }

    public T Fix<T>(T linear, Func<Coordinate[], T> creator) where T : LineString, new()
    {
        List<Coordinate> resultCoordinates = new List<Coordinate>(linear.Count) { linear.Coordinate };
        foreach (Coordinate coordinate in linear.Coordinates)
        {
            if (!_coordinateComparator.IsEquals(resultCoordinates.Last(), coordinate))
            {
                resultCoordinates.Add(coordinate);
            }
        }

        return creator.Invoke(resultCoordinates.ToArray());
    }
}