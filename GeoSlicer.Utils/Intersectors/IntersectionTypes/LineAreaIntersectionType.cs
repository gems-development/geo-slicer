using System;

namespace GeoSlicer.Utils.Intersectors.IntersectionTypes;

[Flags]
public enum LineAreaIntersectionType
{
    Outside = 1,
    PartlyInside = 2,
    Overlay = 3,
    Inside = 4
}