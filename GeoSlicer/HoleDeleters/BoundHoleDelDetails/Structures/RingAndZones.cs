using System;
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

    private bool Equals(RingAndZones other)
    {
        return BoundRing.Equals(other.BoundRing) && Zones.Equals(other.Zones);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((RingAndZones)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BoundRing, Zones);
    }
}