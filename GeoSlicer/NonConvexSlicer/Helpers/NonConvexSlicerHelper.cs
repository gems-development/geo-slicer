using System.Collections.Generic;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;
using static GeoSlicer.Utils.SegmentService;
using LineIntersector = GeoSlicer.Utils.Intersectors.LineIntersector;

namespace GeoSlicer.NonConvexSlicer.Helpers;

public class NonConvexSlicerHelper
{
    private const IntersectionType SuitableIntersectionType = IntersectionType.Inner | IntersectionType.TyShaped |
                                                              IntersectionType.Contains | IntersectionType.Part |
                                                              IntersectionType.Overlay;

    private readonly LineIntersector _lineIntersector;
    private readonly SegmentService _segmentService;
    private readonly TraverseDirection _traverseDirection;

    public NonConvexSlicerHelper(
        double epsilon = 1E-5, 
        LineIntersector? lineIntersector = null, 
        SegmentService? segmentService = null,
        TraverseDirection? traverseDirection = null)
    {
        _lineIntersector = lineIntersector ?? new(new EpsilonCoordinateComparator(epsilon), epsilon);
        _segmentService = segmentService ?? new SegmentService(epsilon);
        _traverseDirection = traverseDirection ?? new TraverseDirection(_segmentService);
    }

    public List<CoordinatePCN> GetSpecialPoints(LinearRing ring)
    {
        var list = new List<CoordinatePCN>();
        var clockwise = _traverseDirection.IsClockwiseBypass(ring);
        var coordinates = ring.Coordinates;
        for (var i = 0; i < coordinates.Length - 1; ++i)
        {
            if (_segmentService.VectorProduct(
                    new Coordinate(
                        coordinates[i].X -
                        coordinates[(i - 1 + coordinates.Length - 1) % (coordinates.Length - 1)].X,
                        coordinates[i].Y -
                        coordinates[(i - 1 + coordinates.Length - 1) % (coordinates.Length - 1)].Y),
                    new Coordinate(coordinates[(i + 1) % (coordinates.Length - 1)].X - coordinates[i].X,
                        coordinates[(i + 1) % (coordinates.Length - 1)].Y - coordinates[i].Y)
                ) >= 0 == clockwise)
            {
                list.Add(new CoordinatePCN(coordinates[i].X, coordinates[i].Y, c: i));
            }
        }

        return list;
    }

    private bool FirstPointCanSeeSecond(CoordinatePCN[] ring, CoordinatePCN pointA, CoordinatePCN pointB)
    {
        return pointA.Equals2D(pointB) ||
               InsideTheAngle(pointA, pointB, ring[pointA.NL],
                   pointA, ring[pointA.PL]) ||
               (ring[pointA.NL].Equals2D(pointB) && pointA.NL == pointB.C) ||
               (ring[pointA.PL].Equals2D(pointB) && pointA.PL == pointB.C);
    }

    public bool CanSeeEachOther(CoordinatePCN[] ring, CoordinatePCN pointA, CoordinatePCN pointB)
    {
        return FirstPointCanSeeSecond(ring, pointA, pointB) && FirstPointCanSeeSecond(ring, pointB, pointA);
    }

    public bool HasIntersection(CoordinatePCN[] ring, CoordinatePCN coordCurrent, CoordinatePCN coordNext)
    {
        if (coordCurrent.Equals2D(coordNext)) return false;
        if (coordCurrent.PL == coordNext.C) return true;
        var index = coordCurrent.C;
        while (ring[index].NL != coordCurrent.C)
        {
            var firstCoord = ring[index];
            var secondCoord = ring[firstCoord.NL];
            if (_lineIntersector.CheckIntersection(SuitableIntersectionType,
                    coordCurrent, coordNext, firstCoord, secondCoord))
            {
                return true;
            }

            index = secondCoord.C;
        }

        return _lineIntersector.CheckIntersection(SuitableIntersectionType,
            coordCurrent, coordNext, ring[index], coordCurrent);
    }
}