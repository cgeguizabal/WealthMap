using FluentValidation;

namespace WealthMap.Application.Features.Jobs.Commands.CreateJob;

public class CreateJobValidator : AbstractValidator<CreateJobCommand>
{
    public CreateJobValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Job title is required.")
            .MaximumLength(120);

        RuleFor(x => x.Employer)
            .NotEmpty().WithMessage("Employer is required.")
            .MaximumLength(120);

        RuleFor(x => x.GrossMonthlySalary)
            .GreaterThan(0).WithMessage("Gross monthly salary must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.");

        RuleFor(x => x.DepositAccountId)
            .NotEmpty().WithMessage("Deposit account is required.");

        RuleFor(x => x.PaymentDays)
            .NotEmpty().WithMessage("At least one payment day is required.")
            .Must(d => d.Distinct().Count() is >= 1 and <= 3)
            .WithMessage("A job must have between 1 and 3 distinct payment days.")
            .ForEach(day => day.InclusiveBetween(1, 31).WithMessage("Payment days must be between 1 and 31."));
    }
}