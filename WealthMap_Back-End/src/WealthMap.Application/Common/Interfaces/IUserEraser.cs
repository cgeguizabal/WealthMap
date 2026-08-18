namespace WealthMap.Application.Common.Interfaces;

/// <summary>
/// Removes a user and everything belonging to them.
/// </summary>
/// <remarks>
/// An interface because the *order* the rows must be deleted in is a fact about
/// the database schema, and the Application layer is not allowed to know it. The
/// handler asks for the account to be erased; Infrastructure knows what that
/// costs.
/// </remarks>
public interface IUserEraser
{
    /// <summary>
    /// Deletes everything, or throws and leaves the account untouched. Call
    /// inside a transaction.
    /// </summary>
    Task EraseAsync(Guid userId, CancellationToken ct = default);
}
