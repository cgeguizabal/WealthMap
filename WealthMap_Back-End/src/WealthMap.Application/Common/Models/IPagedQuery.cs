using FluentValidation;

namespace WealthMap.Application.Common.Models;

/// <summary>Every paged query takes the same two parameters with the same limits.</summary>
public interface IPagedQuery
{
    int Page { get; }
    int PageSize { get; }
}

public static class PagedQueryRules
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    /// <summary>Call from a validator's constructor so every paged endpoint validates alike.</summary>
    public static void ApplyPagingRules<T>(this AbstractValidator<T> validator)
        where T : IPagedQuery
    {
        validator.RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be 1 or greater.");

        validator.RuleFor(x => x.PageSize)
            .InclusiveBetween(1, MaxPageSize)
            .WithMessage($"Page size must be between 1 and {MaxPageSize}.");
    }
}
