using System;

namespace GeoSlicer.Utils.Intersectors;

[Flags]
public enum AreaIntersectionType
{
    Outside = 0,
    PartlyInside = 1,
    Overlay = 2,
    Inside = 3
}