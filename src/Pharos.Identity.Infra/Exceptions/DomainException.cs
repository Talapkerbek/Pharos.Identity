namespace Pharos.Identity.Infra.Exceptions;

public class DomainException : Exception
{
    public int StatusCode { get; init; }
    public DomainException(string message,  int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}