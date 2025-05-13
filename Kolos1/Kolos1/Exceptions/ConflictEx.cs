namespace Kolos1.Exceptions;

public class ConflictEx : Exception
{
    public ConflictEx()
    {
    }

    public ConflictEx(string? message) : base(message)
    {
    }

    public ConflictEx(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}