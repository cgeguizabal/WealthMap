using WealthMap.Domain.Common;
using WealthMap.Domain.Exceptions;

namespace WealthMap.Domain.Entities;

/// <summary>
/// Shared catalog entry — deliberately NOT user-scoped. CreatedByUserId is null
/// for system stores; user-created stores are editable only by their creator.
/// </summary>
public class Store : BaseEntity
{
    public Guid? CreatedByUserId { get; private set; }
    public string Name { get; private set; }
    public string Category { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? Description { get; private set; }

    private Store()
    {
        Name = null!;
        Category = null!;
    }

    public Store(Guid? createdByUserId, string name, string category, string? logoUrl, string? description)
    {
        CreatedByUserId = createdByUserId;
        Name = ValidateText(name, "Store name");
        Category = ValidateText(category, "Category");
        LogoUrl = Normalize(logoUrl);
        Description = Normalize(description);
    }

    public bool IsOwnedBy(Guid userId) => CreatedByUserId == userId;

    public void UpdateDetails(string name, string category, string? logoUrl, string? description)
    {
        Name = ValidateText(name, "Store name");
        Category = ValidateText(category, "Category");
        LogoUrl = Normalize(logoUrl);
        Description = Normalize(description);
        Touch();
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string ValidateText(string value, string field) =>
        !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : throw new DomainException($"{field} is required.");
}