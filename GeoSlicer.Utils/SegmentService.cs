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

        return new LinearRing(res);
    }

    private static double? CalculatePhiFromZeroTo2Pi(double x, double y)
    {
        return x switch
        {
            > 0 when y >= 0 => Math.Atan(y / x),
            > 0 when y < 0 => Math.Atan(y / x) + 2 * Math.PI,
            < 0 => Math.Atan(y / x) + Math.PI,
            0 when y > 0 => Math.PI / 2,
            0 when y < 0 => 3 * Math.PI / 2,
            0 when y == 0 => null,
            _ => null
        };
    }

    private static double? CalculatePhiFromMinusPiToPlusPi(double x, double y)
    {
        return x switch
        {
            > 0 => Math.Atan(y / x),
            < 0 when y >= 0 => Math.Atan(y / x) + Math.PI,
            < 0 when y < 0 => Math.Atan(y / x) - Math.PI,
            0 when y > 0 => Math.PI / 2,
            0 when y < 0 => -Math.PI / 2,
            0 when y == 0 => null,
            _ => null
        };
    }

    // todo поработать в каноническом виде
    public static bool InsideTheAngle(
        Coordinate vectorPointA1,
        Coordinate vectorPointA2,
        Coordinate anglePointB1,
        Coordinate anglePointB2,
        Coordinate anglePointB3)
    {
        var vectorB1 = new Coordinate(anglePointB3.X - anglePointB2.X,
            anglePointB3.Y - anglePointB2.Y);
        var phiB1 = CalculatePhiFromMinusPiToPlusPi(vectorB1.X, vectorB1.Y);
        if (phiB1 == null) return true;
        const int sign = -1;
        var rotatedVectorAx = (vectorPointA2.X - vectorPointA1.X) * Math.Cos(sign * (double)phiB1) -
                              (vectorPointA2.Y - vectorPointA1.Y) * Math.Sin(sign * (double)phiB1);
        var rotatedVectorAy = (vectorPointA2.X - vectorPointA1.X) * Math.Sin(sign * (double)phiB1) +
                              (vectorPointA2.Y - vectorPointA1.Y) * Math.Cos(sign * (double)phiB1);
        var phiA = CalculatePhiFromZeroTo2Pi(rotatedVectorAx, rotatedVectorAy);
        var rotatedVectorB2X = (anglePointB1.X - anglePointB2.X) * Math.Cos(sign * (double)phiB1) -
                               (anglePointB1.Y - anglePointB2.Y) * Math.Sin(sign * (double)phiB1);
        var rotatedVectorB2Y = (anglePointB1.X - anglePointB2.X) * Math.Sin(sign * (double)phiB1) +
                               (anglePointB1.Y - anglePointB2.Y) * Math.Cos(sign * (double)phiB1);
        var phiB2 = CalculatePhiFromZeroTo2Pi(rotatedVectorB2X, rotatedVectorB2Y);
        return phiA < phiB2;
    }
}