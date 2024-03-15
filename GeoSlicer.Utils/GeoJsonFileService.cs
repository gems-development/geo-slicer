using System.IO;
using NetTopologySuite.IO;

namespace GeoSlicer.Utils;

public static class GeoJsonFileService
{
    private static readonly GeoJsonWriter Writer =
        new GeoJsonWriter();

    private static readonly GeoJsonReader Reader =
        new GeoJsonReader();

    private static readonly string RootName = "geo-slicer";
    
    private static string? _fullRootName;

    public static void WriteGeometryToFile<T>(T geometry, string path, bool flagOfGlobalPath = false) where T : class
    {
        if (!flagOfGlobalPath)
            path = GetGlobalPath(path);
        Directory.CreateDirectory(Directory.GetParent(path)!.FullName);
        string geoJson = Writer.Write(geometry);
        File.WriteAllText(path, geoJson);
    }

    public static T ReadGeometryFromFile<T>(string path, bool flagOfGlobalPath = false) where T : class
    {
        if (!flagOfGlobalPath)
            path = GetGlobalPath(path);
        string geoJson = File.ReadAllText(path);

        var geometry = Reader.Read<T>(geoJson);

        return geometry;
    }

    private static string GetGlobalPath(string path)
    {
        string root = GetRoot();
        return Path.Combine(root, path);
    }

    private static string GetRoot()
    {
        if (_fullRootName is null)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directoryInfo.Name != RootName)
            {
                directoryInfo = directoryInfo.Parent!;
            }

            _fullRootName = directoryInfo.FullName;
        }

        return _fullRootName;
    }
}