using System.Collections.Generic;
using GeoSlicer.Utils.BoundRing;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails.Structures;

internal class RingAndZones
{
    internal readonly LinkedListNode<BoundingRing> BoundRing;
    internal readonly List<Zones> Zones;

    internal RingAndZones(LinkedListNode<BoundingRing> boundRing, List<Zones> zones)
    {
        BoundRing = boundRing;
        Zones = zones;
    }
}