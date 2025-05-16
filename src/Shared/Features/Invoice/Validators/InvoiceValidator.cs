using FluentValidation;

namespace Shared.Features.Invoice.Validators;

public class InvoiceValidator : AbstractValidator<Entities.Invoice>
{
    public InvoiceValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty();

        RuleFor(x => x.InvoiceNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.IssueDate)
            .NotEmpty();

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.IssueDate)
            .WithMessage("Due date must be on or after the issue date");

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => new[] { "Draft", "Sent", "Paid", "Overdue" }.Contains(s))
            .WithMessage("Invalid invoice status");

        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency must be a 3-letter ISO code (e.g., USD)");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
