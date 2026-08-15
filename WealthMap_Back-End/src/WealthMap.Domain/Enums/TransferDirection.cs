namespace WealthMap.Domain.Enums;

/// <summary>Which way money moved, from the user's point of view.</summary>
public enum TransferDirection
{
    /// <summary>Money arriving.</summary>
    Inbound = 1,

    /// <summary>Money leaving.</summary>
    Outbound = 2
}
