using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.Validators;

/// <summary>
/// Проверяет, есть ли в геометрии идущие подряд повторяющиеся точки
/// </summary>
public class RepeatingPointsValidator
{
    private readonly ICoordinateComparator _coordinateComparator;

    public RepeatingPointsValidator(ICoordinateComparator coordinateComparator)
    {
        _coordinateComparator = coordinateComparator;
    }

    /// <summary>
    /// Ищет повторяющиеся точки, собирая их в сообщение об ошибке
    /// </summary>
    /// <param name="lineString">Проверяемая геометрия</param>
    /// <param name="isFull">Если true - собираем все ошибки. Иначе останавливаемся после нахождения первой</param>
    public string GetErrorsString(LineString lineString, bool isFull = false)
    {
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < lineString.Count - 1; i++)
        {
            if (_coordinateComparator.IsEquals(lineString[i], lineString[i + 1]))
            {
                stringBuilder.Append($"Equals points at {i} and {i + 1}\n: {lineString[i]}, {lineString[i + 1]}");
                if (!isFull)
                {
                    return stringBuilder.ToString();
                }
            }
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Проверяет, является ли последовательность координат корректной (не содержит она повторений)
    /// </summary>
    public bool IsValid(LineString lineString, bool isFull = false)
    {
        if (string.IsNullOrEmpty(GetErrorsString(lineString, isFull)))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Удаляет повторения в геометрии
    /// </summary>
    public T Fix<T>(T linear, Func<Coordinate[], T> creator) where T : LineString
    {
        List<Coordinate> resultCoordinates = new List<Coordinate>(linear.Count) { linear.Coordinate };
        for (int i = 0; i < linear.Coordinates.Length; i++)
        {
            Coordinate coordinate = linear.Coordinates[i];
            if (!_coordinateComparator.IsEquals(resultCoordinates.Last(), coordinate))
            {
                resultCoordinates.Add(coordinate);
            }
            else if (i == linear.Coordinates.Length - 1)
            {
                resultCoordinates[^1] = coordinate;
            }
        }

        return creator.Invoke(resultCoordinates.ToArray());
    }
}