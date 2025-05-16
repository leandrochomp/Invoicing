using FluentValidation;

namespace Shared.Features.Payment.Validators;

public class PaymentValidator : AbstractValidator<Entities.Payment>
{
    public PaymentValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty();

        RuleFor(x => x.AmountPaid)
            .GreaterThan(0);

        RuleFor(x => x.PaymentDate)
            .NotEmpty();

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.ReferenceNumber));

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
