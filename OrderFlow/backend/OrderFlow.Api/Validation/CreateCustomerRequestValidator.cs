using FluentValidation;
using OrderFlow.Api.Contracts.Requests;
using OrderFlow.Domain.Enums;

namespace OrderFlow.Api.Validation;

public sealed class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Tier)
            .NotEmpty()
            .Must(tier => Enum.TryParse<CustomerTier>(tier, ignoreCase: true, out _))
            .WithMessage($"Tier must be one of: {string.Join(", ", Enum.GetNames<CustomerTier>())}.");
    }
}
