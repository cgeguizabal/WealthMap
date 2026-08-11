using WealthMap.Domain.Exceptions;

namespace WealthMap.Domain.ValueObjects;

public readonly record struct Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new DomainException("Currency must be a 3-letter ISO code (e.g. USD).");

        // Half away from zero, the rounding people check against a payslip:
        // 418.525 is 418.53. Banker's rounding would make it 418.52, because it
        // breaks ties toward the even digit — correct for avoiding drift over
        // many roundings, but not what anyone reading a balance expects.
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero(string currency) => new(0m, currency);

    public bool IsNegative => Amount < 0;
    public bool IsZero => Amount == 0;

    public static Money operator +(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static bool operator >(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return a.Amount > b.Amount;
    }

    public static bool operator <(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return a.Amount < b.Amount;
    }

    public static bool operator >=(Money a, Money b) => a > b || a == b;
    public static bool operator <=(Money a, Money b) => a < b || a == b;

    private static void EnsureSameCurrency(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new DomainException(
                $"Cannot operate on different currencies: {a.Currency} and {b.Currency}.");
    }

    public override string ToString() => $"{Amount:N2} {Currency}";
}