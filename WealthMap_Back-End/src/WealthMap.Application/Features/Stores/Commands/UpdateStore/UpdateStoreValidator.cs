using FluentValidation;

namespace WealthMap.Application.Features.Stores.Commands.UpdateStore;

public class UpdateStoreValidator : AbstractValidator<UpdateStoreCommand>
{
    public UpdateStoreValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Store name is required.")
            .MaximumLength(120);

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(80);

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500)
            .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("Logo URL must be a valid absolute URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl));

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => x.Description is not null);
    }
}