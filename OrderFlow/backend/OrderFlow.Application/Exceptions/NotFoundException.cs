namespace OrderFlow.Application.Exceptions;

/// <summary>Thrown when a requested entity does not exist. Api layer maps this to HTTP 404.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with id '{key}' was not found.")
    {
    }
}
