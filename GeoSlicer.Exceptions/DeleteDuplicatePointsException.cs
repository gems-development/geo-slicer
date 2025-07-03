using System.Runtime.Serialization;

namespace GeoSlicer.Exceptions;

public class DeleteDuplicatePointsException : Exception
{
    public DeleteDuplicatePointsException()
    {
    }

    protected DeleteDuplicatePointsException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public DeleteDuplicatePointsException(string? message) : base(message)
    {
    }

    public DeleteDuplicatePointsException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}