using System;

namespace GeoSlicer.Utils.Intersectors.IntersectionTypes;

[Flags]
public enum AreasIntersectionType
{
    Outside = 1,
    Inside = 2
}