namespace OrderFlow.Domain.Exceptions;

/// <summary>Base type for violations of core business rules raised from within the Domain layer.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}
