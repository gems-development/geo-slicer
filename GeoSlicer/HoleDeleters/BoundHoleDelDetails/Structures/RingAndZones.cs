using System.Collections.Generic;
using GeoSlicer.Utils.BoundRing;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails.Structures;

internal class RingAndZones
{
    internal readonly LinkedListNode<BoundingRing> BoundRing;
    internal readonly List<SeparatingZones> Zones;

    internal RingAndZones(LinkedListNode<BoundingRing> boundRing, List<SeparatingZones> zones)
    {
        BoundRing = boundRing;
        Zones = zones;
    }
}