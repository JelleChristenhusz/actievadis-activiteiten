namespace Application.Exceptions;

/// <summary>Indicates a conflict with existing application data.</summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
    public ConflictException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>Contains user-facing validation errors for an application operation.</summary>
public sealed class ApplicationValidationException(string message, IEnumerable<string> errors)
    : Exception(message)
{
    public IReadOnlyCollection<string> Errors { get; } = errors.ToArray();
}
