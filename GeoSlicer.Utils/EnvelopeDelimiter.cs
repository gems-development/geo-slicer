using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils;

public static class EnvelopeDelimiter
{
    public static void Delimite(
        Envelope envelope,
        in (double value, Ordinate ordinate) ordinateValue,
        out Envelope leftEnvelope, out Envelope rightEnvelope)
    {
        // Создаем нахлест чтобы наверняка
        double minY = envelope.MinY - (envelope.MaxY - envelope.MinY) * 0.1;
        double maxY = envelope.MaxY + (envelope.MaxY - envelope.MinY) * 0.1;
        double minX = envelope.MinX - (envelope.MaxY - envelope.MinY) * 0.1;
        double maxX = envelope.MaxX + (envelope.MaxY - envelope.MinY) * 0.1;
        var extendEnvelope = new Envelope(minX, maxX, minY, maxY);
        
        if (ordinateValue.ordinate == Ordinate.X)
        {
            ChangeXInEnvelope(out leftEnvelope, out rightEnvelope, extendEnvelope, ordinateValue.value);
        }
        else
        {
            ChangeYInEnvelope(out leftEnvelope, out rightEnvelope, extendEnvelope, ordinateValue.value);
        }
    }

    private static void ChangeXInEnvelope(
        out Envelope envelope1, out Envelope envelope2, Envelope envelope, double value)
    {
        double minX = envelope.MinX;
        double maxX = envelope.MaxX;
        double minY = envelope.MinY;  
        double maxY = envelope.MaxY;
        
        envelope1 = new Envelope(minX, value, minY, maxY);
        
        minX = envelope.MinX;
        maxX = envelope.MaxX;
        minY = envelope.MinY;  
        maxY = envelope.MaxY;
        
        envelope2 = new Envelope(value, maxX, minY, maxY);
    }
    
    private static void ChangeYInEnvelope(
        out Envelope envelope1, out Envelope envelope2, Envelope envelope, double value)
    {
        double minX = envelope.MinX;
        double maxX = envelope.MaxX;
        double minY = envelope.MinY;  
        double maxY = envelope.MaxY;
        
        envelope1 = new Envelope(minX, maxX, minY, value);
        
        minX = envelope.MinX;
        maxX = envelope.MaxX;
        minY = envelope.MinY;  
        maxY = envelope.MaxY;
        
        envelope2 = new Envelope(minX, maxX, value, maxY);
    }
}