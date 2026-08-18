using FluentValidation;

namespace WealthMap.Application.Features.FreelanceJobs.Commands.UpdateFreelanceJob;

public class UpdateFreelanceJobValidator : AbstractValidator<UpdateFreelanceJobCommand>
{
    public UpdateFreelanceJobValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("A description of the work is required.")
            .MaximumLength(200);

        RuleFor(x => x.AgreedAmount)
            .GreaterThan(0).WithMessage("Agreed amount must be greater than zero.");

        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Client).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
