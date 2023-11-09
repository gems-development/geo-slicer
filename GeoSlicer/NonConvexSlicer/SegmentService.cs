using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace GeoSlicer.NonConvexSlicer;
public class SegmentService
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

    public static bool HasIntersection(CoordinatePCN[] ring, Coordinate coordCurrent, Coordinate coordNext)
    {
        if (coordCurrent.Equals2D(coordNext)) return false;
        var index = (int)coordCurrent.M;
        while (ring[index].NL != (int)coordCurrent.M)
        {
            var firstCoord = ring[index];
            var secondCoord = ring[firstCoord.NL];
            if (IsIntersectionOfSegments(coordCurrent, coordNext, firstCoord.ToCoordinate(),
                    secondCoord.ToCoordinate()))
            {
                return true;
            }

            index = secondCoord.C;
        }

        return IsIntersectionOfSegments(coordCurrent, coordNext, ring[index].ToCoordinate(),
            coordCurrent);
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
}
