using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.PolygonClippingAlgorithm;

public class CoordinateSupport : Coordinate
{
    public PointType Type { get; set; }
    
    /// <summary>
    /// Ссылка на эту же координату в другом листе в алгоритме пересечения WeilerAtherton (clipped &lt;-&gt; cutting)
    /// </summary>
    public LinkedListNode<CoordinateSupport>? Ref { get; set; }

    public CoordinateSupport(Coordinate coord)
    {
        Ref = null;
        Type = PointType.Useless;
        X = coord.X;
        Y = coord.Y;
    }
}
