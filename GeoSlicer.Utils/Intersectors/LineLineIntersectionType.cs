using System;

namespace GeoSlicer.Utils.Intersectors;

[Flags]
public enum LineLineIntersectionType
{
    Inner = 1,
    Corner = 2,
    TyShaped = 4,
    Overlay = 8,
    Part = 16,
    Contains = 32,
    Equals = 64,
    Outside = 128,
    Extension = 256,
    NoIntersection = 512
}