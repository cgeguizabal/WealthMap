using FluentValidation;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.Jobs.Commands.UpdateDeduction;

public class UpdateDeductionValidator : AbstractValidator<UpdateDeductionCommand>
{
    public UpdateDeductionValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Deduction name is required.")
            .MaximumLength(120);

        RuleFor(x => x.Type)
            .Must(t => Enum.IsDefined(typeof(DeductionType), t))
            .WithMessage("Type must be 1 (Fixed) or 2 (Percentage).");

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("Deduction value must be greater than zero.");

        RuleFor(x => x.Value)
            .LessThanOrEqualTo(100).WithMessage("Percentage deduction cannot exceed 100.")
            .When(x => x.Type == (int)DeductionType.Percentage);
    }
}