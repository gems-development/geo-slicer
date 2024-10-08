﻿using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.PolygonClippingAlghorithm;

public class CoordinateSupport : Coordinate
{
    public PointType Type { get; set; }
    public LinkedListNode<CoordinateSupport>? Coord { get; set; }

    public CoordinateSupport(double x, double y, LinkedListNode<CoordinateSupport>? coord = null, bool b = false, PointType type = PointType.Useless) : base(x, y)
    {
        Coord = coord;
        Type = type;
    }

    public CoordinateSupport(Coordinate coord)
    {
        Coord = null;
        Type = PointType.Useless;
        X = coord.X;
        Y = coord.Y;
    }
}
