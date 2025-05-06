using System;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils;

public class SegmentService
{
    private readonly LineService _lineService;

    public SegmentService(LineService lineService)
    {
        _lineService = lineService;
    }

    /// <summary>
    /// Удаляет точки, что лежат на прямой между соседними (точки, образующие угол в 180)
    /// </summary>
    public LinearRing IgnoreInnerPointsOfSegment(LinearRing ring)
    {
        if (ring.Count < 4)
        {
            return ring;
        }

        var array = new Coordinate[ring.Count - 1];
        var coordinates = ring.Coordinates;
        var j = 0;
        if (!_lineService.IsCoordinateInSegment(
                coordinates[0],
                coordinates[ring.Count - 2],
                coordinates[1]))

        {
            array[j] = coordinates[0];
            j++;
        }

        for (var i = 1; i < coordinates.Length - 1; i++)
        {
            if (!_lineService.IsCoordinateAtLine(
                    coordinates[i],
                    coordinates[i - 1],
                    coordinates[i + 1]))
            {
                array[j] = coordinates[i];
                j++;
            }
        }

        var res = new Coordinate[j + 1];
        for (var i = 0; i < j; i++)
        {
            res[i] = array[i];
        }

        res[j] = res[0];

        return res.Length <= 3 ? ring : new LinearRing(res);
    }
}