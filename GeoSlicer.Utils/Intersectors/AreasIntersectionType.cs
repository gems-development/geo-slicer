using System;

namespace GeoSlicer.Utils.Intersectors;

[Flags]
public enum AreasIntersectionType
{
    Outside = 1,
    Inside = 2
}