namespace API.Exceptions
{
    /// <summary>Indicates that a requested resource does not exist.</summary>
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string entityName, object id)
            : base($"{entityName} with ID {id} not found.")
        {
        }

        public EntityNotFoundException(string message)
            : base(message)
        {
        }
    }

    /// <summary>Indicates invalid parameters for an API operation.</summary>
    public class InvalidOperationApiException : Exception
    {
        public InvalidOperationApiException(string message)
            : base(message)
        {
        }
    }

}
