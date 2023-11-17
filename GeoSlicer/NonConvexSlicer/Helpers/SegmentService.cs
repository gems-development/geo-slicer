using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace GeoSlicer.NonConvexSlicer.Helpers;
public static class SegmentService
{
    public static double VectorProduct
        (Coordinate firstVec,
            Coordinate secondVec,
            double epsilon = 1e-9)
    {
        var product = firstVec.X * secondVec.Y - secondVec.X * firstVec.Y;

        if (Math.Abs(product) < epsilon)
        {
            return 0;
        }

        return product;
    }

    public static bool IsIntersectionOfSegments(
        Coordinate firstSegmentPointA,
        Coordinate firstSegmentPointB,
        Coordinate secondSegmentPointC,
        Coordinate secondSegmentPointD)
    {
        LineIntersector lineIntersector = new RobustLineIntersector();
        lineIntersector.ComputeIntersection(firstSegmentPointA, firstSegmentPointB, secondSegmentPointC,
            secondSegmentPointD);
        return lineIntersector.IsInteriorIntersection();
    }

    public static bool HasIntersection(CoordinatePCN[] ring, CoordinatePCN coordCurrent, CoordinatePCN coordNext)
    {
        if (coordCurrent.Equals2D(coordNext)) return false;
        if (coordCurrent.PL == coordNext.C) return true;
        var index = coordCurrent.C;
        while (ring[index].NL != coordCurrent.C)
        {
            var firstCoord = ring[index];
            var secondCoord = ring[firstCoord.NL];
            if (IsIntersectionOfSegments(coordCurrent, coordNext, firstCoord, secondCoord))
            {
                return true;
            }

            index = secondCoord.C;
        }

        return IsIntersectionOfSegments(coordCurrent, coordNext, ring[index], coordCurrent);
    }

    public static LinearRing IgnoreInnerPointsOfSegment(LinearRing ring)
    {
        var array = new Coordinate[ring.Count - 1];
        var j = 0;
        if (!IsIntersectionOfSegments(
                ring.Coordinates[ring.Count - 2],
                ring.Coordinates[1],
                ring.Coordinates[0],
                ring.Coordinates[1])
            )
        {
            array[j] = ring.Coordinates[0];
            j++;
        }
        for (var i = 1; i < ring.Count - 1; i++)
        {
            if (!IsIntersectionOfSegments(
                ring.Coordinates[i - 1],
                ring.Coordinates[i + 1],
                ring.Coordinates[i],
                ring.Coordinates[i + 1]))
            {
                array[j] = ring.Coordinates[i];
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

    private static double? CalculatePhiFromZeroTo2PI(double x, double y)
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
    
    private static double? CalculatePhiFromMinusPIToPlusPI(double x, double y)
    {
        return x switch
        {
            > 0  => Math.Atan(y / x),
            < 0 when y >= 0 => Math.Atan(y / x) + Math.PI,
            < 0 when y < 0 => Math.Atan(y / x) - Math.PI,
            0 when y > 0 => Math.PI / 2,
            0 when y < 0 => - Math.PI / 2,
            0 when y == 0 => null,
            _ => null
        };
    }
    
    public static bool InsideTheAngle(
        Coordinate vectorPointA1,
        Coordinate vectorPointA2,
        Coordinate anglePointB1,
        Coordinate anglePointB2,
        Coordinate anglePointB3)
    {
        var vectorB1 = new Coordinate(anglePointB3.X - anglePointB2.X,
            anglePointB3.Y - anglePointB2.Y);
        var phiB1 = CalculatePhiFromMinusPIToPlusPI(vectorB1.X, vectorB1.Y);
        if (phiB1 == null) return true;
        var sign = -1;
        var rotatedVectorAX = (vectorPointA2.X - vectorPointA1.X) * Math.Cos(sign * (double)phiB1) - (vectorPointA2.Y - vectorPointA1.Y) * Math.Sin(sign * (double)phiB1);
        var rotatedVectorAY = (vectorPointA2.X - vectorPointA1.X) * Math.Sin(sign * (double)phiB1) + (vectorPointA2.Y - vectorPointA1.Y) * Math.Cos(sign * (double)phiB1);
        var phiA = CalculatePhiFromZeroTo2PI(rotatedVectorAX, rotatedVectorAY);
        var rotatedVectorB2X = (anglePointB1.X - anglePointB2.X) * Math.Cos(sign * (double)phiB1) - (anglePointB1.Y - anglePointB2.Y) * Math.Sin(sign * (double)phiB1);
        var rotatedVectorB2Y = (anglePointB1.X - anglePointB2.X) * Math.Sin(sign * (double)phiB1) + (anglePointB1.Y - anglePointB2.Y) * Math.Cos(sign * (double)phiB1);
        var phiB2 = CalculatePhiFromZeroTo2PI(rotatedVectorB2X, rotatedVectorB2Y);
        return phiA < phiB2;
    }
}
