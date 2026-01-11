using LowRollers.Api.Features.TableManagement.Models;

namespace LowRollers.Api.Features.TableManagement;

/// <summary>
/// Service for managing poker tables - creation, joining, and state management
/// </summary>
public interface ITableManagementService
{
    /// <summary>
    /// Creates a new table with the host as the first player
    /// </summary>
    /// <param name="request">The create table request</param>
    /// <param name="baseUrl">Base URL for generating invite links</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing table info and invite code, or error</returns>
    Task<TableManagementResult<CreateTableResponse>> CreateTableAsync(
        CreateTableRequest request,
        string baseUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current state of a table
    /// </summary>
    /// <param name="sessionId">The session ID</param>
    /// <param name="playerId">The requesting player's ID (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The table state or error</returns>
    Task<TableManagementResult<TableStateResponse>> GetTableStateAsync(
        Guid sessionId,
        Guid playerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an invite code and returns basic table info
    /// </summary>
    /// <param name="inviteCode">The invite code to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with table info if valid</returns>
    Task<TableManagementResult<InviteValidationResponse>> ValidateInviteCodeAsync(
        string inviteCode,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Factory methods for creating TableManagementResult instances
/// </summary>
public static class TableManagementResult
{
    public static TableManagementResult<T> Success<T>(T data) => new(data);
    public static TableManagementResult<T> Failure<T>(string message, TableManagementErrorCode code) => new(message, code);
}

/// <summary>
/// Result wrapper for table management operations
/// </summary>
/// <typeparam name="T">The result data type</typeparam>
public sealed class TableManagementResult<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? ErrorMessage { get; }
    public TableManagementErrorCode? ErrorCode { get; }

    internal TableManagementResult(T data)
    {
        IsSuccess = true;
        Data = data;
    }

    internal TableManagementResult(string errorMessage, TableManagementErrorCode errorCode)
    {
        IsSuccess = false;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Error codes for table management operations
/// </summary>
public enum TableManagementErrorCode
{
    InvalidRequest,
    InvalidDisplayName,
    DisplayNameTaken,
    DisplayNameBanned,
    InvalidInviteCode,
    TableNotFound,
    SessionNotFound,
    NotAuthorized,
    TableFull,
    GameInProgress,
    InternalError
}

/// <summary>
/// Response from validating an invite code
/// </summary>
public sealed class InviteValidationResponse
{
    public Guid SessionId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public int CurrentPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public bool RequiresPassword { get; set; }
    public bool GameInProgress { get; set; }
}
