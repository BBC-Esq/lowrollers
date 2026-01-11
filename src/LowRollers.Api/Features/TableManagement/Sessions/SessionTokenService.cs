using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace LowRollers.Api.Features.TableManagement.Sessions;

/// <summary>
/// Simple session token service using HMAC-SHA256 signed tokens.
/// TODO: Replace with proper JWT implementation for production.
/// </summary>
public sealed partial class SessionTokenService : ISessionTokenService
{
    private readonly byte[] _signingKey;
    private readonly ILogger<SessionTokenService> _logger;
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(24);

    public SessionTokenService(IConfiguration configuration, ILogger<SessionTokenService> logger)
    {
        _logger = logger;

        // Get signing key from configuration or generate one
        var keyString = configuration["SessionToken:SigningKey"];
        if (string.IsNullOrEmpty(keyString))
        {
            // Generate a random key for development - in production this should be configured
            _signingKey = RandomNumberGenerator.GetBytes(32);
            LogSigningKeyGenerated();
        }
        else
        {
            _signingKey = Convert.FromBase64String(keyString);
        }
    }

    /// <inheritdoc />
    public string GenerateToken(Guid sessionId, Guid playerId, string displayName)
    {
        var now = DateTimeOffset.UtcNow;
        var claims = new SessionTokenClaims(
            sessionId,
            playerId,
            displayName,
            now,
            now.Add(TokenLifetime));

        var payload = JsonSerializer.Serialize(claims);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var payloadBase64 = Convert.ToBase64String(payloadBytes);

        var signature = ComputeSignature(payloadBytes);
        var signatureBase64 = Convert.ToBase64String(signature);

        return $"{payloadBase64}.{signatureBase64}";
    }

    /// <inheritdoc />
    public SessionTokenClaims? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var parts = token.Split('.');
        if (parts.Length != 2)
        {
            LogInvalidTokenFormat();
            return null;
        }

        try
        {
            var payloadBytes = Convert.FromBase64String(parts[0]);
            var providedSignature = Convert.FromBase64String(parts[1]);

            var expectedSignature = ComputeSignature(payloadBytes);
            if (!CryptographicOperations.FixedTimeEquals(expectedSignature, providedSignature))
            {
                LogInvalidTokenSignature();
                return null;
            }

            var payload = Encoding.UTF8.GetString(payloadBytes);
            var claims = JsonSerializer.Deserialize<SessionTokenClaims>(payload);

            if (claims == null)
            {
                LogInvalidTokenPayload();
                return null;
            }

            if (claims.ExpiresAt < DateTimeOffset.UtcNow)
            {
                LogTokenExpired(claims.PlayerId);
                return null;
            }

            return claims;
        }
        catch (Exception ex)
        {
            LogTokenValidationError(ex.Message);
            return null;
        }
    }

    private byte[] ComputeSignature(byte[] payload)
    {
        using var hmac = new HMACSHA256(_signingKey);
        return hmac.ComputeHash(payload);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Session token signing key not configured, generated random key")]
    private partial void LogSigningKeyGenerated();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid token format")]
    private partial void LogInvalidTokenFormat();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid token signature")]
    private partial void LogInvalidTokenSignature();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid token payload")]
    private partial void LogInvalidTokenPayload();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Token expired for player {PlayerId}")]
    private partial void LogTokenExpired(Guid playerId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Token validation error: {Error}")]
    private partial void LogTokenValidationError(string error);
}
