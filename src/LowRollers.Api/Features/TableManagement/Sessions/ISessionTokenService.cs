namespace LowRollers.Api.Features.TableManagement.Sessions;

/// <summary>
/// Service for generating and validating session tokens
/// </summary>
public interface ISessionTokenService
{
    /// <summary>
    /// Generates a session token for a player
    /// </summary>
    /// <param name="sessionId">The game session ID</param>
    /// <param name="playerId">The player ID</param>
    /// <param name="displayName">The player's display name</param>
    /// <returns>A session token string</returns>
    string GenerateToken(Guid sessionId, Guid playerId, string displayName);

    /// <summary>
    /// Validates and extracts claims from a session token
    /// </summary>
    /// <param name="token">The token to validate</param>
    /// <returns>The token claims if valid, null otherwise</returns>
    SessionTokenClaims? ValidateToken(string token);
}

/// <summary>
/// Claims contained in a session token
/// </summary>
public sealed record SessionTokenClaims(
    Guid SessionId,
    Guid PlayerId,
    string DisplayName,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt);
