namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails;
//A - северо-восток
//B - север
//C - северо-запад
//D - запад
//E - юго-запад
//F - Юг
//G - Юго-восток
//H - Восток
//ABC - объединение северо-запада, севера, севера-востока. Остальные аналогично.
public enum PartitioningZones
{ 
    A, B, C, D, E, F, G, H, ABC, CDE, EFG, AHG
}