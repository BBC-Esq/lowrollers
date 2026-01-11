namespace LowRollers.Api.Features.TableManagement.Invites;

/// <summary>
/// Service for generating and validating cryptographically secure invite codes
/// </summary>
public interface IInviteCodeGenerator
{
    /// <summary>
    /// Generates a new cryptographically secure invite code
    /// </summary>
    /// <returns>A tuple containing the plain invite code and its hash</returns>
    (string Code, string Hash) GenerateInviteCode();

    /// <summary>
    /// Verifies an invite code against a stored hash
    /// </summary>
    /// <param name="code">The plain invite code to verify</param>
    /// <param name="hash">The stored hash to verify against</param>
    /// <returns>True if the code matches the hash</returns>
    bool VerifyInviteCode(string code, string hash);

    /// <summary>
    /// Hashes an invite code for storage
    /// </summary>
    /// <param name="code">The plain invite code</param>
    /// <returns>The hashed code</returns>
    string HashInviteCode(string code);
}
