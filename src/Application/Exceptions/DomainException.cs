namespace Spike.Application.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}