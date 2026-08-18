using FluentValidation;

namespace WealthMap.Application.Features.FreelanceJobs.Commands.MarkFreelanceJobPaid;

public class MarkFreelanceJobPaidValidator : AbstractValidator<MarkFreelanceJobPaidCommand>
{
    public MarkFreelanceJobPaidValidator()
    {
        RuleFor(x => x.AmountPaid)
            .GreaterThan(0).WithMessage("Payment amount must be greater than zero.");

        RuleFor(x => x.DepositAccountId)
            .NotEmpty().WithMessage("Choose the account the payment landed in.");
    }
}
