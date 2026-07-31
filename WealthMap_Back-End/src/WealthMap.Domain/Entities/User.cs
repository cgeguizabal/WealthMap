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
    public DateOnly? BirthDate { get; private set; }

    private User() { }   // required by EF Core

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

    public void UpdateProfile(string fullName, string country, DateOnly? birthDate)
    {
        FullName = ValidateName(fullName);
        Country = ValidateCountry(country);

        if (birthDate.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (birthDate.Value > today)
                throw new DomainException("Birth date cannot be in the future.");
            if (birthDate.Value < today.AddYears(-120))
                throw new DomainException("Birth date is not valid.");
        }

        BirthDate = birthDate;
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