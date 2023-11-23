using System;
using System.IO;
using NetTopologySuite.IO;

namespace GeoJsonFileService;

public static class GeoJsonFileService
{
    private static readonly GeoJsonWriter Writer =
        new GeoJsonWriter();

    private static readonly GeoJsonReader Reader =
        new GeoJsonReader();

    public static void WriteGeometryToFile<T>(T geometry, String path, bool flagOfGlobalPath = false) where T : class
    {
        if (!flagOfGlobalPath)
            path = GetGlobalPath(path);
        Directory.CreateDirectory(Directory.GetParent(path)!.FullName);
        string geoJson = Writer.Write(geometry);
        File.WriteAllText(path, geoJson);
    }

    public static T ReadGeometryFromFile<T>(String path, bool flagOfGlobalPath = false) where T : class
    {
        if (!flagOfGlobalPath)
            path = GetGlobalPath(path);
        string geoJson = File.ReadAllText(path);

        var geometry = Reader.Read<T>(geoJson);

        return geometry;
    }

    private static String GetGlobalPath(String path)
    {
        string workingDirectory = Directory.GetCurrentDirectory();
        var root = Directory.GetParent(workingDirectory)?.Parent?.Parent?.FullName;
        return Path.Combine(root!, path);
    }
}