using System.Collections.Generic;
using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors;
using NetTopologySuite.Geometries;
using static GeoSlicer.Utils.SegmentService;

namespace GeoSlicer.NonConvexSlicer.Helpers;

public class NonConvexSlicerHelper
{
    private const LinesIntersectionType SuitableLineLineIntersectionType = LinesIntersectionType.Inner | LinesIntersectionType.TyShaped |
                                                              LinesIntersectionType.Contains | LinesIntersectionType.Part |
                                                              LinesIntersectionType.Overlay;

    private const AreasIntersectionType SuitableAreaAreaIntersectionType = AreasIntersectionType.Inside;
    private readonly LinesIntersector _linesIntersector;
    private readonly AreasIntersector _areasIntersector = new();
    private readonly LineService _lineService;

    public NonConvexSlicerHelper(
        LinesIntersector linesIntersector, 
        LineService lineService)
    {
        _linesIntersector = linesIntersector;
        _lineService = lineService;
    }

    public List<CoordinatePcn> GetSpecialPoints(LinearRing ring)
    {
        var list = new List<CoordinatePcn>(ring.Count - 4);
        var coordinates = ring.Coordinates;
        for (var i = 0; i < coordinates.Length - 1; ++i)
        {
            if (_lineService.VectorProduct(
                    coordinates[i].X - coordinates[(i - 1 + coordinates.Length - 1) % (coordinates.Length - 1)].X,
                    coordinates[i].Y - coordinates[(i - 1 + coordinates.Length - 1) % (coordinates.Length - 1)].Y, 
                    coordinates[(i + 1) % (coordinates.Length - 1)].X - coordinates[i].X,
                    coordinates[(i + 1) % (coordinates.Length - 1)].Y - coordinates[i].Y
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
            if (_areasIntersector.CheckIntersection(SuitableAreaAreaIntersectionType,
                    coordCurrent, coordNext, firstCoord, secondCoord))
            {
                if (_linesIntersector.CheckIntersection(SuitableLineLineIntersectionType,
                        coordCurrent, coordNext, firstCoord, secondCoord))
                {
                    return true;
                }
            }

            index = secondCoord.C;
        }

        return _areasIntersector.CheckIntersection(SuitableAreaAreaIntersectionType,
                   coordCurrent, coordNext, ring[index], coordCurrent) &&
            _linesIntersector.CheckIntersection(SuitableLineLineIntersectionType,
            coordCurrent, coordNext, ring[index], coordCurrent);
    }
}