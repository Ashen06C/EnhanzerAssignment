namespace Enhanzer.Assignment.Application.Common;

public sealed class AuthenticationFailedException : Exception
{
    public AuthenticationFailedException()
        : base("The supplied credentials were not accepted.")
    {
    }
}

public sealed class ExternalServiceException : Exception
{
    public ExternalServiceException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public sealed class ValidationException : Exception
{
    public ValidationException(IDictionary<string, string[]> errors)
        : base("The request is invalid.")
    {
        Errors = errors;
    }

    public IDictionary<string, string[]> Errors { get; }
}
