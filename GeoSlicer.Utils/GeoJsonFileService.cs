using System.IO;
using NetTopologySuite.IO;

namespace GeoSlicer.Utils;

public class GeoJsonFileService
{
    private readonly GeoJsonWriter _writer = new();

    private readonly GeoJsonReader _reader = new();

    private readonly string _fullRootName;

    public GeoJsonFileService(string rootName = "geo-slicer")
    {
        _fullRootName = GetFullRoot(rootName);
    }

    public void WriteGeometryToFile<T>(T geometry, string path, bool flagOfGlobalPath = false) where T : class
    {
        if (!flagOfGlobalPath)
            path = GetGlobalPath(path);
        Directory.CreateDirectory(Directory.GetParent(path)!.FullName);
        string geoJson = _writer.Write(geometry);
        File.WriteAllText(path, geoJson);
    }

    public T ReadGeometryFromFile<T>(string path, bool flagOfGlobalPath = false) where T : class
    {
        if (!flagOfGlobalPath)
            path = GetGlobalPath(path);
        string geoJson = File.ReadAllText(path);

        var geometry = _reader.Read<T>(geoJson);

        return geometry;
    }

    private string GetGlobalPath(string path)
    {
        return Path.Combine(_fullRootName, path);
    }
    
    private string GetFullRoot(string rootName)
    {
        DirectoryInfo directoryInfo = new(Directory.GetCurrentDirectory());
        while (directoryInfo.Name != rootName)
        {
            directoryInfo = directoryInfo.Parent!;
        }

        return directoryInfo.FullName;
    }
}