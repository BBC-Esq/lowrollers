namespace LowRollers.Api.Data.Entities;

public class GameSessionPlayer
{
    public Guid SessionId { get; set; }
    public Guid PlayerId { get; set; }
    public DateTimeOffset SeatedOn { get; set; }
    public DateTimeOffset? DepartedOn { get; set; }
    public byte SeatNumber { get; set; }
    public int TimeBankSeconds { get; set; }
    public int ChipStack { get; set; }
    public bool IsHost { get; set; }

    public GameSession GameSession { get; set; } = null!;
    public Player Player { get; set; } = null!;
}
