namespace WealthMap.Domain.Enums;

/// <summary>
/// Where a piece of freelance work has got to.
/// </summary>
/// <remarks>
/// Computed from the dates on <see cref="Entities.FreelanceJob"/> rather than
/// stored. The dates are the facts — when it was delivered, when it was paid,
/// when it was cancelled — and the status is a reading of them. Storing both
/// would let them disagree, and then neither could be trusted.
/// </remarks>
public enum FreelanceJobStatus
{
    /// <summary>Agreed and under way. Nothing delivered yet.</summary>
    InProgress = 1,

    /// <summary>Finished and handed over, but the money has not arrived.</summary>
    Delivered = 2,

    /// <summary>Paid. The money is in an account and counts like any other balance.</summary>
    Paid = 3,

    /// <summary>Called off. Kept rather than deleted, so the history stays honest.</summary>
    Cancelled = 4
}
