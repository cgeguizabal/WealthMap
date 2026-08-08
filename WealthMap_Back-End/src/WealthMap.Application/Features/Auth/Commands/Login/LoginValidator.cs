using FluentValidation;

namespace WealthMap.Application.Features.Auth.Commands.Login;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        // Shape only — a wrong email or password still fails in the handler with the
        // same generic message, so this never reveals whether an account exists.
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
