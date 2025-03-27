using NetTopologySuite.Geometries;

namespace GeoSlicer.DivideAndRuleSlicers;

public interface IDivisionIndexesGiver
{
    /// <summary>
    /// Записывает в out переменные индексы точек, по которым следует разрезать полигон
    /// </summary>
    void GetIndexes(LinearRing ring, out int first, out int second);
    
}