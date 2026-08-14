namespace OrderFlow.Application.Exceptions;

/// <summary>Thrown when a request would violate a business rule. Api layer maps this to HTTP 400.</summary>
public sealed class BusinessRuleViolationException : Exception
{
    public BusinessRuleViolationException(string message) : base(message)
    {
    }
}
