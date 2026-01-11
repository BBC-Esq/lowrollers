namespace LowRollers.Api.Data.Entities;

public class SessionHandEvent
{
    public Guid HandDetailId { get; set; }
    public Guid SessionId { get; set; }
    public Guid HandId { get; set; }
    public Guid? PlayerId { get; set; }
    public byte EventTypeId { get; set; }
    public DateTimeOffset EventTimestamp { get; set; }
    public int? Amount { get; set; }
    public string? EventDetails { get; set; }

    public SessionHand SessionHand { get; set; } = null!;
    public Player? Player { get; set; }
}
