namespace Spike.Hub.Errors;

public class ErrorException : Exception
{
    public ErrorType Type { get; set; }

    public ErrorException(ErrorType type, string message) : base(message)
    {
        Type = type;
    }
}