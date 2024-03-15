using System;

namespace GeoSlicer.Utils.Intersectors;

[Flags]
public enum LineAreaIntersectionType
{
    Outside = 1,
    PartlyInside = 2,
    Overlay = 3,
    Inside = 4
}