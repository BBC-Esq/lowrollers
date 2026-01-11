namespace LowRollers.Api.Features.TableManagement.Models;

/// <summary>
/// Response from creating a table
/// </summary>
public sealed class CreateTableResponse
{
    /// <summary>
    /// The table ID
    /// </summary>
    public Guid TableId { get; set; }

    /// <summary>
    /// The session ID for the active game session
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// The invite code to share with players
    /// </summary>
    public string InviteCode { get; set; } = string.Empty;

    /// <summary>
    /// Full invite URL
    /// </summary>
    public string InviteUrl { get; set; } = string.Empty;

    /// <summary>
    /// Session token for the host (HMAC-signed, base64 encoded)
    /// </summary>
    public string SessionToken { get; set; } = string.Empty;

    /// <summary>
    /// The host's player ID
    /// </summary>
    public Guid PlayerId { get; set; }
}
