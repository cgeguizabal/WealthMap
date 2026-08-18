using WealthMap.Domain.Common;
using WealthMap.Domain.Exceptions;

namespace WealthMap.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string FullName { get; private set; }
    public string Country { get; private set; }
    public string Currency { get; private set; }

    /// <summary>When this user accepted the Terms and Privacy Policy, if they have.</summary>
    /// <remarks>
    /// Nullable because every account created before consent was collected has no
    /// answer, and inventing one would be a false record of something legal.
    /// </remarks>
    public DateTime? TermsAcceptedAt { get; private set; }

    /// <summary>The version they accepted, so a later revision can be re-consented.</summary>
    public string? AcceptedPolicyVersion { get; private set; }

    private User()
{
    Email = null!;
    PasswordHash = null!;
    FullName = null!;
    Country = null!;
    Currency = null!;
}  // required by EF Core

    public User(string email, string passwordHash, string fullName, string country, string currency)
    {
        Email = NormalizeEmail(email);
        PasswordHash = !string.IsNullOrWhiteSpace(passwordHash)
            ? passwordHash
            : throw new DomainException("Password hash is required.");
        FullName = ValidateName(fullName);
        Country = ValidateCountry(country);
        Currency = ValidateCurrency(currency);
    }

    public void UpdateProfile(string fullName, string country)
    {
        FullName = ValidateName(fullName);
        Country = ValidateCountry(country);
        Touch();
    }

    /// <summary>Records that this user accepted a given version of the terms.</summary>
    /// <remarks>
    /// The version is stored alongside the timestamp because "they agreed" is not
    /// a useful record on its own — a revised policy has to be re-accepted, and
    /// that comparison needs to know which text they saw.
    /// </remarks>
    public void AcceptTerms(string version, DateTime acceptedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(version))
            throw new DomainException("Policy version is required.");

        if (acceptedAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException("Terms acceptance date must be UTC.");

        AcceptedPolicyVersion = version.Trim();
        TermsAcceptedAt = acceptedAtUtc;
        Touch();
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Password hash is required.");

        PasswordHash = newPasswordHash;
        Touch();
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("A valid email is required.");

        return email.Trim().ToLowerInvariant();
    }

    private static string ValidateName(string name) =>
        !string.IsNullOrWhiteSpace(name)
            ? name.Trim()
            : throw new DomainException("Full name is required.");

    private static string ValidateCountry(string country) =>
        !string.IsNullOrWhiteSpace(country)
            ? country.Trim()
            : throw new DomainException("Country is required.");

    private static string ValidateCurrency(string currency) =>
        !string.IsNullOrWhiteSpace(currency) && currency.Length == 3
            ? currency.ToUpperInvariant()
            : throw new DomainException("Currency must be a 3-letter ISO code.");
}