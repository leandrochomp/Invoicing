using FluentValidation;

namespace Shared.Features.Client.Validators;

public class ClientValidator : AbstractValidator<Entities.Client>
{
    public ClientValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(c => c.CompanyName)
            .MaximumLength(200)
            .When(c => !string.IsNullOrWhiteSpace(c.CompanyName));

        RuleFor(c => c.Address)
            .MaximumLength(500);

        RuleFor(c => c.PhoneNumber)
            .MaximumLength(20)
            .Matches(@"^\+?[0-9\s\-]{7,20}$")
            .When(c => !string.IsNullOrWhiteSpace(c.PhoneNumber));
    }
}
