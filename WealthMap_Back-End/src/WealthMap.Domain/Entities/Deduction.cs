using WealthMap.Domain.Common;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;

namespace WealthMap.Domain.Entities;

/// <summary>
/// A payslip deduction the user declares (the app does arithmetic, not tax law).
/// Value is an amount for Fixed, a percent of gross for Percentage.
/// </summary>
public class Deduction : BaseEntity
{
    public Guid JobId { get; private set; }
    public string Name { get; private set; }
    public DeductionType Type { get; private set; }
    public decimal Value { get; private set; }

    private Deduction()
    {
        Name = null!;
    }

    internal Deduction(Guid jobId, string name, DeductionType type, decimal value)
    {
        JobId = jobId;
        Name = ValidateName(name);
        Type = ValidateType(type);
        Value = ValidateValue(type, value);
    }

    internal void Update(string name, DeductionType type, decimal value)
    {
        Name = ValidateName(name);
        Type = ValidateType(type);
        Value = ValidateValue(type, value);
        Touch();
    }

    private static string ValidateName(string name) =>
        !string.IsNullOrWhiteSpace(name)
            ? name.Trim()
            : throw new DomainException("Deduction name is required.");

    private static DeductionType ValidateType(DeductionType type) =>
        Enum.IsDefined(type)
            ? type
            : throw new DomainException("Deduction type must be Fixed or Percentage.");

    private static decimal ValidateValue(DeductionType type, decimal value)
    {
        if (value <= 0)
            throw new DomainException("Deduction value must be greater than zero.");

        if (type == DeductionType.Percentage && value > 100)
            throw new DomainException("Percentage deduction cannot exceed 100.");

        return value;
    }
}