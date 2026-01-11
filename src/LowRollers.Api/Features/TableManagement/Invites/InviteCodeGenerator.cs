using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace LowRollers.Api.Features.TableManagement.Invites;

/// <summary>
/// Generates and validates cryptographically secure invite codes
/// </summary>
public sealed partial class InviteCodeGenerator : IInviteCodeGenerator
{
    private const int CodeLength = 8;
    private const string AllowedCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Excluding 0, O, 1, I to avoid confusion
    private readonly ILogger<InviteCodeGenerator> _logger;

    public InviteCodeGenerator(ILogger<InviteCodeGenerator> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public (string Code, string Hash) GenerateInviteCode()
    {
        var code = GenerateSecureCode();
        var hash = HashInviteCode(code);

        LogCodeGenerated(code[..4]); // Only log first 4 chars for security

        return (code, hash);
    }

    /// <inheritdoc />
    public bool VerifyInviteCode(string code, string hash)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        var codeHash = HashInviteCode(code.ToUpperInvariant());
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(codeHash),
            Encoding.UTF8.GetBytes(hash));
    }

    /// <inheritdoc />
    public string HashInviteCode(string code)
    {
        // Use SHA256 for fast hashing - invite codes are short-lived and don't need bcrypt/argon2
        var bytes = Encoding.UTF8.GetBytes(code.ToUpperInvariant());
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    private static string GenerateSecureCode()
    {
        Span<byte> randomBytes = stackalloc byte[CodeLength];
        RandomNumberGenerator.Fill(randomBytes);

        var chars = new char[CodeLength];
        for (int i = 0; i < CodeLength; i++)
        {
            chars[i] = AllowedCharacters[randomBytes[i] % AllowedCharacters.Length];
        }

        return new string(chars);
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Generated invite code starting with {CodePrefix}****")]
    private partial void LogCodeGenerated(string codePrefix);
}
