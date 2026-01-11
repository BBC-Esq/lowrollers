namespace LowRollers.Api.Data.Entities;

public class SessionHand
{
    public Guid HandId { get; set; }
    public Guid SessionId { get; set; }
    public byte[] ShuffleSeed { get; set; } = [];
    public DateTimeOffset StartedOn { get; set; }
    public DateTimeOffset? EndedOn { get; set; }

    public GameSession GameSession { get; set; } = null!;
    public ICollection<SessionHandEvent> SessionHandEvents { get; set; } = [];
}
