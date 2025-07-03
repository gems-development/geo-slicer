using System.Runtime.Serialization;

namespace GeoSlicer.Exceptions;

public class GeometryIntersectionException : Exception
{
    public GeometryIntersectionException()
    {
    }

    public GeometryIntersectionException(string? message) : base(message)
    {
    }

    public GeometryIntersectionException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected GeometryIntersectionException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}