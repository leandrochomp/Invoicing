using FluentValidation;
using Shared.Features.Payments.Entities;

namespace Shared.Features.Payments.Validators;

public class PaymentValidator : AbstractValidator<Payment>
{
    public PaymentValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty();

        RuleFor(x => x.AmountPaid)
            .GreaterThan(0);

        RuleFor(x => x.PaymentDate)
            .NotEmpty();

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.ReferenceNumber));

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}