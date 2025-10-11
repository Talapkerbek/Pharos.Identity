namespace Pharos.Identity.Infra.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string message, string details) : base(message)
    {
        Details = details;
    }

    public string? Details { get; }
}