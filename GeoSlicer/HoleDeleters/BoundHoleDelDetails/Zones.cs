namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails;

/// <summary>
/// A - северо-восток,
/// B - север,
/// C - северо-запад,
/// D - запад,
/// E - юго-запад,
/// F - Юг,
/// G - Юго-восток,
/// H - Восток,
/// ABC - объединение северо-запада, севера, севера-востока. Остальные аналогично.
/// </summary>
public enum Zones
{ 
    A, B, C, D, E, F, G, H, Abc, Cde, Efg, Ahg
}