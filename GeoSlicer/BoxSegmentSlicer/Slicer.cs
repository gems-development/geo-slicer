using System;
using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Exceptions;
using GeoSlicer.Utils;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace GeoSlicer.BoxSegmentSlicer;

public class Slicer
{
    private readonly int _maxPointsCount;
    private readonly double _epsilon;
    private readonly bool _skipDuplicatePointsDeletion;
    private readonly bool _ignoreUncutGeometry;
    private readonly List<Geometry> _uncutGeometries;

    public IReadOnlyList<Geometry> UncutGeometries
    {
        get
        {
            if (!_ignoreUncutGeometry)
                throw new ApplicationException("the algorithm is configured to not ignore non-cut geometries");
            return _uncutGeometries;
        }
    }
    

    public Slicer
        (int maxPointsCount,
            bool skipDuplicatePointsDeletion = false, bool ignoreUncutGeometry = false, double epsilon = 1e-12)
    {
        if (maxPointsCount < 5)
            throw new ArgumentException("maxPointsCount must >= 5");
        _maxPointsCount = maxPointsCount;
        _epsilon = epsilon;
        _skipDuplicatePointsDeletion = skipDuplicatePointsDeletion;
        _ignoreUncutGeometry = ignoreUncutGeometry;
        _uncutGeometries = new List<Geometry>();
    }
    
    public List<Polygon> Slice(Polygon data)
    {
        
        var polygon = new Polygon(data.Shell, data.Holes){SRID = 0};
        polygon = _skipDuplicatePointsDeletion ? polygon : DuplicatePointsDeleter.HandlePolygon(polygon);
        
        if (!polygon.IsValid)
            throw new ArgumentException("data is invalid");
        
        _uncutGeometries.Clear();
        return SliceRecursive(polygon)
            .Select(a => new Polygon(a.Shell, a.Holes, data.Factory){SRID = data.SRID})
            .ToList();
    }

    private IEnumerable<Polygon> SliceRecursive(Polygon polygon)
    {
        if (polygon.IsEmpty)
            throw new ArgumentException("polygon is empty");
        
        Envelope envelope = polygon.EnvelopeInternal;
        
        if (envelope.Area == 0)
            throw new ArgumentException("area of polygon bounding box is zero");

        var pointsCount = polygon.NumPoints;
        
        if (pointsCount <= _maxPointsCount)
        {
            return new[] { polygon };
        }
        
        var cuttingOrdinateValue = FindCuttingOrdinateValue(envelope, FindCuttingRing(polygon, pointsCount));
        CloseOrdinateValueChecker.Fix(envelope, ref cuttingOrdinateValue, _epsilon);

        try
        {
            return NtsPolygonSlicer
                .SliceByOrdinateValue(cuttingOrdinateValue, polygon)
                .Select(p => _skipDuplicatePointsDeletion ? p : DuplicatePointsDeleter.HandlePolygon(p))
                .Select(SliceRecursive)
                .SelectMany(x => x);
        }
        catch (Exception ex) when (ex is DeleteDuplicatePointsException or GeometryIntersectionException)
        {
            if (!_ignoreUncutGeometry) throw;
            _uncutGeometries.Add(polygon);
            return new[] { polygon };
        }
    }

    private LinearRing FindCuttingRing(Polygon polygon, int pointsCount)
    {
        if (pointsCount < 2 * polygon.Shell.NumPoints)
            return polygon.Shell;

        LinearRing currentRing = null!;
        double area = 0;

        foreach (var hole in polygon.Holes)
        {
            double currentArea = Area.OfRing(hole.CoordinateSequence);
            if (currentArea < area) continue;
            area = currentArea;
            currentRing = hole;
        }
        
        return currentRing;
    }

    private (double value, Ordinate ordinate) FindCuttingOrdinateValue(Envelope polygonEnvelope, LinearRing cuttingRing)
    {
        Coordinate center = polygonEnvelope.Centre;
        Ordinate sliceOrdinate = polygonEnvelope.Width > polygonEnvelope.Height ? Ordinate.X : Ordinate.Y;
        double ordinateCenter = sliceOrdinate == Ordinate.X ? center.X : center.Y;
        
        double value = Double.NaN;
        double epsilon = Double.MaxValue;
        foreach (var coordinate in cuttingRing.Coordinates)
        {
            var currentValue = sliceOrdinate == Ordinate.X ? coordinate.X : coordinate.Y;
            var currentEpsilon = Math.Abs(currentValue - ordinateCenter);
            if (epsilon <= currentEpsilon) continue;
            value = currentValue;
            epsilon = currentEpsilon;
        }

        if (double.IsNaN(value))
        {
            throw new ApplicationException("value must not be Double.NaN");
        }
        return (value, sliceOrdinate);
    }

}