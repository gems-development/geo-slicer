using System;

namespace GeoSlicer.Utils.Intersectors;

[Flags]
public enum AreaAreaIntersectionType
{
    Outside = 1,
    Inside = 2
}