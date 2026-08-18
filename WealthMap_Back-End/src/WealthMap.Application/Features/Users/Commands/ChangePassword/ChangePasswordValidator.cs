using FluentValidation;

namespace WealthMap.Application.Features.Users.Commands.ChangePassword;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Enter your current password.");

        // The same floor registration uses. A change that could weaken the
        // password below what signing up allows would be a strange door to leave.
        RuleFor(x => x.NewPassword)
            .NotEmpty().MinimumLength(8)
            .WithMessage("Password must be at least 8 characters.");
    }
}
