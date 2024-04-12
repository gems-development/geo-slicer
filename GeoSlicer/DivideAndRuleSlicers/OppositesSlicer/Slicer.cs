using System;
using System.Collections.Generic;
using GeoSlicer.Utils.Intersectors;
using NetTopologySuite.Geometries;

namespace GeoSlicer.DivideAndRuleSlicers.OppositesSlicer;

public class Slicer
{
    private readonly OppositesSlicerHelper _oppositesSlicerHelper;
    private readonly LinesIntersector _linesIntersector;

    public Slicer(OppositesSlicerHelper oppositesSlicerHelper, LinesIntersector linesIntersector)
    {
        _oppositesSlicerHelper = oppositesSlicerHelper;
        _linesIntersector = linesIntersector;
    }


    public IEnumerable<MultiPolygon> Slice(Polygon polygon)
    {
        int oppositesIndex = Utils.GetNearestOppositesInner(polygon.Shell);
        Console.WriteLine(oppositesIndex);
        throw new NotImplementedException();
    }
    
    // todo Сделать соединение без создания новых точек
    // todo Вынести в отдельный класс, мб пригодится извне
    private IEnumerable<Polygon> SliceByLine(Polygon polygon, Coordinate a, Coordinate b)
    {
        
        // Мб не пересечение, а направление от линии
    }
}