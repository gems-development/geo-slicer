using System.Collections.Generic;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using NetTopologySuite.Geometries;
using static GeoSlicer.Utils.SegmentService;
using LineIntersector = GeoSlicer.Utils.Intersectors.LineIntersector;

namespace GeoSlicer.NonConvexSlicer.Helpers;

public class NonConvexSlicerHelper
{
    private const LineIntersectionType SuitableIntersectionType = LineIntersectionType.Inner | LineIntersectionType.TyShaped |
                                                              LineIntersectionType.Contains | LineIntersectionType.Part |
                                                              LineIntersectionType.Overlay;

    private readonly LineIntersector _lineIntersector;
    private readonly LineService _lineService;

    public NonConvexSlicerHelper(
        LineIntersector lineIntersector, 
        LineService lineService)
    {
        _lineIntersector = lineIntersector;
        _lineService = lineService;
    }

    public List<CoordinatePcn> GetSpecialPoints(LinearRing ring)
    {
        var list = new List<CoordinatePcn>(ring.Count - 4);
        var coordinates = ring.Coordinates;
        for (var i = 0; i < coordinates.Length - 1; ++i)
        {
            if (_lineService.VectorProduct(
                    new Coordinate(
                        coordinates[i].X -
                        coordinates[(i - 1 + coordinates.Length - 1) % (coordinates.Length - 1)].X,
                        coordinates[i].Y -
                        coordinates[(i - 1 + coordinates.Length - 1) % (coordinates.Length - 1)].Y),
                    new Coordinate(coordinates[(i + 1) % (coordinates.Length - 1)].X - coordinates[i].X,
                        coordinates[(i + 1) % (coordinates.Length - 1)].Y - coordinates[i].Y)
                ) >= 0)
            {
                list.Add(new CoordinatePcn(coordinates[i].X, coordinates[i].Y, c: i));
            }
        }

        return list;
    }

    private bool FirstPointCanSeeSecond(CoordinatePcn[] ring, CoordinatePcn pointA, CoordinatePcn pointB)
    {
        return pointA.Equals2D(pointB) ||
               InsideTheAngle(pointA, pointB, ring[pointA.Nl],
                   pointA, ring[pointA.Pl]) ||
               (ring[pointA.Nl].Equals2D(pointB) && pointA.Nl == pointB.C) ||
               (ring[pointA.Pl].Equals2D(pointB) && pointA.Pl == pointB.C);
    }

    public bool CanSeeEachOther(CoordinatePcn[] ring, CoordinatePcn pointA, CoordinatePcn pointB)
    {
        return FirstPointCanSeeSecond(ring, pointA, pointB) && FirstPointCanSeeSecond(ring, pointB, pointA);
    }

    public bool HasIntersection(CoordinatePcn[] ring, CoordinatePcn coordCurrent, CoordinatePcn coordNext)
    {
        if (coordCurrent.Equals2D(coordNext)) return false;
        if (coordCurrent.Pl == coordNext.C) return true;
        var index = coordCurrent.C;
        while (ring[index].Nl != coordCurrent.C)
        {
            var firstCoord = ring[index];
            var secondCoord = ring[firstCoord.Nl];
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