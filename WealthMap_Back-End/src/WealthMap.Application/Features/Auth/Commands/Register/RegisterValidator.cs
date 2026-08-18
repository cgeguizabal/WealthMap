using FluentValidation;

namespace WealthMap.Application.Features.Auth.Commands.Register;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .WithMessage("Password must be at least 8 characters.");
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Currency).NotEmpty().Length(3);

        // Consent is recorded, not assumed. A client that omits the flag fails here
        // rather than creating an account with no acceptance on file.
        RuleFor(x => x.AcceptedTerms).Equal(true)
            .WithMessage("You must accept the Terms of Service and Privacy Policy.");
        RuleFor(x => x.PolicyVersion).NotEmpty().MaximumLength(20);
    }
}