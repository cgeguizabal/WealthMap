namespace WealthMap.Domain.Enums;

/// <summary>
/// How transactions reach an account or card.
/// </summary>
/// <remarks>
/// <see cref="EmailSync"/> is reserved. The ingestion that would populate it is
/// deliberately not built yet; the mode exists so instruments can be identified
/// and opted in ahead of it. See "Planned: automatic transaction sync" in
/// docs/PROJECT_GUIDE.md.
/// </remarks>
public enum TrackingMode
{
    /// <summary>The user enters everything by hand. The only mode that does anything today.</summary>
    Manual = 1,

    /// <summary>Reserved: future automatic ingestion from bank notification emails.</summary>
    EmailSync = 2
}
