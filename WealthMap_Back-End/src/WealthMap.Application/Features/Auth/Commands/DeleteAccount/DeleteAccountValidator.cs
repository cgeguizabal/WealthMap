using FluentValidation;

namespace WealthMap.Application.Features.Auth.Commands.DeleteAccount;

public class DeleteAccountValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Enter your password to delete your account.");
    }
}
