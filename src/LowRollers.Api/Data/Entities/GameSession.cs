namespace LowRollers.Api.Data.Entities;

public class GameSession
{
    public Guid SessionId { get; set; }
    public Guid TableId { get; set; }
    public DateTimeOffset StartedOn { get; set; }
    public DateTimeOffset? EndedOn { get; set; }
    public string? TableConfigOverride { get; set; }
    public string InviteCodeHash { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }

    public GameTable GameTable { get; set; } = null!;
    public ICollection<GameSessionPlayer> GameSessionPlayers { get; set; } = [];
    public ICollection<SessionTransaction> SessionTransactions { get; set; } = [];
    public ICollection<SessionHand> SessionHands { get; set; } = [];
}
